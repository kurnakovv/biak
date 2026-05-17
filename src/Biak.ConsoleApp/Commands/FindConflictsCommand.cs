// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.FindConflicts;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak find-conflicts` command.
/// </summary>
public static class FindConflictsCommand
{
    private static readonly char[] s_separator = new[] { '\r', '\n' };

    /// <summary>
    /// Can `dotnet biak find-conflicts` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args.Length == 1 && args[0] == CommandArgumentConstant.FIND_CONFLICTS;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync()
    {
        string workingTreeStatus = await GitHelper.RunAsync("status --porcelain");

        if (!string.IsNullOrWhiteSpace(workingTreeStatus))
        {
            Console.WriteLine(FindConflictsCommandConstant.LOCAL_CHANGES_DETECTED);
            return;
        }

        FindConflictsInputModel input = FindConflictsInputHelper.Request();
        Console.WriteLine(FindConflictsCommandConstant.START);

        string originalBranch = (await GitHelper.RunAsync("branch --show-current")).Trim();

        try
        {
            await GitHelper.RunAsync($"checkout {input.DefaultBranch}");
            Dictionary<string, List<string>> allConflictFiles = new();
            List<string> notFoundBranches = new();

            foreach (string branch in input.Branches)
            {
                string isBranchExistsOutput = await GitHelper.RunAsync($"branch -a -l {branch}");

                if (string.IsNullOrWhiteSpace(isBranchExistsOutput))
                {
                    notFoundBranches.Add(branch);
                    continue;
                }

                GitResult mergeGitResult = await GitHelper.RunWithModelAsync($"merge --no-commit --no-ff {branch}");

                if (mergeGitResult.ExitCode != 0 && !mergeGitResult.Output.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("GIT ERROR: " + mergeGitResult.Error);
                    Environment.Exit(1);
                }

                string conflictFilesOutput = await GitHelper.RunAsync("diff --name-only --diff-filter=U");
                if (!string.IsNullOrWhiteSpace(conflictFilesOutput))
                {
                    IEnumerable<string> currentConflictFiles = conflictFilesOutput
                        .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim());

                    foreach (string conflictFile in currentConflictFiles)
                    {
                        if (!allConflictFiles.TryGetValue(conflictFile, out List<string>? list))
                        {
                            list = new List<string>();
                            allConflictFiles[conflictFile] = list;
                        }

                        list.Add(branch);
                    }
                }

                GitResult mergeHeadGitResult = await GitHelper.RunWithModelAsync("rev-parse -q --verify MERGE_HEAD");
                if (mergeHeadGitResult.ExitCode == 0 && !string.IsNullOrWhiteSpace(mergeHeadGitResult.Output))
                {
                    await GitHelper.RunAsync("merge --abort");
                }
            }

            _ = FindConflictsOutputHelper.Print(allConflictFiles, notFoundBranches);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(originalBranch))
            {
                await GitHelper.RunAsync($"checkout {originalBranch}");
            }
        }
    }
}
