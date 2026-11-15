using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using CrucibleAI.Engine;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace CrucibleAI.Xunit;

internal sealed class TestClassInstances(XunitTestRunnerContext ctxt)
{
    private Exception? _skip;

    public Exception? Skip => Volatile.Read(ref _skip);

    public async Task<RunBody> CreateAsync()
    {
        RuntimeHelpers.RunClassConstructor(ctxt.Test.TestMethod.TestClass.Class.TypeHandle);
        var instance = ctxt.Method.IsStatic ? null : Construct();
        try
        {
            if (instance is IAsyncLifetime lifetime)
            {
                await lifetime.InitializeAsync().ConfigureAwait(false);
            }
        }
        catch (Exception failure)
        {
            await DisposeAfterAsync(instance, failure).ConfigureAwait(false);
            throw;
        }
        return new RunBody(_ => ObserveAsync(instance), () => DisposeAsync(instance));
    }

    private object Construct()
    {
        var type = ctxt.Test.TestMethod.TestClass.Class;
        var constructors = type.GetConstructors().Where(candidate => !candidate.IsStatic && candidate.IsPublic).ToArray();
        if (constructors.Length != 1)
        {
            throw new TestPipelineException("A test class may only define a single public constructor.");
        }
        var parameters = constructors[0].GetParameters();
        var arguments = ctxt.ConstructorArguments.Select((argument, index) => Unwrap(argument, parameters[index].ParameterType)).ToArray();
        try
        {
            return TypeActivator.Current.CreateInstance(
                constructors[0],
                arguments,
                (_, missing) => string.Format(
                    CultureInfo.CurrentCulture,
                    "The following constructor parameters did not have matching fixture data: {0}",
                    string.Join(", ", missing.Select(parameter => parameter.ParameterType.Name + " " + parameter.Name))));
        }
        catch (TargetInvocationException failure) when (failure.InnerException is { } inner)
        {
            ExceptionDispatchInfo.Throw(inner);
            throw;
        }
    }

    private static object? Unwrap(object? argument, Type parameterType) =>
        argument is not null
        && argument.GetType() != parameterType
        && argument.GetType() == typeof(Func<>).MakeGenericType(parameterType)
            ? ((Delegate)argument).DynamicInvoke()
            : argument;

    private async Task ObserveAsync(object? instance)
    {
        try
        {
            await Invoke(instance).ConfigureAwait(false);
        }
        catch (Exception failure) when (IsSkip(failure))
        {
            Interlocked.CompareExchange(ref _skip, failure, null);
            throw;
        }
    }

    private bool IsSkip(Exception failure) =>
        failure.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal)
        || ctxt.Test.TestCase.SkipExceptions?.Contains(failure.GetType()) == true;

    private Task Invoke(object? instance)
    {
        var result = ctxt.Method.Invoke(instance, BindingFlags.DoNotWrapExceptions, null, ctxt.MethodArguments, CultureInfo.InvariantCulture);
        return result switch
        {
            Task task => task,
            ValueTask valueTask => valueTask.AsTask(),
            not null when result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(ValueTask<>) =>
                (Task)result.GetType().GetMethod(nameof(ValueTask<object>.AsTask))!.Invoke(result, null)!,
            _ => Task.CompletedTask,
        };
    }

    private static async ValueTask DisposeAfterAsync(object? instance, Exception failure)
    {
        try
        {
            await DisposeAsync(instance).ConfigureAwait(false);
        }
        catch (Exception disposal)
        {
            throw new AggregateException(failure, disposal);
        }
    }

    private static async ValueTask DisposeAsync(object? instance)
    {
        if (instance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
