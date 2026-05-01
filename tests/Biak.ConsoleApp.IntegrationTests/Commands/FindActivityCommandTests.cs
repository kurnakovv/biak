// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class FindActivityCommandTests
{
    private const string DEFAULT_INPUT_OUTPUT = $"""
        {FindActivityCommandConstant.ENTER_CRITERIA}
        {FindActivityCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.ABOUT_FILE_TYPES}
        {FindActivityCommandConstant.FILE_TYPES_INPUT}
        {FindActivityCommandConstant.FILE_EXTENSIONS_INPUT}
        {FindActivityCommandConstant.EXCLUDE_BRANCES_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT}
        {FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT}
        """;

    [Theory]
    [InlineData(
        "DefaultCase",
        "main\n-10\ndfassfds\n35\nMD\n.cs,.vb\nbranch-for-exclude\n\n",
        $"""
        {FindActivityCommandConstant.ENTER_CRITERIA}
        {FindActivityCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.INVALID_EXPIRATION_PERIOD_FORMAT}
        {FindActivityCommandConstant.EXPIRATION_PERIOD_INPUT}
        {FindActivityCommandConstant.ABOUT_FILE_TYPES}
        {FindActivityCommandConstant.FILE_TYPES_INPUT}
        {FindActivityCommandConstant.FILE_EXTENSIONS_INPUT}
        {FindActivityCommandConstant.EXCLUDE_BRANCES_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_FILTER_EXAMPLE}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR}
        {FindActivityCommandConstant.EXCLUDE_BRANCHES_INPUT}
        {FindActivityCommandConstant.INCLUDE_FILE_PATHS_INPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]


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
        null
    )]
    [InlineData(
        "IncludeSpecificFiles",
        "main\n35\nMD\n.cs,.vb\nbranch-for-exclude\nTestService1.cs,TestService2.cs\n",
        $"""
        {DEFAULT_INPUT_OUTPUT}
        {FindActivityCommandConstant.START}
        {FindActivityCommandConstant.ACTIVITY}
        TestService1.cs
        [change-testservice1 test-f-1]

        TestService2.cs
        [test-f-2]


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
        null
    )]
    [InlineData(
        "DisableAllFilters",
        "main\n*\n*\n*\n\n\n",
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


        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        {FindActivityCommandConstant.NO_ENTRIES}

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
        null
    )]
    [InlineData(
        "ExcludeBranches",
        "main\n*\n*\n*\ntest-f-* *-test *change* test\n\n",
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


        {FindActivityCommandConstant.INACTIVE_BRANCHES}
        {FindActivityCommandConstant.NO_ENTRIES}

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE}
        NewTestFile.cs,TestService1.cs,TestService9.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE}
        NewTestFile.cs TestService1.cs TestService9.cs

        {FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE}
        ^biak^ var activeFiles = "NewTestFile.cs"
            + ",TestService1.cs"
            + ",TestService9.cs";

        """,
        null
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
        "default-config.json"
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
        "invalid-expiration-period-config.json"
    )]
    public async Task RunTestAsync(string name, string inputText, string expectedOutputText, string? configFilePath)
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

            await MockGitRepositoryAsync();

            await FindActivityCommand.RunAsync();

            string result = output.ToString();
            result = Regex.Replace(result, $@"({FindActivityCommandConstant.ACTIVITY})\s*\[.*?\]", "$1");

            Assert.NotEmpty(result);
            Assert.Equal(expectedOutputText, result);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static async Task MockGitRepositoryAsync()
    {
        await GitHelper.RunAsync("init");

        await GitHelper.RunAsync("config --local user.email \"test@example.com\"");
        await GitHelper.RunAsync("config --local user.name \"Test User\"");

        await GitHelper.RunAsync("branch -m master main");

        await GitHelper.RunAsync("add .");

        await GitHelper.RunAsync("commit -m \"Initial commit\"");

        string defaultBranch = (await GitHelper.RunAsync("branch --show-current")).Trim();
        if (string.IsNullOrEmpty(defaultBranch))
        {
            defaultBranch = "master";
        }

        for (int i = 1; i <= 3; i++)
        {
            string featureBranch = $"test-f-{i}";
            await GitHelper.RunAsync($"checkout -b {featureBranch}");

            string testServicePath = Path.Join($"TestService{i}.cs");

            string serviceContent = await File.ReadAllTextAsync(testServicePath);
            serviceContent = serviceContent.Replace(
                "Console.WriteLine(\"2\");",
                "Console.WriteLine(\"2\");Console.WriteLine(\"test\");",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(testServicePath, serviceContent);

            await GitHelper.RunAsync("add .");

            await GitHelper.RunAsync($"commit -m \"Update TestService{i}\"");

            await GitHelper.RunAsync($"checkout {defaultBranch}");
        }

        await GitHelper.RunAsync("checkout -b empty-branch");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b no-cs-file-changes");
        string gitattributesPath = Path.Join(".gitattributes");
        string gitattributesContent = await File.ReadAllTextAsync(gitattributesPath);
        gitattributesContent += " ";
        await File.WriteAllTextAsync(gitattributesPath, gitattributesContent);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update .gitattributes\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b old-branch");
        string testService9Path = Path.Join("TestService9.cs");
        string testService9Content = await File.ReadAllTextAsync(testService9Path);
        testService9Content += " ";
        await File.WriteAllTextAsync(testService9Path, testService9Content);
        await GitHelper.RunAsync("add .");
        string? originCommitterDate = Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
        try
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", "2021-01-01 12:12:00");
            await GitHelper.RunAsync("commit --date=\"2021-01-01 12:12:00\" -m \"Update TestService9\"");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", originCommitterDate);
        }
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b change-testservice1");
        string testService1Path = Path.Join("TestService1.cs");
        string testService1Content = await File.ReadAllTextAsync(testService1Path);
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update TestService1\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b f-new-cs-file");
        string newTestFilePath = Path.Join("NewTestFile.cs");
        await File.WriteAllTextAsync(newTestFilePath, "// Test");
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Add NewTestFile.cs\"");

        await GitHelper.RunAsync("checkout -b branch-for-exclude");
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update TestService1\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync($"checkout {defaultBranch}");
    }
}
