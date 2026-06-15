// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class WarningsBaselineSyncCommandTests
{
    [Theory]
    [InlineData("PathEscapesDirectory", "../../.editorconfig", false, WarningsBaselineSyncCommandConstant.PATH_ESCAPES_DIRECTORY, false)]
    [InlineData("InvalidFileName", "not-editorconfig.txt", false, WarningsBaselineSyncCommandConstant.PATH_ESCAPES_DIRECTORY, false)]
    [InlineData("EditorConfigNotFound", ".editorconfig", false, WarningsBaselineSyncCommandConstant.FILE_NOT_FOUND, false)]
    [InlineData("DefaultConfigNotFound", null, true, WarningsBaselineSyncCommandConstant.DEFAULT_CONFIGURATION_FILE_NOT_FOUND, false)]
    [InlineData("UnexpectedException", null, false, WarningsBaselineSyncCommandConstant.SYNC_FAILED, true)]
    public async Task RunShouldThrowBiakApplicationExceptionAsync(
        string testCaseName,
        string? editorconfigPath,
        bool isDefaultCommand,
        string expected,
        bool useStartsWith)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldThrowBiakApplicationExceptionAsync)}_{testCaseName}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Exception? exception = await Record.ExceptionAsync(
                async () =>
                {
                    await WarningsBaselineSyncCommand.RunAsync(
                        isDefaultCommand
                            ? [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
                            : [
                                CommandArgumentConstant.WARNINGS_BASELINE,
                                CommandArgumentConstant.SYNC,
                                CommandArgumentConstant.PATH,
                                editorconfigPath!,
                            ]
                    );
                }
            );

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);

            if (useStartsWith)
            {
                Assert.StartsWith(expected, exception.Message, StringComparison.Ordinal);
            }
            else
            {
                Assert.Equal(expected, exception.Message);
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowBiakApplicationExceptionWhenEditorConfigHasNoBaselineMarkerAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldThrowBiakApplicationExceptionWhenEditorConfigHasNoBaselineMarkerAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".editorconfig"),
                """
                [*.cs]
                indent_style = space
                indent_size = 4
                """
            );

            Exception? exception = await Record.ExceptionAsync(
                async () =>
                {
                    await WarningsBaselineSyncCommand.RunAsync(
                        [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
                    );
                }
            );

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldPreferBiakEditorconfigMainByDefaultAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldPreferBiakEditorconfigMainByDefaultAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-main"),
                "root = true"
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".editorconfig"),
                $$"""
                [{Program.cs}]
                dotnet_diagnostic.CS0168.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
                """
            );

            Exception? exception = await Record.ExceptionAsync(
                async () =>
                {
                    await WarningsBaselineSyncCommand.RunAsync(
                        [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
                    );
                }
            );

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldUsePathOptionForUserSpecifiedConfigAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldUsePathOptionForUserSpecifiedConfigAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Directory.CreateDirectory(Path.Join(testDir.Value, ".biak"));

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-main"),
                $$"""
                [{Program.cs}]
                dotnet_diagnostic.CS0168.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
                """
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, ".biak", ".editorconfig-specific"),
                "root = true"
            );

            Exception? exception = await Record.ExceptionAsync(
                async () =>
                {
                    await WarningsBaselineSyncCommand.RunAsync([
                        CommandArgumentConstant.WARNINGS_BASELINE,
                        CommandArgumentConstant.SYNC,
                        CommandArgumentConstant.PATH,
                        ".biak/.editorconfig-specific",
                    ]);
                }
            );

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineSyncCommandConstant.NO_BASELINE_MARKER, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRestoreEditorConfigWhenBuildFailsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldRestoreEditorConfigWhenBuildFailsAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG);

            Exception? exception = await Record.ExceptionAsync(
                async () =>
                {
                    await WarningsBaselineSyncCommand.RunAsync(
                        [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
                    );
                }
            );

            string restoredContent = await File.ReadAllTextAsync(editorconfigPath);

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG, restoredContent);
            Assert.DoesNotContain($"= warning {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}", restoredContent, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldReturnAllWarningsFixedWhenNoBaselineCodesAreActiveAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldReturnAllWarningsFixedWhenNoBaselineCodesAreActiveAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG);

            string result = await WarningsBaselineSyncCommand.RunAsync(
                [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
            );

            string syncedContent = await File.ReadAllTextAsync(editorconfigPath);
            string consoleOutput = output.ToString();

            string expectedOutput = WarningsBaselineSyncCommandConstant.SYNC_STARTED
                + Environment.NewLine
                + Environment.NewLine
                + WarningsBaselineSyncCommandConstant.ALL_WARNINGS_FIXED
                + Environment.NewLine
                + Environment.NewLine;

            Assert.Equal(WarningsBaselineSyncCommandConstant.ALL_WARNINGS_FIXED, result);
            Assert.Equal(expectedOutput, consoleOutput);
            Assert.DoesNotContain(WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER, syncedContent, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldHandleLfEditorConfigLineEndingsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldHandleLfEditorConfigLineEndingsAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            string lfBaseline = WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG.ReplaceLineEndings("\n");
            await File.WriteAllTextAsync(editorconfigPath, lfBaseline);

            string result = await WarningsBaselineSyncCommand.RunAsync(
                [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
            );

            string syncedContent = await File.ReadAllTextAsync(editorconfigPath);
            string consoleOutput = output.ToString();

            string expectedOutput = WarningsBaselineSyncCommandConstant.SYNC_STARTED
                + Environment.NewLine
                + Environment.NewLine
                + WarningsBaselineSyncCommandConstant.ALL_WARNINGS_FIXED
                + Environment.NewLine
                + Environment.NewLine;

            Assert.Equal(WarningsBaselineSyncCommandConstant.ALL_WARNINGS_FIXED, result);
            Assert.Equal(expectedOutput, consoleOutput);
            Assert.DoesNotContain(WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER, syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("\r\n", syncedContent, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRemoveFilterWhenCodeIsActiveButNoFilesRemainInSectionAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldRemoveFilterWhenCodeIsActiveButNoFilesRemainInSectionAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, "Program.cs"),
                """
                // Copyright (c) 2026 kurnakovv
                // This file is licensed under the MIT License.
                // See the LICENSE file in the project root for full license information.

                using System;

                namespace Biak.ConsoleApp.IntegrationTests.Templates.SimpleProject.MySimpleProjectTemplate;

                internal class Program
                {
                    static void Main()
                    {
                        int value = 42;
                    }
                }
                """
            );

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(
                editorconfigPath,
                $$"""
                root = true

                [{ResolvedFile.cs}]

                dotnet_diagnostic.CS0219.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{ProgramCS0168Warning.cs}]
                dotnet_diagnostic.cs0168.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File.cs}]

                """
            );

            string result = await WarningsBaselineSyncCommand.RunAsync(
                [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
            );

            string syncedContent = await File.ReadAllTextAsync(editorconfigPath);
            string consoleOutput = output.ToString();

            string expectedResult = "Sync complete. Removed 2 file(s); resolved 2 filter(s). 0 filter(s) still alive.";
            string expectedOutput = WarningsBaselineSyncCommandConstant.SYNC_STARTED
                + Environment.NewLine
                + Environment.NewLine
                + "ProgramCS0168Warning.cs (cs0168)"
                + Environment.NewLine
                + "ResolvedFile.cs (CS0219)"
                + Environment.NewLine
                + Environment.NewLine
                + expectedResult
                + Environment.NewLine
                + Environment.NewLine;

            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedOutput, consoleOutput);
            Assert.DoesNotContain("[{ResolvedFile.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.CS0219.severity", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("[{ProgramCS0168Warning.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.cs0168.severity", syncedContent, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRemoveResolvedFiltersAndPrunePartiallyFixedGroupsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineSyncCommandTests)}_{nameof(RunShouldRemoveResolvedFiltersAndPrunePartiallyFixedGroupsAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProjectWithWarnings",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG);

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, "ProgramCS0168Warning.cs"),
                """
                // Copyright (c) 2026 kurnakovv
                // This file is licensed under the MIT License.
                // See the LICENSE file in the project root for full license information.

                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace Biak.ConsoleApp.IntegrationTests.Templates.SimpleProjectWithWarnings.MySimpleProjectTemplate;

                internal class ProgramCS0168Warning
                {
                    static void Main()
                    {
                        int x = 0;
                        Console.WriteLine(x);
                    }
                }
                """
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, "MyTestModel.cs"),
                """
                // Copyright (c) 2026 kurnakovv
                // This file is licensed under the MIT License.
                // See the LICENSE file in the project root for full license information.

                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace Biak.ConsoleApp.IntegrationTests.Templates.SimpleProjectWithWarnings.MySimpleProjectTemplate;

                public class MyTestModel
                {
                    public string Name { get; set; } = string.Empty;
                }
                """
            );

            await File.WriteAllTextAsync(
                Path.Join(testDir.Value, "DerivedClassCS0649.cs"),
                """
                // Copyright (c) 2026 kurnakovv
                // This file is licensed under the MIT License.
                // See the LICENSE file in the project root for full license information.

                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace Biak.ConsoleApp.IntegrationTests.Templates.SimpleProjectWithWarnings.MySimpleProjectTemplate;

                internal class DerivedClassCS0649
                {
                }

                class BaseClass
                {
                    public int Value = 0;
                }

                class DerivedClass : BaseClass
                {
                    public new int Value = 0;
                }
                """
            );

            string result = await WarningsBaselineSyncCommand.RunAsync(
                [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]
            );

            string syncedContent = await File.ReadAllTextAsync(editorconfigPath);
            string consoleOutput = output.ToString();

            string expectedResult = "Sync complete. Removed 3 file(s); resolved 3 filter(s). 5 filter(s) still alive.";
            string expectedOutput = WarningsBaselineSyncCommandConstant.SYNC_STARTED
                + Environment.NewLine
                + Environment.NewLine
                + "DerivedClassCS0649.cs (CS0108, CS0649)"
                + Environment.NewLine
                + "MyTestModel.cs (CS8618)"
                + Environment.NewLine
                + "ProgramCS0168Warning.cs (CS0168)"
                + Environment.NewLine
                + Environment.NewLine
                + expectedResult
                + Environment.NewLine
                + Environment.NewLine;

            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedOutput, consoleOutput);

            Assert.DoesNotContain("[{ProgramCS0168Warning.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.CS0168.severity", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.CS0108.severity", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.CS0649.severity", syncedContent, StringComparison.Ordinal);

            Assert.Contains("[{MyTestForlder/MyTestModel1.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("[{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.Contains(
                $"dotnet_diagnostic.CS8618.severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}",
                syncedContent,
                StringComparison.Ordinal
            );
            Assert.DoesNotContain($"= warning {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}", syncedContent, StringComparison.Ordinal);
            Assert.Contains("[{ProgramCS0219Warning.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
