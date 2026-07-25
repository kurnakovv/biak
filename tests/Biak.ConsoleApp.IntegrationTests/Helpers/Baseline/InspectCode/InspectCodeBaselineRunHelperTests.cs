// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Helpers.Baseline.InspectCode;

public class InspectCodeBaselineRunHelperTests
{
    [Fact]
    public async Task RunAsyncWhenInspectCodeFailsShouldThrowBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenInspectCodeFailsShouldThrowBiakApplicationExceptionAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        string targetPath = Path.Join(testDir.Value, "InspectCodeBaselineTemplate.csproj");
        await File.WriteAllTextAsync(targetPath, "this is not valid xml");

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(context, targetPath));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenTargetNotFoundShouldThrowBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenTargetNotFoundShouldThrowBiakApplicationExceptionAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string nonExistentPath = Path.Join(Path.GetTempPath(), "nonexistent-biak-test", "Missing.csproj");

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(context, nonExistentPath));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_PREFIX, exception.Message, StringComparison.Ordinal);
        Assert.Contains(nonExistentPath, exception.Message, StringComparison.Ordinal);
        Assert.EndsWith(InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_SUFFIX, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenNoSolutionOrProjectFoundShouldThrowBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenNoSolutionOrProjectFoundShouldThrowBiakApplicationExceptionAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(executionContext: context));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);
    }

    [Fact]
    public async Task RunAsyncWhenSlnxFileExistsShouldAutoDiscoverItAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenSlnxFileExistsShouldAutoDiscoverItAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        await File.WriteAllTextAsync(Path.Join(testDir.Value, "fake.slnx"), "invalid content");

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(executionContext: context));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenSlnFileExistsShouldAutoDiscoverItAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenSlnFileExistsShouldAutoDiscoverItAsync)}"
        );
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        await File.WriteAllTextAsync(Path.Join(testDir.Value, "fake.sln"), "invalid content");

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(executionContext: context));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
    }
}
