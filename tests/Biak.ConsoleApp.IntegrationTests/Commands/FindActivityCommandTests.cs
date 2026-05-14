// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class FindActivityCommandTests
{
    private const string DEFAULT_INPUT_OUTPUT = $"""
        {SharedFindCommandConstant.ENTER_CRITERIA}
        {SharedFindCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.ABOUT_FILE_TYPES}
        {FindActivityCommandConstant.FILE_TYPES_INPUT}
        {FindActivityCommandConstant.FILE_EXTENSIONS_INPUT}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT}
        {FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT}
        {FindActivityCommandConstant.SAVE_OUTPUT_INPUT}
        """;

    [Theory]
    [InlineData(
        "DefaultCase",
        "main\n-10\ndfassfds\n35\nMD\n.cs,.vb\nbranch-for-exclude\n\nfdsdfsdasf\ntrue\n",
        $"""
        {SharedFindCommandConstant.ENTER_CRITERIA}
        {SharedFindCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.ABOUT_FILE_TYPES}
        {FindActivityCommandConstant.FILE_TYPES_INPUT}
        {FindActivityCommandConstant.FILE_EXTENSIONS_INPUT}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT}
        {FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT}
        {FindActivityCommandConstant.SAVE_OUTPUT_INPUT}
        {FindActivityCommandConstant.INVALID_SAVE_OUTPUT_FORMAT}
        {FindActivityCommandConstant.SAVE_OUTPUT_INPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        change-testservice1 test-f-1 test-f-2 test-f-3

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        f-new-cs-file no-cs-file-changes old-branch

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        TestService1.cs,TestService2.cs,TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        TestService1.cs TestService2.cs TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "TestService1.cs"
            + ",TestService2.cs"
            + ",TestService3.cs";

        """,
        null,
        true
    )]
    [InlineData(
        "IncludeSpecificFiles",
        "main\n35\nMD\n.cs,.vb\nbranch-for-exclude\nTestService1.cs,TestService2.cs\n\n",
        $"""
        {DEFAULT_INPUT_OUTPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        change-testservice1 test-f-1 test-f-2

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        f-new-cs-file no-cs-file-changes old-branch test-f-3

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        TestService1.cs,TestService2.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        TestService1.cs TestService2.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "TestService1.cs"
            + ",TestService2.cs";

        """,
        null,
        false
    )]
    [InlineData(
        "DisableAllFilters",
        "main\n*\n*\n*\n\n\n\n",
        $"""
        {DEFAULT_INPUT_OUTPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        NewTestFile.cs
        [branch-for-exclude f-new-cs-file]

        TestService1.cs
        [branch-for-exclude change-testservice1 test-f-1]

        .gitattributes
        [no-cs-file-changes]

        TestService9.cs
        [old-branch]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        branch-for-exclude f-new-cs-file change-testservice1 test-f-1 no-cs-file-changes old-branch test-f-2 test-f-3

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        {SharedFindCommandConstant.NO_ENTRIES}

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        NewTestFile.cs,TestService1.cs,.gitattributes,TestService9.cs,TestService2.cs,TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        NewTestFile.cs TestService1.cs .gitattributes TestService9.cs TestService2.cs TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "NewTestFile.cs"
            + ",TestService1.cs"
            + ",.gitattributes"
            + ",TestService9.cs"
            + ",TestService2.cs"
            + ",TestService3.cs";

        """,
        null,
        false
    )]
    [InlineData(
        "ExcludeBranches",
        "main\n*\n*\n*\ntest-f-* *-test *change* test\n\n\n",
        $"""
        {DEFAULT_INPUT_OUTPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        NewTestFile.cs
        [branch-for-exclude f-new-cs-file]
        
        TestService1.cs
        [branch-for-exclude]
        
        TestService9.cs
        [old-branch]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        branch-for-exclude f-new-cs-file old-branch

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        {SharedFindCommandConstant.NO_ENTRIES}

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        NewTestFile.cs,TestService1.cs,TestService9.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        NewTestFile.cs TestService1.cs TestService9.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "NewTestFile.cs"
            + ",TestService1.cs"
            + ",TestService9.cs";

        """,
        null,
        false
    )]
    [InlineData(
        "ConfigWithDefaultValues",
        "",
        $"""
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [branch-for-exclude change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        branch-for-exclude change-testservice1 test-f-1 test-f-2 test-f-3

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        f-new-cs-file no-cs-file-changes old-branch

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        TestService1.cs,TestService2.cs,TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        TestService1.cs TestService2.cs TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "TestService1.cs"
            + ",TestService2.cs"
            + ",TestService3.cs";

        """,
        "default-config.json",
        false
    )]
    [InlineData(
        "InvalidExpirationPeriodInConfig",
        "30\n",
        $"""
        {BiakConfigConstant.SEVERITIES_TO_DISABLE_DUPLICATES}
        {FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT_IN_CONFIG}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [branch-for-exclude change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]

        {FindActivityCommandConstant.ACTIVE_BRANCHES}
        branch-for-exclude change-testservice1 test-f-1 test-f-2 test-f-3

        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        f-new-cs-file no-cs-file-changes old-branch

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        TestService1.cs,TestService2.cs,TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        TestService1.cs TestService2.cs TestService3.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "TestService1.cs"
            + ",TestService2.cs"
            + ",TestService3.cs";

        """,
        "invalid-expiration-period-config.json",
        true
    )]
    public async Task RunTestAsync(string name, string inputText, string expectedOutputText, string? configFilePath, bool saveOutput)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(FindActivityCommandTests)}_{nameof(RunTestAsync)}_{name}");

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new(inputText);
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            if (configFilePath != null)
            {
                string templateConfigPath = Path.Join(
                    AppContext.BaseDirectory,
                    "Templates",
                    "FindActivityConfigs",
                    configFilePath
                );
                string biakDir = Path.Join(testDir.Value, ".biak");
                Directory.CreateDirectory(biakDir);
                File.Copy(
                    sourceFileName: templateConfigPath,
                    destFileName: Path.Join(biakDir, "config.json"),
                    overwrite: true
                );
            }

            await GitRepository.MockAsync();

            await FindActivityCommand.RunAsync();

            string result = output.ToString();
            result = Regex.Replace(result, $@"({FindActivityCommandConstant.ACTIVITY})\s*\[.*?\]", "$1");

            Assert.NotEmpty(result);
            Assert.Equal(expectedOutputText, result);

            string logsDir = Path.Join(Directory.GetCurrentDirectory(), ".biak", "logs");
            if (saveOutput)
            {
                Assert.True(Directory.Exists(logsDir));
                string? logFilePath = Directory.GetFiles(logsDir).FirstOrDefault(x => x.EndsWith(".txt"));
                Assert.NotNull(logFilePath);
                string logFileContent = await File.ReadAllTextAsync(logFilePath);
                logFileContent = Regex.Replace(logFileContent, $@"({FindActivityCommandConstant.ACTIVITY})\s*\[.*?\]", "$1");
                Assert.Contains(logFileContent, result, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.False(Directory.Exists(logsDir));
            }
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
