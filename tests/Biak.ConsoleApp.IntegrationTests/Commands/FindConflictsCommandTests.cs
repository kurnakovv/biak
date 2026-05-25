// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class FindConflictsCommandTests
{
    private const string DEFAULT_START_TEXT = $"""
        {SharedFindCommandConstant.ENTER_CRITERIA}
        {SharedFindCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindConflictsCommandConstant.BRANCHES_INPUT}
        {FindConflictsCommandConstant.START}
        {FindConflictsCommandConstant.CONFLICTING_FILES}
        """;

    [Theory]
    [InlineData(
        "NoConflicts",
        "main\ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        {SharedFindCommandConstant.NO_ENTRIES}
        
        """,
        false,
        null
    )]
    [InlineData(
        "RunDotnetFormatForAll",
        "main\ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        TestService1.cs
        [test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]


        """,
        true,
        null
    )]
    [InlineData(
        "ChangeFilesForConflict",
        "main\ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        TestService1.cs
        [test-f-1]

        TestService2.cs
        [test-f-2]

        TestService3.cs
        [test-f-3]


        """,
        false,
        "TestService1.cs TestService2.cs TestService3.cs"
    )]
    [InlineData(
        "ChangeSingleFileForConflict",
        "main\ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        TestService1.cs
        [test-f-1]


        """,
        false,
        "TestService1.cs"
    )]
    [InlineData(
        "ChangeSingleFileForConflict",
        "  main  \ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        {SharedFindCommandConstant.NO_ENTRIES}

        """,
        false,
        "TestService4.cs TestService5.cs TestService6.cs TestService7.cs TestService8.cs TestService9.cs"
    )]
    [InlineData(
        "DefaultOrInvalidInput",
        "\n\ntest-f-1 test-f-2 test-f-3\n",
        $"""
        {SharedFindCommandConstant.ENTER_CRITERIA}
        {SharedFindCommandConstant.DEFAULT_BRANCH_INPUT}
        {FindConflictsCommandConstant.BRANCHES_INPUT}
        {FindConflictsCommandConstant.INVALID_BRANCHES_FORMAT}
        {FindConflictsCommandConstant.BRANCHES_INPUT}
        {FindConflictsCommandConstant.START}
        {FindConflictsCommandConstant.CONFLICTING_FILES}
        {SharedFindCommandConstant.NO_ENTRIES}

        """,
        false,
        null
    )]
    [InlineData(
        "InvalidBranches",
        "\ntest-f-1 test-f-2 bvcbvcbcxb origin/dfdfasdfssdfasdfa origin/dfdfasdfssdfasdfa/dffasdfs test-f-3\n",
        $"""
        {DEFAULT_START_TEXT}
        {SharedFindCommandConstant.NO_ENTRIES}

        {FindConflictsCommandConstant.NOT_FOUND_BRANCHES}
        bvcbvcbcxb origin/dfdfasdfssdfasdfa origin/dfdfasdfssdfasdfa/dffasdfs

        """,
        false,
        null
    )]
    [InlineData(
        "NoChangesBranch",
        "\nempty-branch\n",
        $"""
        {DEFAULT_START_TEXT}
        {SharedFindCommandConstant.NO_ENTRIES}

        """,
        false,
        null
    )]
    [InlineData(
        "LocalChangesDetected",
        "",
        "",
        false,
        null
    )]
    public async Task RunTestAsync(string name, string inputText, string expectedOutputText, bool runDotnetFormat, string? filePathsToChangeInput)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(FindConflictsCommandTests)}_{nameof(RunTestAsync)}_{name}");

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

            await GitRepository.MockAsync();

            if (runDotnetFormat)
            {
                _ = await RunDotnetAsync();
                await GitHelper.RunAsync("add .");
                await GitHelper.RunAsync("commit -m \"Update after dotnet format\"");
            }

            if (filePathsToChangeInput != null)
            {
                foreach (string filePath in filePathsToChangeInput.Split(" ", StringSplitOptions.TrimEntries))
                {
                    await File.WriteAllTextAsync(filePath, "TestContent");
                }
                await GitHelper.RunAsync("add .");
                await GitHelper.RunAsync("commit -m \"Update after file changes\"");
            }

            if (string.IsNullOrEmpty(inputText) && string.IsNullOrEmpty(expectedOutputText))
            {
                await File.WriteAllTextAsync("TestService1.cs", "TestContent");
                await GitHelper.RunAsync("add .");
                await File.WriteAllTextAsync("TestService2.cs", "TestContent");

                await Assert.ThrowsAsync<BiakApplicationException>(async () => await FindConflictsCommand.RunAsync());
                return;
            }

            await FindConflictsCommand.RunAsync();

            string result = output.ToString();
            result = Regex.Replace(result, $@"({FindConflictsCommandConstant.CONFLICTING_FILES})\s*\[.*?\]", "$1");

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

    [Fact]
    public async Task RunShouldThrowWhenBranchesInputReachedEofAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(FindConflictsCommandTests)}_{nameof(RunShouldThrowWhenBranchesInputReachedEofAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;

        using StringReader input = new("main\n");

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

            await GitRepository.MockAsync();

            await Assert.ThrowsAsync<OperationCanceledException>(
                FindConflictsCommand.RunAsync
            );
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenInvalidMergeAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(FindConflictsCommandTests)}_{nameof(RunShouldThrowWhenInvalidMergeAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\nunrelated\n");
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

            await GitRepository.MockAsync();

            await GitHelper.RunAsync("checkout --orphan unrelated");
            await File.WriteAllTextAsync("orphan.txt", "test");
            await GitHelper.RunAsync("add .");
            await GitHelper.RunAsync("commit -m orphan");
            await GitHelper.RunAsync("checkout main");

            await Assert.ThrowsAsync<BiakApplicationException>(
                FindConflictsCommand.RunAsync
            );
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static async Task<string> RunDotnetAsync()
    {
        using Process process = new();

        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = " format --exclude-diagnostics IDE0130";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask);

        await process.WaitForExitAsync();

        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to run dotnet format command. Error message: '{error}'");
        }

        return output;
    }
}
