// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// A group of issues sharing the same .editorconfig key.
/// </summary>
public sealed class InspectCodeBaselineIssueGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InspectCodeBaselineIssueGroup"/> class.
    /// </summary>
    /// <param name="ruleId">InspectCode rule identifier.</param>
    public InspectCodeBaselineIssueGroup(string ruleId)
    {
        RuleId = ruleId;
    }

    /// <summary>
    /// Gets the InspectCode rule identifier for this group.
    /// </summary>
    public string RuleId { get; }

    /// <summary>
    /// Gets the set of relative file paths that have this issue.
    /// </summary>
    public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
}
