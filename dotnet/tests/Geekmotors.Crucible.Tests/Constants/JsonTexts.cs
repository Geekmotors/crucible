namespace Geekmotors.Crucible.Tests.Constants;

internal static class JsonTexts
{
    public const string WithComment = "{ \"a\": 1 // note\n}";
    public const string WithTrailingComma = "[1,]";
    public const string WithByteOrderMark = "\uFEFF[1]";
    public const string Overflowing = "[1e999]";
    public const string DuplicateKeys = """{ "a": 1, "b": 2, "a": 3 }""";
    public const string FirstKey = "a";
    public const double LastDuplicateValue = 3;
    public const string ErrorAfterMixedLineBreaks = "\r\r\n\r!";
    public const int MixedLineBreaksErrorLine = 4;
    public const int NestingLimit = 512;
    public const char OpenArray = '[';
    public const char CloseArray = ']';
}
