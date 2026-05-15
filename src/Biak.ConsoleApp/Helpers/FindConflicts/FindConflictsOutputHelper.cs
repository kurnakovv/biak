// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers.FindConflicts;

internal static class FindConflictsOutputHelper
{
    internal static string Print(
        Dictionary<string, List<string>> allCoflictFiles,
        List<string> notFoundBranches
    )
    {
        StringBuilder sb = new();

        sb.AppendLine($"{FindConflictsCommandConstant.CONFLICTING_FILES} [{DateTime.UtcNow}]");

        if (allCoflictFiles.Count != 0)
        {
            foreach ((string file, List<string> conflictFiles) in allCoflictFiles)
            {
                sb.AppendLine(file);
                sb.AppendLine($"[{string.Join(" ", conflictFiles)}]");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine(SharedFindCommandConstant.NO_ENTRIES);
        }

        if (notFoundBranches.Count != 0)
        {
            sb.AppendLine();
            sb.AppendLine(FindConflictsCommandConstant.NOT_FOUND_BRANCHES);
            sb.AppendJoin(" ", notFoundBranches).AppendLine();
        }

        string output = sb.ToString();
        Console.Write(output);
        return output;
    }
}
