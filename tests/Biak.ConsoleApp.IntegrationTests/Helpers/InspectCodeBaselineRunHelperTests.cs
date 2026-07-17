// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class InspectCodeBaselineRunHelperTests
{
    [Fact]
    public async Task RunAsyncWhenInspectCodeFailsShouldThrowBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenInspectCodeFailsShouldThrowBiakApplicationExceptionAsync)}"
        );

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
            InspectCodeBaselineRunHelper.RunAsync(targetPath));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenTargetNotFoundShouldThrowBiakApplicationExceptionAsync()
    {
        string nonExistentPath = Path.Join(Path.GetTempPath(), "nonexistent-biak-test", "Missing.csproj");

        Exception? exception = await Record.ExceptionAsync(() =>
            InspectCodeBaselineRunHelper.RunAsync(nonExistentPath));

        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.StartsWith(InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_PREFIX, exception.Message, StringComparison.Ordinal);
        Assert.Contains(nonExistentPath, exception.Message, StringComparison.Ordinal);
        Assert.EndsWith(InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_SUFFIX, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsyncWhenNoSolutionOrProjectFoundShouldThrowBiakApplicationExceptionAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenNoSolutionOrProjectFoundShouldThrowBiakApplicationExceptionAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Exception? exception = await Record.ExceptionAsync(() =>
                InspectCodeBaselineRunHelper.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND, exception.Message);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenSlnxFileExistsShouldAutoDiscoverItAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenSlnxFileExistsShouldAutoDiscoverItAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            await File.WriteAllTextAsync(Path.Join(testDir.Value, "fake.slnx"), "invalid content");

            Exception? exception = await Record.ExceptionAsync(() =>
                InspectCodeBaselineRunHelper.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunAsyncWhenSlnFileExistsShouldAutoDiscoverItAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenSlnFileExistsShouldAutoDiscoverItAsync)}"
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            await File.WriteAllTextAsync(Path.Join(testDir.Value, "fake.sln"), "invalid content");

            Exception? exception = await Record.ExceptionAsync(() =>
                InspectCodeBaselineRunHelper.RunAsync());

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
