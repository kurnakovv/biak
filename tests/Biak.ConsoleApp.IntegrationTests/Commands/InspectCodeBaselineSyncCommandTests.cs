// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
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

            Assert.Equal(InspectCodeBaselineSyncCommandConstant.ALL_ISSUES_FIXED, result);
            Assert.DoesNotContain(
                $"resharper_replace_with_string_is_null_or_empty_highlighting = suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                syncedBaselineContent,
                StringComparison.OrdinalIgnoreCase
            );
            Assert.DoesNotContain(
                $"resharper_convert_to_auto_property_highlighting = suggestion {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}",
                syncedBaselineContent,
                StringComparison.OrdinalIgnoreCase
            );
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
