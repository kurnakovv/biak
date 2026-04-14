// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

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
        string branchOutput = await GitHelper.RunAsync("branch");
        string remoteBranchOutput = await GitHelper.RunAsync("branch -r");

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

        foreach (string item in allBranches)
        {
            Console.WriteLine(item);
        }
    }
}
