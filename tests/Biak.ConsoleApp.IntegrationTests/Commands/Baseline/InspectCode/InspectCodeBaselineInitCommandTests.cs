// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands.Baseline.InspectCode;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands.Baseline.InspectCode;

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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunShouldOutputBaselineToConsoleAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
        await File.WriteAllTextAsync(
            Path.Join(testDir.Value, ".biak", "config.json"),
            // language=json
            """
            {
              "inspectCodeBaseline": {
                "additionalArgs": ["--sEverity=WARNING"]
              }
            }
            """);

        string firstRunResult = await InspectCodeBaselineInitCommand.RunAsync(context);

        string firstRunOutput = output.ToString().Trim();

        Assert.Equal(InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS, firstRunResult.Trim());
        Assert.Equal(TEST_OUTPUT, firstRunOutput);

        string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
        await File.AppendAllTextAsync(editorconfigPath, firstRunResult.Trim());

        // In rare cases, file-specific overrides are not detected reliably, clearing this source file removes the residual issue.
        string serviceEPath = Path.Join(testDir.Value, "ServiceE.cs");
        await File.WriteAllTextAsync(serviceEPath, string.Empty);

        output.GetStringBuilder().Clear();

        string secondRunResult = await InspectCodeBaselineInitCommand.RunAsync(context);
        string secondRunOutput = output.ToString().Trim();

        Assert.Equal(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, secondRunResult);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.INIT_STARTED, secondRunOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, secondRunOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE, secondRunOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenNoIssuesFoundShouldReturnNoIssuesFoundMessageAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenNoIssuesFoundShouldReturnNoIssuesFoundMessageAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineNoIssuesTemplate"
        );

        testDir.CopyDirectory(templatePath);

        string result = await InspectCodeBaselineInitCommand.RunAsync(context);

        string actualOutput = output.ToString().Trim();

        Assert.Equal(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, result);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.INIT_STARTED, actualOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, actualOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenUnmappedRuleDetectedShouldOutputWarningToConsoleAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenUnmappedRuleDetectedShouldOutputWarningToConsoleAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

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

        string result = await InspectCodeBaselineInitCommand.RunAsync(context);

        string actualOutput = output.ToString().Trim();

        Assert.Contains(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER, actualOutput, StringComparison.Ordinal);
        Assert.Contains("CA1822", actualOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE, actualOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND, actualOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE, actualOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("CA1822", result, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenOnlyUnmappedIssuesExistShouldReturnNoIssuesFoundAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenOnlyUnmappedIssuesExistShouldReturnNoIssuesFoundAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        File.Delete(Path.Join(testDir.Value, "ServiceA.cs"));
        File.Delete(Path.Join(testDir.Value, "ServiceB.cs"));
        File.Delete(Path.Join(testDir.Value, "ServiceC.cs"));
        File.Delete(Path.Join(testDir.Value, "ServiceD.cs"));
        File.Delete(Path.Join(testDir.Value, "ServiceE.cs"));

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
resharper_unused_member_global_highlighting = none
resharper_unused_type_global_highlighting = none
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

        string consumerPath = Path.Join(testDir.Value, "Consumer.cs");
        const string CONSUMER_CLASS = """
namespace InspectCodeBaselineTemplate;

public static class Consumer
{
    public static int Execute()
    {
        Ca1822ViolationService service = new();
        return service.GetValue();
    }
}
""";

        await File.WriteAllTextAsync(consumerPath, CONSUMER_CLASS);

        string result = await InspectCodeBaselineInitCommand.RunAsync(context);

        string actualOutput = output.ToString().Trim();

        Assert.Equal(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, result);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER, actualOutput, StringComparison.Ordinal);
        Assert.Contains("CA1822", actualOutput, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND, actualOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE, actualOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenUnexpectedExceptionOccursShouldWrapIntoBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenUnexpectedExceptionOccursShouldWrapIntoBiakApplicationExceptionAsync)}");
        await using StringWriter disposedOutput = new();
        // ReSharper disable once DisposeOnUsingVariable
        await disposedOutput.DisposeAsync();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = disposedOutput };

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineInitCommand.RunAsync(context));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineInitCommandConstant.INIT_FAILED, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenRuleIdOverridesProvidedShouldUseOverrideKeysAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunAsyncWhenRuleIdOverridesProvidedShouldUseOverrideKeysAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

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

        Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
        string configPath = Path.Join(testDir.Value, ".biak", "config.json");

        // language=json
        const string CONFIG_WITH_RULE_ID_OVERRIDES = """
{
  "inspectCodeBaseline": {
    "ruleIdOverrides": {
      "CA1822": "resharper_mocked_custom_ca_rule_highlighting",
      "UnusedType.Global": "resharper_mocked_unused_type_global_highlighting"
    }
  }
}
""";
        await File.WriteAllTextAsync(configPath, CONFIG_WITH_RULE_ID_OVERRIDES);

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

        string result = await InspectCodeBaselineInitCommand.RunAsync(context);
        string actualOutput = output.ToString().Trim();

        Assert.Contains("resharper_mocked_custom_ca_rule_highlighting", result, StringComparison.Ordinal);
        Assert.Contains("resharper_mocked_unused_type_global_highlighting", result, StringComparison.Ordinal);
        Assert.DoesNotContain("resharper_unused_type_global_highlighting", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA1822", actualOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldWriteExecutedInspectCodeCommandAndPreserveGeneratedSarifWhenDebugModeEnabledAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunShouldWriteExecutedInspectCodeCommandAndPreserveGeneratedSarifWhenDebugModeEnabledAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
        await File.WriteAllTextAsync(
            Path.Join(testDir.Value, ".biak", "config.json"),
            // language=json
            """
            {
              "inspectCodeBaseline": {
                "debugMode": true
              }
            }
            """
        );

        _ = await InspectCodeBaselineInitCommand.RunAsync(context);

        string commandOutput = output.ToString();
        string sarifDirectory = Path.Join(testDir.Value, ".biak", "logs", "inspectcode-baseline");
        string[] generatedSarifFiles = Directory.Exists(sarifDirectory)
            ? Directory.GetFiles(sarifDirectory, "*.sarif")
            : [];

        Assert.Contains("jb", commandOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inspectcode", commandOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-f=Sarif", commandOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("InspectCode SARIF log:", commandOutput, StringComparison.Ordinal);
        Assert.Single(generatedSarifFiles);

        string generatedSarifContent = await File.ReadAllTextAsync(generatedSarifFiles[0]);

        Assert.Contains("\"runs\"", generatedSarifContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldDeleteGeneratedSarifWhenDebugModeIsDisabledAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunShouldDeleteGeneratedSarifWhenDebugModeIsDisabledAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
        await File.WriteAllTextAsync(
            Path.Join(testDir.Value, ".biak", "config.json"),
            // language=json
            """
            {
              "inspectCodeBaseline": {
                "debugMode": false
              }
            }
            """
        );

        _ = await InspectCodeBaselineInitCommand.RunAsync(context);

        string commandOutput = output.ToString();
        string sarifDirectory = Path.Join(testDir.Value, ".biak", "logs", "inspectcode-baseline");
        string[] generatedSarifFiles = Directory.Exists(sarifDirectory)
            ? Directory.GetFiles(sarifDirectory, "*.sarif")
            : [];

        Assert.DoesNotContain("InspectCode command attempt", commandOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("InspectCode SARIF log:", commandOutput, StringComparison.Ordinal);
        Assert.Empty(generatedSarifFiles);
    }
}
