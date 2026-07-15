// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace InspectCodeBaselineTemplate;

// Multiple rules in one file:
// Rule 4: FieldCanBeMadeReadOnly.Local — _repository only assigned in constructor
// Rule 5: MemberCanBeMadeStatic.Local — Sanitize() does not use instance members
// Rule 6: ConvertIfStatementToConditionalTernaryExpression — if/else can be ?:
public class ServiceC
{
    private string _repository;    // Rule 4

    public ServiceC(string repository)
    {
        _repository = repository;
    }

    public string GetLabel(bool active)
    {
        string label;                // Rule 6
        if (active)
            label = "active";
        else
            label = "inactive";
        return $"{_repository}/{label}";
    }

    private string Sanitize(string input)    // Rule 5
    {
        return input.Trim().ToLowerInvariant();
    }
}
