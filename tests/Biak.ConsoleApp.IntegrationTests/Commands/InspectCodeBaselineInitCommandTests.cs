// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class InspectCodeBaselineInitCommandTests
{
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

            Assert.NotEmpty(result);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
