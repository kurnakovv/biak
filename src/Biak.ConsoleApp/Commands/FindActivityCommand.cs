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
        await GitHelper.RunAsync($"status");

        Console.WriteLine("Please enter the desired criteria");

        Console.Write("Default branch ('main' by default): ");
        string? defaultBranch = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(defaultBranch))
        {
            defaultBranch = "main";
        }
        Console.WriteLine();

        int? expirationPeriod = 30;
        while (true)
        {
            Console.Write("Expiration period in days (default: 30, '*' for unlimited): ");
            string? expirationPeriodInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(expirationPeriodInput))
            {
                break;
            }
            if (expirationPeriodInput == "*")
            {
                expirationPeriod = null;
                break;
            }

            if (int.TryParse(expirationPeriodInput, out int value) && value > 0)
            {
                expirationPeriod = value;
                break;
            }

            Console.WriteLine("Invalid format");
        }
        Console.WriteLine();

        Console.WriteLine("About file types https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB");
        Console.Write("File types (MDR by default, '*' all files): ");
        string? fileTypes = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fileTypes))
        {
            fileTypes = "MDR";
        }
        else if (fileTypes == "*")
        {
            fileTypes = null;
        }
        Console.WriteLine();

        Console.Write("File extensions separated by commas ('.cs' by default, '*' all files): ");
        string? fileExtensionsInput = Console.ReadLine();
        IEnumerable<string> fileExtensions = new List<string>() { ".cs" };
        if (fileExtensionsInput == "*")
        {
            fileExtensions = new List<string>();
        }
        else if (!string.IsNullOrWhiteSpace(fileExtensionsInput))
        {
            fileExtensions = fileExtensionsInput.Trim().Split(",");
        }
        Console.WriteLine();

        Console.WriteLine("Exclude specific branches separated by space (e.g., f-1 f-2).");
        Console.WriteLine("You can use '*' to select multiple similar branches (e.g., f-*).");
        Console.WriteLine("By default, no additional branches are excluded.");
        Console.Write("Exclude branches: ");
        string? excludeBranchesInput = Console.ReadLine();
        IEnumerable<string>? excludeBranches = null;
        if (!string.IsNullOrWhiteSpace(excludeBranchesInput))
        {
            excludeBranches = excludeBranchesInput.Trim().Split(" ");
        }
        Console.WriteLine();

        Console.Write("Enter file paths (comma-separated) to process only these files, others will be skipped (default: all files): ");
        string? includedFilePathsInput = Console.ReadLine();
        IEnumerable<string>? includedFilePaths = null;
        if (!string.IsNullOrWhiteSpace(includedFilePathsInput))
        {
            includedFilePaths = includedFilePathsInput.Trim().Split(",");
        }
        Console.WriteLine();

        Console.WriteLine(FindActivityCommandConstant.START);
        string branchOutput = await GitHelper.RunAsync($"branch --no-merged {defaultBranch}");
        string remoteBranchOutput = await GitHelper.RunAsync($"branch -r --no-merged {defaultBranch}");

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

        IEnumerable<string> allBranches = branches
            .Concat(remoteBranches)
            .GroupBy(
                x => x.StartsWith(FindActivityCommandConstant.ORIGIN_PREFIX)
                    ? x.Substring(FindActivityCommandConstant.ORIGIN_PREFIX.Length)
                    : x
            )
            .Select(g => g.First());

        if (excludeBranches != null)
        {
            allBranches = allBranches.Where(b => !(
                excludeBranches.Any(eb => eb == b) ||
                excludeBranches.Any(x => FindActivityCommandConstant.ORIGIN_PREFIX + x == b)
           ));

            allBranches = allBranches.Where(b =>
                !excludeBranches.Any(pattern =>
                    MatchExcludeBranch(b, pattern) ||
                    MatchExcludeBranch(b, FindActivityCommandConstant.ORIGIN_PREFIX + pattern)
                )
            );
        }

        List<string> inactiveBranches = new();
        Dictionary<string, List<string>> activity = new();

        foreach (string branch in allBranches)
        {
            string lastCommitDateOutput = await GitHelper.RunAsync($"log {branch} -1 --format=%cd --date=iso-strict");
            DateTimeOffset lastCommitDate = DateTimeOffset.Parse(lastCommitDateOutput, CultureInfo.InvariantCulture);

            if (expirationPeriod != null && lastCommitDate < DateTimeOffset.Now.AddDays(-(double)expirationPeriod))
            {
                inactiveBranches.Add(branch);
                continue;
            }

            string diffTypesInput = $"diff {defaultBranch}...{branch} --name-only";
            if (fileTypes != null)
            {
                diffTypesInput += $" --diff-filter={fileTypes}";
            }
            string diffFilesOutput = await GitHelper.RunAsync(diffTypesInput);
            if (string.IsNullOrWhiteSpace(diffFilesOutput))
            {
                inactiveBranches.Add(branch);
                continue;
            }

            IEnumerable<string> diffFiles = diffFilesOutput
                .Split(s_separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            if (fileExtensions.Any())
            {
                diffFiles = diffFiles.Where(f => fileExtensions.Any(e => f.EndsWith(e)));
            }

            if (includedFilePaths != null)
            {
                diffFiles = diffFiles.Where(x => includedFilePaths.Contains(x));
            }

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

        Console.WriteLine(FindActivityCommandConstant.ACTIVITY);
        if (activity.Count != 0)
        {
            foreach ((string file, List<string> activeBranches) in activity)
            {
                Console.WriteLine(file);
                Console.WriteLine($"[{string.Join(" ", activeBranches)}]");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine(FindActivityCommandConstant.NO_ENTRIES);
        }

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.INACTIVE_BRANCHES);
        Console.WriteLine(inactiveBranches.Count != 0 ? string.Join(" ", inactiveBranches) : FindActivityCommandConstant.NO_ENTRIES);

        List<string> keys = activity.Keys.ToList();

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE);
        Console.WriteLine(keys.Count != 0 ? string.Join(",", keys) : FindActivityCommandConstant.NO_ENTRIES);

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE);

        if (keys.Count != 0)
        {
            string result = "var activeFiles = " +
                string.Join(
                    Environment.NewLine + "    + ",
                    keys.Select(x => $"\"{x}\"")
                );

            result += ";";

            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine(FindActivityCommandConstant.NO_ENTRIES);
        }
    }

    private static bool MatchExcludeBranch(string text, string pattern)
    {
        if (pattern.StartsWith('*') && pattern.EndsWith('*'))
        {
            string value = pattern.Trim('*');
            return text.Contains(value, StringComparison.Ordinal);
        }

        if (pattern.StartsWith('*'))
        {
            string value = pattern.Substring(1);
            return text.EndsWith(value);
        }

        if (pattern.EndsWith('*'))
        {
            string value = pattern.Substring(0, pattern.Length - 1);
            return text.StartsWith(value);
        }

        return text == pattern;
    }
}
