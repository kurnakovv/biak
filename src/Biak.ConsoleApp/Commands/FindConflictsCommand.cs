// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.FindConflicts;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak find-coflicts` command.
/// </summary>
public static class FindConflictsCommand
{
    private static readonly char[] s_separator = new[] { '\r', '\n' };

    /// <summary>
    /// Can `dotnet biak find-coflicts` command be run.
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
        await GitHelper.RunAsync("status");

        FindConflictsInputModel input = FindConflictsInputHelper.Request();
        Console.WriteLine(FindConflictsCommandConstant.START);

        await GitHelper.RunAsync($"checkout {input.DefaultBranch}");
        Dictionary<string, List<string>> allConflictFiles = new();

        foreach (string branch in input.Branches)
        {
            await GitHelper.RunAsync($"merge --no-commit --no-ff {branch}", ignoreExitCode: true);

            string conflictFilesOutput = await GitHelper.RunAsync($"diff --name-only --diff-filter=U");
            if (!string.IsNullOrWhiteSpace(conflictFilesOutput))
            {
                IEnumerable<string> curentConflictFiles = conflictFilesOutput
                    .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());

                foreach (string conflictFile in curentConflictFiles)
                {
                    if (!allConflictFiles.TryGetValue(conflictFile, out List<string>? list))
                    {
                        list = new List<string>();
                        allConflictFiles[conflictFile] = list;
                    }

                    list.Add(branch);
                }
            }

            await GitHelper.RunAsync($"merge --abort");
        }

        _ = FindConflictsOutputHelper.Print(allConflictFiles);
    }
}
