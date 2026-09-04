// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Commands.Baseline.InspectCode;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands.Baseline.InspectCode;

public class InspectCodeBaselineSyncCommandTests
{
    [Fact]
    public async Task RunShouldSynchronizePreparedBaselineFileAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldSynchronizePreparedBaselineFileAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

        Assert.Equal("Sync complete. Removed 1 file(s); resolved 1 filter(s). 7 filter(s) still alive.", result);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
        Assert.DoesNotContain($"suggestion{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", syncedBaselineContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldNormalizeAliveFiltersToUpdatedSnapshotSeverityAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldNormalizeAliveFiltersToUpdatedSnapshotSeverityAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 8 filter(s) still alive.", result);
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

    [Fact]
    public async Task RunShouldCountAliveRulesWhenBadFormattingAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldCountAliveRulesWhenBadFormattingAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

        string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
        await File.WriteAllTextAsync(
            baselinePath,
            $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting  =  suggestion    {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}      
                    # user comment after baseline rule with extra spaces      
                # Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
                [{ServiceD.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting        =         suggestion         {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}     
                    # another user comment right after diagnostic


                # Convert local variable or field into constant (private accessibility) [ConvertToConstant.Local]
                [{ServiceA.cs}]
                resharper_convert_to_constant_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                # user note: keep for now
                # Member can be made private (non-private accessibility) [MemberCanBePrivate.Global]
                [{ServiceB.cs}]
                resharper_member_can_be_private_global_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}





                # Convert property into auto-property [ConvertToAutoProperty]
                [{ServiceE.cs}]
                resharper_convert_to_auto_property_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                [{ServiceC.cs}]
                resharper_unused_member_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}


                [{ServiceE.cs}]
                resharper_replace_with_single_call_to_first_or_default_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
        );

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);

        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 7 filter(s) still alive.", result);
    }

    [Fact]
    public async Task RunShouldTrimFilesInsideKeptRuleBlockAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldTrimFilesInsideKeptRuleBlockAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

        Assert.Equal("Sync complete. Removed 1 file(s); resolved 0 filter(s). 1 filter(s) still alive.", result);
        Assert.Contains("[{ServiceC.cs}]", syncedBaselineContent, StringComparison.Ordinal);
        Assert.DoesNotContain("ServiceA.cs,ServiceC.cs", syncedBaselineContent, StringComparison.Ordinal);
        Assert.DoesNotContain("[{ServiceA.cs}]", syncedBaselineContent, StringComparison.Ordinal);
        Assert.Contains(
            $"resharper_field_can_be_made_read_only_local_highlighting = suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
            syncedBaselineContent,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task RunShouldKeepActiveNoneFiltersAndRemoveResolvedOnesAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldKeepActiveNoneFiltersAndRemoveResolvedOnesAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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
        await EnableCommand.RunAsync(context);

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

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);
        string rootEditorconfigContent = await File.ReadAllTextAsync(Path.Join(testDir.Value, ".editorconfig"));

        Assert.Equal("Sync complete. Removed 1 file(s); resolved 1 filter(s). 1 filter(s) still alive.", result);
        Assert.DoesNotContain(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, result, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "resharper_replace_with_string_is_null_or_empty_highlighting",
            syncedBaselineContent,
            StringComparison.OrdinalIgnoreCase
        );
        Assert.DoesNotContain(
            "resharper_replace_with_string_is_null_or_empty_highlighting",
            rootEditorconfigContent,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task RunShouldThrowWhenRootEditorconfigIsMissingAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowWhenRootEditorconfigIsMissingAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineSyncCommandConstant.ROOT_EDITORCONFIG_FILE_NOT_FOUND, exception.Message);
        Assert.Equal(InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS, await File.ReadAllTextAsync(baselinePath));
    }

    [Fact]
    public async Task RunShouldThrowWhenBiakStatusIsUnsynchronisedAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowWhenBiakStatusIsUnsynchronisedAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineSyncCommandConstant.BIAK_STATUS_IS_NOT_SYNCHRONIZED, exception.Message);
    }

    [Fact]
    public async Task RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
    }

    [Fact]
    public async Task RunShouldPreferConfigPathOverDiscoveryWhenCliPathIsNotProvidedAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferConfigPathOverDiscoveryWhenCliPathIsNotProvidedAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
    }

    [Fact]
    public async Task RunShouldUseDiscoveryFromNestedBiakDirectoryWhenCliAndConfigPathsAreNotProvidedAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldUseDiscoveryFromNestedBiakDirectoryWhenCliAndConfigPathsAreNotProvidedAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

        string nestedDirectory = Path.Join(testDir.Value, ".biak", "Categories");
        Directory.CreateDirectory(nestedDirectory);
        await File.WriteAllTextAsync(Path.Join(nestedDirectory, ".editorconfig-discovery"), InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 8 filter(s) still alive.", result);
    }

    [Fact]
    public async Task RunShouldPreferBiakDiscoveryFileWhenMarkerExistsInBothBiakAndRootAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferBiakDiscoveryFileWhenMarkerExistsInBothBiakAndRootAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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
        await EnableCommand.RunAsync(context);

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);

        string biakContent = await File.ReadAllTextAsync(biakBaselinePath);
        string rootContent = await File.ReadAllTextAsync(rootBaselinePath);

        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 1 filter(s) still alive.", result);
        Assert.DoesNotContain($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", biakContent, StringComparison.Ordinal);
        Assert.Contains($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldFallbackToRootEditorconfigWhenNoBiakDirectoryExistsAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldFallbackToRootEditorconfigWhenNoBiakDirectoryExistsAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
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

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);

        string rootContent = await File.ReadAllTextAsync(rootBaselinePath);
        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 1 filter(s) still alive.", result);
        Assert.DoesNotContain($"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
        Assert.Contains($"= suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}", rootContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldAskToRunInitWhenNoBaselineMarkerExistsInBiakAndRootAsync()
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldAskToRunInitWhenNoBaselineMarkerExistsInBiakAndRootAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

        await File.WriteAllTextAsync(Path.Join(testDir.Value, ".biak", ".editorconfig-empty"), "root = true");

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
    }

    [Theory]
    [InlineData("PathEscapesDirectory", "../../.editorconfig", InspectCodeBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG)]
    [InlineData("InvalidFileName", "not-editorconfig.txt", InspectCodeBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG)]
    [InlineData("EditorConfigNotFound", ".editorconfig-missing", InspectCodeBaselineSyncCommandConstant.FILE_NOT_FOUND)]
    public async Task RunShouldThrowForInvalidPathScenariosAsync(string testCaseName, string editorconfigPath, string expectedMessage)
    {
        TestDirectory testDir = new($"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldThrowForInvalidPathScenariosAsync)}_{testCaseName}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), "root = true\n");
        await EnsureBiakStatusConfiguredAsync(context);

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                editorconfigPath,
            ];

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task RunAsyncWhenMappedEditorconfigKeyIsNullShouldSkipRuleAndRemoveStaleFilterAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunAsyncWhenMappedEditorconfigKeyIsNullShouldSkipRuleAndRemoveStaleFilterAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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
        await EnableCommand.RunAsync(context);

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

        Assert.Equal(InspectCodeBaselineSyncCommandConstant.ALL_ISSUES_FIXED, result);
        Assert.DoesNotContain("dotnet_diagnostic.CA1822.severity", syncedBaselineContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenRuleIdOverrideMapsCa1822ShouldKeepBaselineFilterAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunAsyncWhenRuleIdOverrideMapsCa1822ShouldKeepBaselineFilterAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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
        await EnableCommand.RunAsync(context);

        string[] args =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ];

        string result = await InspectCodeBaselineSyncCommand.RunAsync(context, args);
        string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

        Assert.Equal("Sync complete. Removed 0 file(s); resolved 0 filter(s). 1 filter(s) still alive.", result);
        Assert.Contains("dotnet_diagnostic.CA1822.severity", syncedBaselineContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, syncedBaselineContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldRestoreBaselineFileWhenSyncFailsAfterOriginalContentCapturedAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldRestoreBaselineFileWhenSyncFailsAfterOriginalContentCapturedAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);

        string actualBaselineContent = await File.ReadAllTextAsync(baselinePath);
        DateTime actualLastWriteTime = File.GetLastWriteTimeUtc(baselinePath);

        Assert.Equal(expectedBaselineContent, actualBaselineContent);
        Assert.True(actualLastWriteTime > expectedLastWriteTime);
    }

    [Fact]
    public async Task RunShouldRestoreRuntimeEditorconfigWhenSyncFailsAfterTemporaryModificationAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldRestoreRuntimeEditorconfigWhenSyncFailsAfterTemporaryModificationAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };
        CopyInspectCodeTemplate(testDir.Value);
        await EnsureBiakStatusConfiguredAsync(context);

        string baselinePath = Path.Join(testDir.Value, ".editorconfig-InspectCodeBaseline");
        await File.WriteAllTextAsync(baselinePath, InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS);

        string biakDirectoryPath = Path.Join(testDir.Value, ".biak");
        if (Directory.Exists(biakDirectoryPath))
        {
            Directory.Delete(biakDirectoryPath, recursive: true);
        }

        string runtimeEditorconfigPath = Path.Join(testDir.Value, ".editorconfig");
        const string EXPECTED_RUNTIME_EDITORCONFIG_CONTENT = $"""
                root = true

                [*.cs]
                dotnet_diagnostic.CA1822.severity = suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}
                """;
        await File.WriteAllTextAsync(runtimeEditorconfigPath, EXPECTED_RUNTIME_EDITORCONFIG_CONTENT);

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

        Exception? exception = await Record.ExceptionAsync(() => InspectCodeBaselineSyncCommand.RunAsync(context, args));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);

        string actualRuntimeEditorconfigContent = await File.ReadAllTextAsync(runtimeEditorconfigPath);
        DateTime actualLastWriteTime = File.GetLastWriteTimeUtc(runtimeEditorconfigPath);

        Assert.Equal(EXPECTED_RUNTIME_EDITORCONFIG_CONTENT, actualRuntimeEditorconfigContent);
        Assert.True(actualLastWriteTime > expectedLastWriteTime);
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

    private static async Task EnsureBiakStatusConfiguredAsync(AppExecutionContext context)
    {
        string editorconfigPath = Path.Join(context.WorkingDirectory, ".editorconfig");
        if (!File.Exists(editorconfigPath))
        {
            await File.WriteAllTextAsync(editorconfigPath, "root = true\n");
        }

        await SetupCommand.RunAsync(context);
    }
}
