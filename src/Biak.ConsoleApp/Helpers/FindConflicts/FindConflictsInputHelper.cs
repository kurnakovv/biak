// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers.FindConflicts;

internal static class FindConflictsInputHelper
{
    internal static FindConflictsInputModel Request()
    {
        Console.WriteLine(SharedFindCommandConstant.ENTER_CRITERIA);

        Console.Write(SharedFindCommandConstant.DEFAULT_BRANCH_INPUT);
        string? defaultBranch = Console.ReadLine();
        defaultBranch = string.IsNullOrWhiteSpace(defaultBranch) ? "main" : defaultBranch.Trim();

        Console.WriteLine();

        IEnumerable<string> branches;

        while (true)
        {
            Console.Write(FindConflictsCommandConstant.BRANCHES_INPUT);
            string branchesInput = Console.ReadLine()
                ?? throw new OperationCanceledException("Input stream closed.");

            if (string.IsNullOrWhiteSpace(branchesInput))
            {
                Console.WriteLine();
                Console.WriteLine(FindConflictsCommandConstant.INVALID_BRANCHES_FORMAT);
                continue;
            }

            branches = branchesInput.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            break;
        }

        Console.WriteLine();

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
