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
    public async Task RunAsyncWhenInspectCodeIsNotAvailableShouldThrowBiakApplicationExceptionAsync()
    {
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineRunHelperTests)}_{nameof(RunAsyncWhenInspectCodeIsNotAvailableShouldThrowBiakApplicationExceptionAsync)}"
        );

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        string targetPath = Path.Join(testDir.Value, "InspectCodeBaselineTemplate.csproj");

        string originalPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        Environment.SetEnvironmentVariable("PATH", string.Empty);

        try
        {
            Exception? exception = await Record.ExceptionAsync(() =>
                InspectCodeBaselineRunHelper.RunAsync(targetPath));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(InspectCodeBaselineRunHelperConstant.FAILED_TO_START_INSPECTCODE, exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}
