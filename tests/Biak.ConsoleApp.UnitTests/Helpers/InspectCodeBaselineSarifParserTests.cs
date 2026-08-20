// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class InspectCodeBaselineSarifParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseReturnsEmptyForNullOrWhiteSpace(string? input)
    {
        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(input!);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseReturnsEmptyWhenNoRunsProperty()
    {
        // language=json
        const string SARIF = """{ "version": "2.1.0" }""";

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseReturnsEmptyWhenRunsIsEmpty()
    {
        // language=json
        const string SARIF = """{ "runs": [] }""";

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseReturnsEmptyWhenNoResults()
    {
        // language=json
        const string SARIF = """{ "runs": [{ "results": [] }] }""";

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseReturnsSingleIssue()
    {
        // language=json
        const string SARIF = """
            {
              "runs": [{
                "results": [{
                  "ruleId": "ConvertToConstant.Local",
                  "locations": [{
                    "physicalLocation": {
                      "artifactLocation": {
                        "uri": "src/MyClass.cs"
                      }
                    }
                  }]
                }]
              }]
            }
            """;

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Single(result);
        Assert.Equal("ConvertToConstant.Local", result[0].RuleId);
        Assert.Equal("src/MyClass.cs", result[0].FilePath);
    }

    [Fact]
    public void ParseReturnsMultipleIssuesFromMultipleResults()
    {
        // language=json
        const string SARIF = """
            {
              "runs": [{
                "results": [
                  {
                    "ruleId": "ConvertToConstant.Local",
                    "locations": [{
                      "physicalLocation": { "artifactLocation": { "uri": "src/ServiceA.cs" } }
                    }]
                  },
                  {
                    "ruleId": "MemberCanBePrivate.Global",
                    "locations": [{
                      "physicalLocation": { "artifactLocation": { "uri": "src/ServiceB.cs" } }
                    }]
                  }
                ]
              }]
            }
            """;

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.RuleId == "ConvertToConstant.Local" && x.FilePath == "src/ServiceA.cs");
        Assert.Contains(result, x => x.RuleId == "MemberCanBePrivate.Global" && x.FilePath == "src/ServiceB.cs");
    }

    [Fact]
    public void ParseReturnsSameRuleForMultipleLocations()
    {
        // language=json
        const string SARIF = """
            {
              "runs": [{
                "results": [{
                  "ruleId": "UseArrayEmptyMethod",
                  "locations": [
                    { "physicalLocation": { "artifactLocation": { "uri": "src/ServiceD.cs" } } },
                    { "physicalLocation": { "artifactLocation": { "uri": "src/ServiceE.cs" } } }
                  ]
                }]
              }]
            }
            """;

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal("UseArrayEmptyMethod", x.RuleId));
        Assert.Contains(result, x => x.FilePath == "src/ServiceD.cs");
        Assert.Contains(result, x => x.FilePath == "src/ServiceE.cs");
    }

    [Fact]
    public void ParseUsesOnlyFirstRun()
    {
        // language=json
        const string SARIF = """
            {
              "runs": [
                {
                  "results": [{
                    "ruleId": "ConvertToConstant.Local",
                    "locations": [{ "physicalLocation": { "artifactLocation": { "uri": "src/First.cs" } } }]
                  }]
                },
                {
                  "results": [{
                    "ruleId": "MemberCanBePrivate.Global",
                    "locations": [{ "physicalLocation": { "artifactLocation": { "uri": "src/Second.cs" } } }]
                  }]
                }
              ]
            }
            """;

        IReadOnlyList<InspectCodeIssue> result = InspectCodeBaselineSarifParser.Parse(SARIF);

        Assert.Single(result);
        Assert.Equal("ConvertToConstant.Local", result[0].RuleId);
        Assert.Equal("src/First.cs", result[0].FilePath);
    }
}
