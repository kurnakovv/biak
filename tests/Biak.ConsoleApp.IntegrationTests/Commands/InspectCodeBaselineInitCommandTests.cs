// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class InspectCodeBaselineInitCommandTests
{
#pragma warning disable IDE1006 // Naming Styles
    private static readonly string TEST_OUTPUT = InspectCodeBaselineInitCommandConstant.INIT_STARTED
#pragma warning restore IDE1006 // Naming Styles
        + Environment.NewLine
        + InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE
        + Environment.NewLine
        + InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS;

    [Fact]
    public async Task RunShouldOutputBaselineToConsoleAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(InspectCodeBaselineInitCommandTests)}_{nameof(RunShouldOutputBaselineToConsoleAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

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

            string result = await InspectCodeBaselineInitCommand.RunAsync();

            string actualOutput = output.ToString().Trim();

            Assert.Equal(InspectCodeBaselineCommandTestConstants.BASELINE_FILTERS, result.Trim());
            Assert.Equal(TEST_OUTPUT, actualOutput);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
