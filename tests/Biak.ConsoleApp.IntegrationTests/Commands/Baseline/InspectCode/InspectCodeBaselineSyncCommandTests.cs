// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Commands.Baseline.InspectCode;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands.Baseline.InspectCode;

public class InspectCodeBaselineSyncCommandTests
{
    [Fact]
    public async Task RunShouldSynchronizePreparedBaselineFileAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldSynchronizePreparedBaselineFileAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string serviceDPath = Path.Join(testDir.Value, "ServiceD.cs");
            string serviceDContent = await File.ReadAllTextAsync(serviceDPath);
            serviceDContent = serviceDContent.Replace(
                "return value == null || value.Length == 0;    // Rule 7",
                "return string.IsNullOrEmpty(value);",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(serviceDPath, serviceDContent);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("Sync complete.", result, StringComparison.Ordinal);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
            Assert.DoesNotContain($"suggestion{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", syncedBaselineContent, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldNormalizeAliveFiltersToUpdatedSnapshotSeverityAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldNormalizeAliveFiltersToUpdatedSnapshotSeverityAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", "config.json"),
                // language=json
                """
                {
                  "inspectCodeBaseline": {
                    "snapshotSeverity": "none"
                  }
                }
                """
            );

            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("Sync complete.", result, StringComparison.Ordinal);
            Assert.DoesNotContain(
                $"= suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                syncedBaselineContent,
                StringComparison.Ordinal
            );
            Assert.Contains(
                $"= none {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                syncedBaselineContent,
                StringComparison.Ordinal
            );
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldTrimFilesInsideKeptRuleBlockAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldTrimFilesInsideKeptRuleBlockAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(
                baselinePath,
                $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceA.cs,ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            );

            string serviceAPath = Path.Join(testDir.Value, "ServiceA.cs");
            string serviceAContent = await File.ReadAllTextAsync(serviceAPath);
            serviceAContent = serviceAContent.Replace(
                "public string Format(string message) =>",
                "public string Format(string message)\n    {\n        _enabled = !_enabled;\n        _timeout++;\n        _prefix = _prefix + string.Empty;\n        return",
                StringComparison.Ordinal
            );
            serviceAContent = serviceAContent.Replace(
                "_enabled ? $\"[{_prefix}:{_timeout}] {message}\" : string.Empty;",
                "_enabled ? $\"[{_prefix}:{_timeout}] {message}\" : string.Empty;\n    }",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(serviceAPath, serviceAContent);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("Sync complete.", result, StringComparison.Ordinal);
            Assert.Contains("[{ServiceC.cs}]", syncedBaselineContent, StringComparison.Ordinal);
            Assert.DoesNotContain("ServiceA.cs,ServiceC.cs", syncedBaselineContent, StringComparison.Ordinal);
            Assert.DoesNotContain("[{ServiceA.cs}]", syncedBaselineContent, StringComparison.Ordinal);
            Assert.Contains(
                $"resharper_field_can_be_made_read_only_local_highlighting = suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                syncedBaselineContent,
                StringComparison.OrdinalIgnoreCase
            );
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldKeepActiveNoneFiltersAndRemoveResolvedOnesAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldKeepActiveNoneFiltersAndRemoveResolvedOnesAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", "config.json"),
                // language=json
                """
                {
                  "inspectCodeBaseline": {
                    "snapshotSeverity": "none"
                  }
                }
                """
            );

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-main");
            await File.WriteAllTextAsync(
                baselinePath,
                $$"""
                root = true

                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = none {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                # Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
                [{ServiceD.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            );
            await EnableCommand.RunAsync();

            string serviceDPath = Path.Join(testDir.Value, "ServiceD.cs");
            string serviceDContent = await File.ReadAllTextAsync(serviceDPath);
            serviceDContent = serviceDContent.Replace(
                "return value == null || value.Length == 0;    // Rule 7",
                "return string.IsNullOrEmpty(value);",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(serviceDPath, serviceDContent);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-main",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Equal(InspectCodeBaselineSyncCommandConstant.ALL_ISSUES_FIXED, result);
            Assert.DoesNotContain(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, result, StringComparison.Ordinal);
            Assert.DoesNotContain(
                "resharper_replace_with_string_is_null_or_empty_highlighting",
                syncedBaselineContent,
                StringComparison.OrdinalIgnoreCase
            );
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenRootEditorconfigIsMissingAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowWhenRootEditorconfigIsMissingAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            string rootEditorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            if (File.Exists(rootEditorconfigPath))
            {
                File.Delete(rootEditorconfigPath);
            }

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.ROOT_EDITORCONFIG_FILE_NOT_FOUND, exception.Message);
            Assert.Equal(InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS, await File.ReadAllTextAsync(baselinePath));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenBiakStatusIsUnsynchronisedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowWhenBiakStatusIsUnsynchronisedAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), "root = true\n");

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));
            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline"),
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.BIAK_STATUS_IS_NOT_SYNCHRONIZED, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-cli"), "root = true");
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-config"), InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"), InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", "config.json"),
                // language=json
                """
                {
                  "inspectCodeBaseline": {
                    "path": ".biak/.editorconfig-config"
                  }
                }
                """
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-cli",
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldPreferConfigPathOverDiscoveryWhenCliPathIsNotProvidedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferConfigPathOverDiscoveryWhenCliPathIsNotProvidedAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-config"), "root = true");
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"), InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", "config.json"),
                // language=json
                """
                {
                  "inspectCodeBaseline": {
                    "path": ".biak/.editorconfig-config"
                  }
                }
                """
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldUseDiscoveryWhenCliAndConfigPathsAreNotProvidedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldUseDiscoveryWhenCliAndConfigPathsAreNotProvidedAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"), InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            Assert.DoesNotContain(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, result, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldPreferBiakDiscoveryFileWhenMarkerExistsInBothBiakAndRootAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferBiakDiscoveryFileWhenMarkerExistsInBothBiakAndRootAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            const string BASELINE = $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """;

            string biakBaselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-discovery");
            string rootBaselinePath = Path.Join(testDir.Value, ".editorconfig");
            string editorconfigMainPath = Path.Join(testDir.Value, ".biak", ".editorconfig-main");

            await File.WriteAllTextAsync(biakBaselinePath, BASELINE);
            await File.WriteAllTextAsync(editorconfigMainPath, BASELINE);
            await EnableCommand.RunAsync();

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            await InspectCodeBaselineSyncCommand.RunAsync(args);

            string biakContent = await File.ReadAllTextAsync(biakBaselinePath);
            string rootContent = await File.ReadAllTextAsync(rootBaselinePath);

            Assert.DoesNotContain($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", biakContent, StringComparison.Ordinal);
            Assert.Contains($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldFallbackToRootEditorconfigWhenNoBiakDirectoryExistsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldFallbackToRootEditorconfigWhenNoBiakDirectoryExistsAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);

            string biakDir = Path.Join(testDir.Value, ".biak");
            if (Directory.Exists(biakDir))
            {
                Directory.Delete(biakDir, recursive: true);
            }

            string rootBaselinePath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(
                rootBaselinePath,
                $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);

            string rootContent = await File.ReadAllTextAsync(rootBaselinePath);
            Assert.DoesNotContain($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
            Assert.Contains($"= suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
            Assert.Contains("filter(s) still alive.", result, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldAskToRunInitWhenNoBaselineMarkerExistsInBiakAndRootAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldAskToRunInitWhenNoBaselineMarkerExistsInBiakAndRootAsync)}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-empty"), "root = true");

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Theory]
    [InlineData("PathEscapesDirectory", "../../.editorconfig", InspectCodeBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG)]
    [InlineData("InvalidFileName", "not-editorconfig.txt", InspectCodeBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG)]
    [InlineData("EditorConfigNotFound", ".editorconfig-missing", InspectCodeBaselineSyncCommandConstant.FILE_NOT_FOUND)]
    public async Task RunShouldThrowForInvalidPathScenariosAsync(string testCaseName, string editorconfigPath, string expectedMessage)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowForInvalidPathScenariosAsync)}_{testCaseName}");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), "root = true\n");
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                editorconfigPath,
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(expectedMessage, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenMappedEditorconfigKeyIsNullShouldSkipRuleAndRemoveStaleFilterAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunAsyncWhenMappedEditorconfigKeyIsNullShouldSkipRuleAndRemoveStaleFilterAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

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

            const string EDITORCONFIG_WITH_CA1822 = """
root = true

[*.cs]
dotnet_diagnostic.CA1822.severity = warning
""";
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), EDITORCONFIG_WITH_CA1822);

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
            await File.WriteAllTextAsync(Path.Join(testDir.Value, "Ca1822ViolationService.cs"), CA1822_VIOLATION_CLASS);

            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(
                baselinePath,
                $$"""
                # Mocked CA1822 baseline mapping
                [{Ca1822ViolationService.cs}]
                dotnet_diagnostic.CA1822.severity = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            );
            await EnableCommand.RunAsync();

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Equal(InspectCodeBaselineSyncCommandConstant.ALL_ISSUES_FIXED, result);
            Assert.DoesNotContain("dotnet_diagnostic.CA1822.severity", syncedBaselineContent, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenRuleIdOverrideMapsCa1822ShouldKeepBaselineFilterAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunAsyncWhenRuleIdOverrideMapsCa1822ShouldKeepBaselineFilterAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

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

            const string EDITORCONFIG_WITH_CA1822 = """
root = true

[*.cs]
dotnet_diagnostic.CA1822.severity = warning
""";
            await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), EDITORCONFIG_WITH_CA1822);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", "config.json"),
                // language=json
                """
                {
                  "inspectCodeBaseline": {
                    "ruleIdOverrides": {
                      "CA1822": "dotnet_diagnostic.CA1822.severity"
                    }
                  }
                }
                """
            );

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
            await File.WriteAllTextAsync(Path.Join(testDir.Value, "Ca1822ViolationService.cs"), CA1822_VIOLATION_CLASS);

            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(
                baselinePath,
                $$"""
                # Mocked CA1822 baseline mapping
                [{Ca1822ViolationService.cs}]
                dotnet_diagnostic.CA1822.severity = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            );
            await EnableCommand.RunAsync();

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);
            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("filter(s) still alive.", result, StringComparison.Ordinal);
            Assert.Contains("dotnet_diagnostic.CA1822.severity", syncedBaselineContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRestoreBaselineFileWhenSyncFailsAfterOriginalContentCapturedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldRestoreBaselineFileWhenSyncFailsAfterOriginalContentCapturedAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string expectedBaselineContent = await File.ReadAllTextAsync(baselinePath);
            DateTime expectedLastWriteTime = DateTime.UtcNow.AddMinutes(-5);
            File.SetLastWriteTimeUtc(baselinePath, expectedLastWriteTime);

            File.Delete(Path.Join(testDir.Value, "InspectCodeBaselineTemplate.csproj"));

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);

            string actualBaselineContent = await File.ReadAllTextAsync(baselinePath);
            DateTime actualLastWriteTime = File.GetLastWriteTimeUtc(baselinePath);

            Assert.Equal(expectedBaselineContent, actualBaselineContent);
            Assert.True(actualLastWriteTime > expectedLastWriteTime);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRestoreRuntimeEditorconfigWhenSyncFailsAfterTemporaryModificationAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldRestoreRuntimeEditorconfigWhenSyncFailsAfterTemporaryModificationAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            CopyInspectCodeTemplate(testDir.Value);
            await EnsureBiakStatusConfiguredAsync(testDir.Value);

            string baselinePath = Path.Join(testDir.Value, ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

            string biakDirectoryPath = Path.Join(testDir.Value, ".biak");
            if (Directory.Exists(biakDirectoryPath))
            {
                Directory.Delete(biakDirectoryPath, recursive: true);
            }

            string runtimeEditorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            string expectedRuntimeEditorconfigContent = $$"""
                root = true

                [*.cs]
                dotnet_diagnostic.CA1822.severity = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """;
            await File.WriteAllTextAsync(runtimeEditorconfigPath, expectedRuntimeEditorconfigContent);

            DateTime expectedLastWriteTime = DateTime.UtcNow.AddMinutes(-5);
            File.SetLastWriteTimeUtc(runtimeEditorconfigPath, expectedLastWriteTime);

            File.Delete(Path.Join(testDir.Value, "InspectCodeBaselineTemplate.csproj"));

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".editorconfig-InspectCodeBaseline",
            ];

            Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);

            string actualRuntimeEditorconfigContent = await File.ReadAllTextAsync(runtimeEditorconfigPath);
            DateTime actualLastWriteTime = File.GetLastWriteTimeUtc(runtimeEditorconfigPath);

            Assert.Equal(expectedRuntimeEditorconfigContent, actualRuntimeEditorconfigContent);
            Assert.True(actualLastWriteTime > expectedLastWriteTime);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static void CopyInspectCodeTemplate(string testDirectory)
    {
        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        CopyDirectory(templatePath, testDirectory);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        DirectoryInfo dir = new(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Join(destinationDir, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Join(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private static async Task EnsureBiakStatusConfiguredAsync(string testDirectory)
    {
        string editorconfigPath = Path.Join(testDirectory, ".editorconfig");
        if (!File.Exists(editorconfigPath))
        {
            await File.WriteAllTextAsync(editorconfigPath, "root = true\n");
        }

        await SetupCommand.RunAsync();
    }
}
