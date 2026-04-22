// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers.FindActivity;

internal static class FindActivityOutputHelper
{
    internal static void Print(Dictionary<string, List<string>> activity, IEnumerable<string> inactiveBranches)
    {
        Console.WriteLine($"{FindActivityCommandConstant.ACTIVITY} [{DateTime.UtcNow}]");
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
        Console.WriteLine(inactiveBranches.Any() ? string.Join(" ", inactiveBranches) : FindActivityCommandConstant.NO_ENTRIES);

        List<string> keys = activity.Keys.ToList();

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE);
        Console.WriteLine(keys.Count != 0 ? string.Join(",", keys) : FindActivityCommandConstant.NO_ENTRIES);

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE);
        Console.WriteLine(keys.Count != 0 ? string.Join(" ", keys) : FindActivityCommandConstant.NO_ENTRIES);

        Console.WriteLine();
        Console.WriteLine(FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE);

        if (keys.Count != 0)
        {
            string result = "^biak^ var activeFiles = " +
                string.Join(
                    Environment.NewLine + "    + ",
                    keys.Select((x, i) => i == 0 ? $"\"{x}\"" : $"\",{x}\"")
                );

            result += ";";

            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine(FindActivityCommandConstant.NO_ENTRIES);
        }
    }
}
