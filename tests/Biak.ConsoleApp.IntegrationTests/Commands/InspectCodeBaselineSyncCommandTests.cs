// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class InspectCodeBaselineSyncCommandTests
{
    [Fact]
    public async Task RunShouldSynchronizePreparedBaselineFileAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldSynchronizePreparedBaselineFileAsync)}"
        );

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

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            string baselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-InspectCodeBaseline");
            await File.WriteAllTextAsync(
                baselinePath,
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

            string serviceDPath = Path.Join(testDir.Value, "ServiceD.cs");
            string serviceDContent = await File.ReadAllTextAsync(serviceDPath);
            serviceDContent = serviceDContent.Replace(
                "return value == null || value.Length == 0;    // Rule 7",
                "return string.IsNullOrEmpty(value);",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(serviceDPath, serviceDContent);

            string result = await InspectCodeBaselineSyncCommand.RunAsync(
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ]);

            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("Sync complete.", result, StringComparison.Ordinal);
            Assert.Contains("filter(s) still alive.", result, StringComparison.Ordinal);
            Assert.Contains(
                InspectCodeBaselineInitCommandConstant.BASELINE_MARKER,
                syncedBaselineContent,
                StringComparison.Ordinal
            );
            Assert.DoesNotContain(
                $"suggestion{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldTrimFilesInsideKeptRuleBlockAsync)}"
        );

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

            string result = await InspectCodeBaselineSyncCommand.RunAsync(
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                ".biak/.editorconfig-InspectCodeBaseline",
            ]);

            string syncedBaselineContent = await File.ReadAllTextAsync(baselinePath);

            Assert.Contains("Sync complete.", result, StringComparison.Ordinal);
            Assert.Contains("filter(s) still alive.", result, StringComparison.Ordinal);
            Assert.Contains("[{ServiceC.cs}]", syncedBaselineContent, StringComparison.Ordinal);
            Assert.DoesNotContain("ServiceA.cs,ServiceC.cs", syncedBaselineContent, StringComparison.Ordinal);
            Assert.DoesNotContain("[{ServiceA.cs}]", syncedBaselineContent, StringComparison.Ordinal);
            Assert.Contains(
                "resharper_field_can_be_made_read_only_local_highlighting",
                syncedBaselineContent,
                StringComparison.OrdinalIgnoreCase
            );
            Assert.Contains(
                InspectCodeBaselineInitCommandConstant.BASELINE_MARKER,
                syncedBaselineContent,
                StringComparison.Ordinal
            );
            Assert.DoesNotContain(
                $"suggestion{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
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
    public async Task RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferCliPathOverConfigPathAndDiscoveryAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-cli"),
                "root = true"
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-config"),
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"),
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

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

            Exception? exception = await Record.ExceptionAsync(
                () => InspectCodeBaselineSyncCommand.RunAsync(args));

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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferConfigPathOverDiscoveryWhenCliPathIsNotProvidedAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-config"),
                "root = true"
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"),
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

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

            Exception? exception = await Record.ExceptionAsync(
                () => InspectCodeBaselineSyncCommand.RunAsync(args));

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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldUseDiscoveryWhenCliAndConfigPathsAreNotProvidedAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-discovery"),
                InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".editorconfig"),
                "root = true"
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            string result = await InspectCodeBaselineSyncCommand.RunAsync(args);

            Assert.DoesNotContain(
                InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER,
                result,
                StringComparison.Ordinal
            );
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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldPreferBiakDiscoveryFileWhenMarkerExistsInBothBiakAndRootAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            const string BIAK_BASELINE = $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """;

            const string ROOT_BASELINE = $$"""
                # Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
                [{ServiceC.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """;

            string biakBaselinePath = Path.Join(testDir.Value, ".biak", ".editorconfig-discovery");
            string rootBaselinePath = Path.Join(testDir.Value, ".editorconfig");

            await File.WriteAllTextAsync(biakBaselinePath, BIAK_BASELINE);
            await File.WriteAllTextAsync(rootBaselinePath, ROOT_BASELINE);

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            await InspectCodeBaselineSyncCommand.RunAsync(args);

            string biakContent = await File.ReadAllTextAsync(biakBaselinePath);
            string rootContent = await File.ReadAllTextAsync(rootBaselinePath);

            Assert.DoesNotContain(
                $"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                biakContent,
                StringComparison.Ordinal
            );
            Assert.Contains(
                $"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                rootContent,
                StringComparison.Ordinal
            );
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldFallbackToRootEditorconfigWhenNoBiakDiscoveryFileHasMarkerAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldFallbackToRootEditorconfigWhenNoBiakDiscoveryFileHasMarkerAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-empty"),
                "root = true"
            );

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

            await InspectCodeBaselineSyncCommand.RunAsync(args);

            string rootContent = await File.ReadAllTextAsync(rootBaselinePath);

            Assert.DoesNotContain(
                $"= warning {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                rootContent,
                StringComparison.Ordinal
            );
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
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineSyncCommandTests)}_{nameof(RunShouldAskToRunInitWhenNoBaselineMarkerExistsInBiakAndRootAsync)}"
        );

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
            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-empty"),
                "root = true"
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".editorconfig"),
                "root = true"
            );

            string[] args =
            [
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
            ];

            Exception? exception = await Record.ExceptionAsync(
                () => InspectCodeBaselineSyncCommand.RunAsync(args));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
