// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.FindActivity;

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
        await GitHelper.RunAsync("status");

        FindActivityInputModel input = FindActivityInputHelper.Request();

        Console.WriteLine(FindActivityCommandConstant.START);
        string branchOutput = await GitHelper.RunAsync($"branch --no-merged {input.DefaultBranch}");
        string remoteBranchOutput = await GitHelper.RunAsync($"branch -r --no-merged {input.DefaultBranch}");

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

        if (input.ExcludeBranches != null)
        {
            allBranches = allBranches.Where(b => !(
                input.ExcludeBranches.Any(eb => eb == b) ||
                input.ExcludeBranches.Any(x => FindActivityCommandConstant.ORIGIN_PREFIX + x == b)
           ));

            allBranches = allBranches.Where(b =>
                !input.ExcludeBranches.Any(pattern =>
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

            if (input.ExpirationPeriod != null && lastCommitDate < DateTimeOffset.UtcNow.AddDays(-(double)input.ExpirationPeriod))
            {
                inactiveBranches.Add(branch);
                continue;
            }

            string diffTypesInput = $"diff {input.DefaultBranch}...{branch} --name-only";
            if (input.FileTypes != null)
            {
                diffTypesInput += $" --diff-filter={input.FileTypes}";
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

            if (input.FileExtensions.Any())
            {
                diffFiles = diffFiles.Where(f => input.FileExtensions.Any(e => f.EndsWith(e)));
            }

            if (input.IncludedFilePaths != null)
            {
                diffFiles = diffFiles.Where(x => input.IncludedFilePaths.Contains(x));
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

        FindActivityOutputHelper.Print(activity, inactiveBranches);
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
