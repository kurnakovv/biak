// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands.Baseline.Warnings;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class WarningsBaselineInitCommandTests
{
#pragma warning disable IDE1006 // Naming Styles
    private static readonly string TEST_OUTPUT = WarningsBaselineInitCommandConstant.INIT_STARTED
#pragma warning restore IDE1006 // Naming Styles
        + Environment.NewLine
        + WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE
        + Environment.NewLine
        + WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_CONFIGURATION
        + Environment.NewLine
        + Environment.NewLine
        + WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE
        + Environment.NewLine
        + WarningsBaselineCommandTestConstants.BASELINE_FILTERS;

    [Fact]
    public async Task RunTestAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunTestAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

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

            string csprojPath = Path.Join(testDir.Value, "MySimpleProjectTemplate.csproj");
            string csprojContent = await File.ReadAllTextAsync(csprojPath);
            csprojContent = csprojContent.Replace(
                "</Project>",
                """
                  <Target Name="Biak_TestWarning" BeforeTargets="CoreCompile">
                    <Warning Text="Test warning from project file (should not be included in baseline)" Code="BIKTEST001" />
                  </Target>
                </Project>
                """,
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(csprojPath, csprojContent);

            string editorconfigContent = await WarningsBaselineInitCommand.RunAsync();

            string result = output.ToString();

            Assert.NotEmpty(result);
            Assert.Equal(TEST_OUTPUT, result.Trim());
            Assert.DoesNotContain("dotnet_diagnostic.BIKTEST001.severity", result, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, "root = true" + Environment.NewLine + Environment.NewLine + editorconfigContent);

            string secondEditorconfigContent = await WarningsBaselineInitCommand.RunAsync();

            Assert.Equal(WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND, secondEditorconfigContent);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildFailedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildFailedAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildProcessCannotStartAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildProcessCannotStartAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        string? originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.True(
                exception.Message.StartsWith(WarningsBaselineInitCommandConstant.INIT_FAILED, StringComparison.Ordinal)
                    || exception.Message.StartsWith(WarningsBaselineBuildConstant.FAILED_TO_START_DOTNET_BUILD, StringComparison.Ordinal)
                    || exception.Message.StartsWith(WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED, StringComparison.Ordinal),
                $"Expected message to start with '{WarningsBaselineInitCommandConstant.INIT_FAILED}', '{WarningsBaselineBuildConstant.FAILED_TO_START_DOTNET_BUILD}', or '{WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED}', but got '{exception.Message}'."
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

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

            string brokenFile = Path.Join(testDir.Value, "ProgramCS0168Warning.cs");
            await File.WriteAllTextAsync(
                brokenFile,
                """
                public class ProgramCS0168Warning
                {
                    public static void M1()
                    {
                        int a = ;
                    }
                }
                """
            );

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.True(
                exception.Message.StartsWith(WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED, StringComparison.Ordinal)
                    || exception.Message.Equals(WarningsBaselineBuildConstant.BUILD_CONTAINS_ERRORS, StringComparison.Ordinal),
                $"Expected message to start with '{WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED}' or be '{WarningsBaselineBuildConstant.BUILD_CONTAINS_ERRORS}', but got '{exception.Message}'."
            );
            Assert.DoesNotContain(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE, output.ToString(), StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenBiakBuildBinlogNotFoundAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenBiakBuildBinlogNotFoundAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

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

            string anotherDirectory = Path.Join(testDir.Value, "another");
            Directory.CreateDirectory(anotherDirectory);

            Task<Exception> exceptionTask = Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync());
            await Task.Delay(100);
            Directory.SetCurrentDirectory(anotherDirectory);

            Exception exception = await exceptionTask;

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(WarningsBaselineBuildConstant.BUILD_BINLOG_NOT_FOUND, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldSupportExplicitBuildTargetAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldSupportExplicitBuildTargetAsync)}"
        );

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

            string result = await WarningsBaselineInitCommand.RunAsync([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "MySimpleProjectTemplate.csproj",
            ]);

            Assert.Contains("dotnet_diagnostic.CS0168.severity", result, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldRejectBuildTargetOutsideCurrentDirectoryAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRejectBuildTargetOutsideCurrentDirectoryAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "../outside.csproj",
            ]));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineBuildConstant.INVALID_BUILD_TARGET_PATH, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
