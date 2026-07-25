// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands.Baseline.Warnings;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands.Baseline.Warnings;

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
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunTestAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
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

            string editorconfigContent = await WarningsBaselineInitCommand.RunAsync(executionContext: context);

            string result = output.ToString();

            Assert.NotEmpty(result);
            Assert.Equal(TEST_OUTPUT, result.Trim());
            Assert.DoesNotContain("dotnet_diagnostic.BIKTEST001.severity", result, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, "root = true" + Environment.NewLine + Environment.NewLine + editorconfigContent);

            string secondEditorconfigContent = await WarningsBaselineInitCommand.RunAsync(executionContext: context);

            Assert.Equal(WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND, secondEditorconfigContent);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildFailedAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildFailedAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildProcessCannotStartAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildProcessCannotStartAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        string? originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));

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
        }
    }

    [Fact]
    public async Task RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
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

            Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));

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
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenBiakBuildBinlogNotFoundAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenBiakBuildBinlogNotFoundAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProjectWithWarnings",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            string buildBinlogPath = Path.Join(testDir.Value, WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH);

            Task<Exception?> exceptionTask = Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));
            await DeleteFileWhileTaskIsRunningAsync(exceptionTask, buildBinlogPath, TimeSpan.FromSeconds(30));

            Exception? exception = await exceptionTask;

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(WarningsBaselineBuildConstant.BUILD_BINLOG_NOT_FOUND, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task RunShouldSupportExplicitBuildTargetAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldSupportExplicitBuildTargetAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProjectWithWarnings",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        string result = await WarningsBaselineInitCommand.RunAsync(
            [
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "MySimpleProjectTemplate.csproj",
            ],
            executionContext: context
        );

        Assert.Contains("dotnet_diagnostic.CS0168.severity", result, StringComparison.Ordinal);
        Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));
    }

    [Fact]
    public async Task RunShouldRejectBuildTargetOutsideCurrentDirectoryAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRejectBuildTargetOutsideCurrentDirectoryAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(
            [
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "../outside.csproj",
            ],
            executionContext: context
        ));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(WarningsBaselineBuildConstant.INVALID_BUILD_TARGET_PATH, exception.Message);
    }

    private static async Task DeleteFileWhileTaskIsRunningAsync(Task task, string filePath, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);

        while (!task.IsCompleted && DateTime.UtcNow < deadline)
        {
            TryDeleteFile(filePath);
            await Task.Delay(10);
        }
    }

    private static void TryDeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
