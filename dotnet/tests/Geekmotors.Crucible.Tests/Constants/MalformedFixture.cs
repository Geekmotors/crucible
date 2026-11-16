namespace Geekmotors.Crucible.Tests.Constants;

internal static class MalformedFixture
{
    public const string FileName = "spec/verdict/compute.json";
    public const string OtherFileName = "spec/verdict/compute-extra.json";

    public const string Valid = """
        { "area": "verdict", "operation": "compute", "description": "d",
          "cases": [{ "id": "verdict-compute-one", "description": "d",
                      "input": { "threshold": 1, "statuses": { "passed": 1 } },
                      "expect": { "value": { "outcome": "passed" } } }] }
        """;

    public const string MistypedInput = """
        { "area": "verdict", "operation": "compute", "description": "d",
          "cases": [{ "id": "verdict-compute-one", "description": "d",
                      "input": { "threshold": "high", "statuses": { "passed": 1 } },
                      "expect": { "value": { "outcome": "passed" } } }] }
        """;

    public const string WrongPrefix = """
        { "area": "verdict", "operation": "compute", "description": "d",
          "cases": [{ "id": "verdict-tally-one", "description": "d",
                      "input": { "threshold": 1, "statuses": { "passed": 1 } },
                      "expect": { "value": { "outcome": "passed" } } }] }
        """;

    public const string CaseWithoutId = """
        { "area": "verdict", "operation": "compute", "description": "d",
          "cases": [{ "description": "d",
                      "input": { "threshold": 1, "statuses": { "passed": 1 } },
                      "expect": { "value": { "outcome": "passed" } } }] }
        """;

    public const string UnknownToken = """{ "runs": { "$special": "Huge" } }""";

    public const string UnsetField = """{ "runs": { "$special": "undefined" }, "threshold": 1 }""";

    public const string NotANumber = """{ "$special": "NaN" }""";
}
