// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.FindConflicts;

internal static class FindConflictsInputHelper
{
    internal static async Task<FindConflictsInputModel> RequestAsync(AppExecutionContext executionContext)
    {
        await executionContext.Out.WriteLineAsync(SharedFindCommandConstant.ENTER_CRITERIA);

        await executionContext.Out.WriteAsync(SharedFindCommandConstant.DEFAULT_BRANCH_INPUT);
        string? defaultBranch = await executionContext.In.ReadLineAsync();
        defaultBranch = string.IsNullOrWhiteSpace(defaultBranch) ? "main" : defaultBranch.Trim();

        await executionContext.Out.WriteLineAsync();

        IEnumerable<string> branches;

        while (true)
        {
            await executionContext.Out.WriteAsync(FindConflictsCommandConstant.BRANCHES_INPUT);
            string branchesInput = await executionContext.In.ReadLineAsync()
                ?? throw new OperationCanceledException("Input stream closed.");

            if (string.IsNullOrWhiteSpace(branchesInput))
            {
                await executionContext.Out.WriteLineAsync();
                await executionContext.Out.WriteLineAsync(FindConflictsCommandConstant.INVALID_BRANCHES_FORMAT);
                continue;
            }

            branches = branchesInput.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            break;
        }

        await executionContext.Out.WriteLineAsync();

        return new FindConflictsInputModel(
            defaultBranch: defaultBranch,
            branches: branches
        );
    }
}

internal class FindConflictsInputModel
{
    internal FindConflictsInputModel(
        string defaultBranch,
        IEnumerable<string> branches
    )
    {
        DefaultBranch = defaultBranch;
        Branches = branches;
    }

    internal string DefaultBranch { get; } = null!;
    internal IEnumerable<string> Branches { get; }
}
