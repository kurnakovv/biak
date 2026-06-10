// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class WarningsBaselineSyncCommandTests
{
    private const string BASELINE_EDITORCONFIG = """
        root = true

        [{VisualBasicProject/Module1.vb}]
        dotnet_diagnostic.BC40000.severity = suggestion # ^biak^ baseline

        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0108.severity = suggestion # ^biak^ baseline

        [{ProgramCS0168Warning.cs}]
        dotnet_diagnostic.CS0168.severity = suggestion # ^biak^ baseline

        [{MyClassCS0169.cs}]
        dotnet_diagnostic.CS0169.severity = suggestion # ^biak^ baseline

        [{ProgramCS0219Warning.cs}]
        dotnet_diagnostic.CS0219.severity = suggestion # ^biak^ baseline

        [{ProgramCS0612.cs}]
        dotnet_diagnostic.CS0612.severity = suggestion # ^biak^ baseline

        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0649.severity = suggestion # ^biak^ baseline

        [{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]
        dotnet_diagnostic.CS8618.severity = suggestion # ^biak^ baseline
        """;

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
            await File.WriteAllTextAsync(editorconfigPath, BASELINE_EDITORCONFIG);

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

            string result = await WarningsBaselineSyncCommand.RunAsync(
                [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, ".editorconfig"]
            );

            string syncedContent = await File.ReadAllTextAsync(editorconfigPath);
            string consoleOutput = output.ToString();

            Assert.Equal("Sync complete. Removed 1 resolved filter(s). 7 filter(s) still active.", result);
            Assert.Contains(WarningsBaselineSyncCommandConstant.SYNC_STARTED, consoleOutput, StringComparison.Ordinal);
            Assert.Contains(result, consoleOutput, StringComparison.Ordinal);
            Assert.Contains("MyTestModel.cs (CS8618)", consoleOutput, StringComparison.Ordinal);
            Assert.Contains("ProgramCS0168Warning.cs (CS0168)", consoleOutput, StringComparison.Ordinal);

            Assert.DoesNotContain("[{ProgramCS0168Warning.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("dotnet_diagnostic.CS0168.severity", syncedContent, StringComparison.Ordinal);

            Assert.Contains("[{MyTestForlder/MyTestModel1.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.DoesNotContain("[{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]", syncedContent, StringComparison.Ordinal);
            Assert.Contains(
                "dotnet_diagnostic.CS8618.severity = suggestion # ^biak^ baseline",
                syncedContent,
                StringComparison.Ordinal
            );
            Assert.DoesNotContain("= warning # ^biak^ baseline", syncedContent, StringComparison.Ordinal);
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
