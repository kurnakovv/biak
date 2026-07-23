// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace InspectCodeBaselineTemplate;

// Multiple rules in one file:
// Rule 9:  ReplaceWithSingleCallToFirstOrDefault — .Where(...).FirstOrDefault() can be simplified
// Rule 10: ConvertToAutoProperty (x2, repeated) — trivial backing field properties
public class ServiceE
{
    private string _name;        // Rule 10
    private int _score;          // Rule 10

    public ServiceE(string name, int score)
    {
        _name = name;
        _score = score;
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }

    public string? FindByPrefix(List<string> items, string prefix)
    {
        return items.Where(x => x.StartsWith(prefix)).FirstOrDefault();    // Rule 9
    }
}
