// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers.FindActivity;

internal static class FindActivityOutputHelper
{
    internal static string Print(
        Dictionary<string, List<string>> activity,
        IEnumerable<string> inactiveBranches,
        DateTime currentDate
    )
    {
        StringBuilder sb = new();

        sb.AppendLine($"{FindActivityCommandConstant.ACTIVITY} [{currentDate}]");

        if (activity.Count != 0)
        {
            foreach ((string file, List<string> activeBranches) in activity)
            {
                sb.AppendLine(file);
                sb.AppendLine($"[{string.Join(" ", activeBranches)}]");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine(FindActivityCommandConstant.NO_ENTRIES);
            sb.AppendLine();
        }

        List<string> allActiveBranches = activity.Values.SelectMany(x => x).Distinct().ToList();

        sb.AppendLine(FindActivityCommandConstant.ACTIVE_BRANCHES);
        sb.AppendLine(
            allActiveBranches.Count != 0
                ? string.Join(" ", allActiveBranches)
                : FindActivityCommandConstant.NO_ENTRIES
        );

        sb.AppendLine();
        sb.AppendLine(FindActivityCommandConstant.INACTIVE_BRANCHES);
        sb.AppendLine(
            inactiveBranches.Any()
                ? string.Join(" ", inactiveBranches)
                : FindActivityCommandConstant.NO_ENTRIES
        );

        List<string> keys = activity.Keys.ToList();

        sb.AppendLine();
        sb.AppendLine(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE);
        sb.AppendLine(
            keys.Count != 0
                ? string.Join(",", keys)
                : FindActivityCommandConstant.NO_ENTRIES
        );

        sb.AppendLine();
        sb.AppendLine(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE);
        sb.AppendLine(
            keys.Count != 0
                ? string.Join(" ", keys)
                : FindActivityCommandConstant.NO_ENTRIES
        );

        sb.AppendLine();
        sb.AppendLine(FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE);

        if (keys.Count != 0)
        {
            string result = "^biak^ var activeFiles = " +
                string.Join(
                    Environment.NewLine + "    + ",
                    keys.Select((x, i) => i == 0 ? $"\"{x}\"" : $"\",{x}\"")
                );

            result += ";";

            sb.Append(result);
        }
        else
        {
            sb.Append(FindActivityCommandConstant.NO_ENTRIES);
        }

        string output = sb.ToString();

        Console.WriteLine(output);

        return output;
    }
}
