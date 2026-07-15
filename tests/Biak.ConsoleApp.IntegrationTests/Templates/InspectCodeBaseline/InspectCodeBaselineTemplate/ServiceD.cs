// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace InspectCodeBaselineTemplate;

// Multiple rules in one file:
// Rule 7: ReplaceWithStringIsNullOrEmpty (x1)
// Rule 8: UseArrayEmptyMethod (x2, repeated)
public class ServiceD
{
    public bool IsBlank(string? value)
    {
        return value == null || value.Length == 0;    // Rule 7
    }

    public string[] GetDefaultTags()
    {
        return new string[0];    // Rule 8
    }

    public int[] GetDefaultIds()
    {
        return new int[0];       // Rule 8
    }
}
