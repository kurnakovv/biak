// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak find-activity` command.
/// </summary>
public static class FindActivityCommand
{
    private static readonly char[] s_separator = new[] { '\r', '\n' };

    /// <summary>
    /// Can `dotnet biak find-activity` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args.Length == 1 && args[0] == CommandArgumentConstant.FIND_ACTIVITY;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("Start find activity...");
        string branchOutput = await GitHelper.RunAsync("branch --no-merged");
        string remoteBranchOutput = await GitHelper.RunAsync("branch -r --no-merged");

        IEnumerable<string> branches = branchOutput
            .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !x.TrimStart().StartsWith('*'))
            .Select(x => x.Trim());

        IEnumerable<string> remoteBranches = remoteBranchOutput
            .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
#pragma warning disable CA1307 // Specify StringComparison for clarity
            .Where(x => !x.Contains("->")); // StringComparison only for char
#pragma warning restore CA1307 // Specify StringComparison for clarity

        string origin = "origin/";

        IEnumerable<string> allBranches = branches
            .Concat(remoteBranches)
            .GroupBy(x => x.StartsWith(origin) ? x.Substring(origin.Length) : x)
            .Select(g => g.First());

        List<string> inactiveBranches = new();
        Dictionary<string, List<string>> activity = new();

        foreach (string branch in allBranches)
        {
            string lastCommitDateOutput = await GitHelper.RunAsync($"log {branch} -1 --format=%cd --date=iso-strict");
            DateTimeOffset lastCommitDate = DateTimeOffset.Parse(lastCommitDateOutput, CultureInfo.InvariantCulture);

            if (lastCommitDate < DateTimeOffset.Now.AddMonths(-1))
            {
                inactiveBranches.Add(branch);
                continue;
            }

            string hash = (await GitHelper.RunAsync($"merge-base HEAD {branch}")).Trim();
            string diffFilesOutput = await GitHelper.RunAsync($"diff {hash}..{branch} --name-only --diff-filter=MDR");
            if (string.IsNullOrWhiteSpace(diffFilesOutput))
            {
                inactiveBranches.Add(branch);
                continue;
            }

            IEnumerable<string> diffFiles = diffFilesOutput
                .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.EndsWith(".cs"));

            if (!diffFiles.Any())
            {
                inactiveBranches.Add(branch);
                continue;
            }

            foreach (string file in diffFiles)
            {
                if (!activity.TryGetValue(file, out List<string>? list))
                {
                    list = new List<string>();
                    activity[file] = list;
                }

                list.Add(branch);
            }
        }

        Console.WriteLine("Activity");
        if (activity.Count != 0)
        {
            foreach ((string file, List<string> activeBranches) in activity)
            {
                Console.WriteLine($"{file} [{string.Join(" ", activeBranches)}]");
            }
        }
        else
        {
            Console.WriteLine("Empty");
        }

        Console.WriteLine();
        Console.WriteLine("Inactive branches");
        if (inactiveBranches.Count != 0)
        {
            Console.WriteLine(string.Join(" ", inactiveBranches));
        }
        else
        {
            Console.WriteLine("Empty");
        }

        List<string> keys = activity.Keys.ToList();

        Console.WriteLine();
        Console.WriteLine("All active files in single line");
        if (keys.Count != 0)
        {
            Console.WriteLine(string.Join(",", keys));
        }
        else
        {
            Console.WriteLine("Empty");
        }

        Console.WriteLine();
        Console.WriteLine("All active files via variable");

        if (keys.Count != 0)
        {
            string result = "var activeFiles = " +
                string.Join(
                    "\n    + ",
                    keys.Select((key, index) => $"\"{key}\"")
                );

            result += ";";

            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine("Empty");
        }
    }
}
