using CrucibleAI.Text;

namespace CrucibleAI.Judging;

internal static class JudgeReplyText
{
    private const string Fence = "```";

    public static string Unwrap(string reply)
    {
        var text = EcmaScriptTrim.Trim(reply);
        if (text.StartsWith(Fence, StringComparison.Ordinal))
        {
            var index = Fence.Length;
            while (index < text.Length && char.IsAsciiLetter(text[index]))
            {
                index++;
            }
            while (index < text.Length && EcmaScriptTrim.IsWhiteSpace(text[index]))
            {
                index++;
            }
            text = text[index..];
        }
        var end = EcmaScriptTrim.Trim(text);
        if (end.EndsWith(Fence, StringComparison.Ordinal))
        {
            text = end[..^Fence.Length];
        }
        return EcmaScriptTrim.Trim(text);
    }
}
