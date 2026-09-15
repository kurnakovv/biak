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
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

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

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildFailedAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildFailedAsync)}"
        );
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync)}"
        );
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

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

    [Fact]
    public async Task RunShouldThrowWhenBiakBuildBinlogNotFoundAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenBiakBuildBinlogNotFoundAsync)}"
        );
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProjectWithWarnings",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        string buildBinlogPath = Path.Join(testDir.Value, WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH);

        Task<Exception> exceptionTask = Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(executionContext: context));
        await DeleteFileWhileTaskIsRunningAsync(exceptionTask, buildBinlogPath, TimeSpan.FromSeconds(30));

        Exception exception = await exceptionTask;

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(WarningsBaselineBuildConstant.BUILD_BINLOG_NOT_FOUND, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunShouldSupportExplicitBuildTargetAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldSupportExplicitBuildTargetAsync)}"
        );
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProjectWithWarnings",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        string result = await WarningsBaselineInitCommand.RunAsync(
            executionContext: context,
            [
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "MySimpleProjectTemplate.csproj",
            ]
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
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        Exception? exception = await Record.ExceptionAsync(() => WarningsBaselineInitCommand.RunAsync(
            executionContext: context,
            [
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.INIT,
                CommandArgumentConstant.TARGET,
                "../outside.csproj",
            ]
        ));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(WarningsBaselineBuildConstant.INVALID_BUILD_TARGET_PATH, exception.Message);
    }

    private static async Task DeleteFileWhileTaskIsRunningAsync(IAsyncResult task, string filePath, TimeSpan timeout)
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
        catch (IOException ex)
        {
            // Best-effort cleanup during concurrent test execution; retry loop handles transient failures.
            _ = ex;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Best-effort cleanup during concurrent test execution; retry loop handles transient failures.
            _ = ex;
        }
    }
}
