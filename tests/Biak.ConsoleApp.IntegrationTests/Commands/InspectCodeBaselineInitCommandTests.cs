// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class InspectCodeBaselineInitCommandTests
{
#pragma warning disable IDE1006 // Naming Styles
    private static readonly string TEST_OUTPUT = InspectCodeBaselineInitCommandConstant.INIT_STARTED
#pragma warning restore IDE1006 // Naming Styles
        + Environment.NewLine
        + InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE
        + Environment.NewLine
        + InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS;

    [Fact]
    public async Task RunShouldOutputBaselineToConsoleAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunShouldOutputBaselineToConsoleAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templatePath = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "InspectCodeBaseline",
                "InspectCodeBaselineTemplate"
            );

            testDir.CopyDirectory(templatePath);

            string result = await InspectCodeBaselineInitCommand.RunAsync();

            string actualOutput = output.ToString().Trim();

            Assert.Equal(InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS, result.Trim());
            Assert.Equal(TEST_OUTPUT, actualOutput);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenNoIssuesFoundShouldReturnNoIssuesFoundMessageAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenNoIssuesFoundShouldReturnNoIssuesFoundMessageAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templatePath = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "InspectCodeBaseline",
                "InspectCodeBaselineNoIssuesTemplate"
            );

            testDir.CopyDirectory(templatePath);

            string result = await InspectCodeBaselineInitCommand.RunAsync();

            string actualOutput = output.ToString().Trim();

            Assert.Equal(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, result);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.INIT_STARTED, actualOutput, StringComparison.Ordinal);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, actualOutput, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenUnmappedRuleDetectedShouldOutputWarningToConsoleAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenUnmappedRuleDetectedShouldOutputWarningToConsoleAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templatePath = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "InspectCodeBaseline",
                "InspectCodeBaselineTemplate"
            );

            testDir.CopyDirectory(templatePath);

            string projectPath = Path.Join(testDir.Value, "InspectCodeBaselineTemplate.csproj");
            string projectContent = await File.ReadAllTextAsync(projectPath);
            projectContent = projectContent
                .Replace("<EnableNETAnalyzers>false</EnableNETAnalyzers>", "<EnableNETAnalyzers>true</EnableNETAnalyzers>", StringComparison.Ordinal)
                .Replace("<RunAnalyzers>false</RunAnalyzers>", "<RunAnalyzers>true</RunAnalyzers>", StringComparison.Ordinal)
                .Replace("<RunAnalyzersDuringBuild>false</RunAnalyzersDuringBuild>", "<RunAnalyzersDuringBuild>true</RunAnalyzersDuringBuild>", StringComparison.Ordinal)
                .Replace(
                    "<GenerateDocumentationFile>false</GenerateDocumentationFile>",
                    "<GenerateDocumentationFile>false</GenerateDocumentationFile>\n    <AnalysisMode>AllEnabledByDefault</AnalysisMode>\n    <AnalysisLevel>latest-all</AnalysisLevel>",
                    StringComparison.Ordinal
                );

            await File.WriteAllTextAsync(projectPath, projectContent);

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            const string EDITORCONFIG_CONTENT = """
root = true

[*.cs]
dotnet_diagnostic.CA1822.severity = warning
""";

            await File.WriteAllTextAsync(editorconfigPath, EDITORCONFIG_CONTENT);

            string ca1822ViolationPath = Path.Join(testDir.Value, "Ca1822ViolationService.cs");
            const string CA1822_VIOLATION_CLASS = """
namespace InspectCodeBaselineTemplate;

public class Ca1822ViolationService
{
    public int GetValue()
    {
        return 42;
    }
}
""";

            await File.WriteAllTextAsync(ca1822ViolationPath, CA1822_VIOLATION_CLASS);

            string result = await InspectCodeBaselineInitCommand.RunAsync();

            string actualOutput = output.ToString().Trim();

            Assert.Contains(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER, actualOutput, StringComparison.Ordinal);
            Assert.Contains("CA1822", actualOutput, StringComparison.Ordinal);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE, actualOutput, StringComparison.Ordinal);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND, actualOutput, StringComparison.Ordinal);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE, actualOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("CA1822", result, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
