// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace InspectCodeBaselineTemplate;

// Repeated violations of Rule 2: MemberCanBePrivate.Global (x2)
// Repeated violations of Rule 3: InvertCondition.1 (x2)
public class ServiceB
{
    public void Run(List<string>? items, List<int>? ids)
    {
        if (!(items == null))        // Rule 3
        {
            PrintItems(items);
        }

        if (!(ids == null))          // Rule 3
        {
            PrintIds(ids);
        }
    }

    public void PrintItems(List<string> items)    // Rule 2
    {
        foreach (string item in items)
            Console.WriteLine(item);
    }

    public void PrintIds(List<int> ids)           // Rule 2
    {
        foreach (int id in ids)
            Console.WriteLine(id);
    }
}
