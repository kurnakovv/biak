// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Result of grouping InspectCode issues by .editorconfig key.
/// </summary>
public sealed class InspectCodeBaselineIssuesGroupResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InspectCodeBaselineIssuesGroupResult"/> class.
    /// </summary>
    /// <param name="groupsByKey">Issue groups keyed by .editorconfig key.</param>
    /// <param name="unmappedRuleIds">Rule IDs that had no .editorconfig key mapping.</param>
    public InspectCodeBaselineIssuesGroupResult(
        Dictionary<string, InspectCodeBaselineIssueGroup> groupsByKey,
        HashSet<string> unmappedRuleIds)
    {
        GroupsByKey = groupsByKey;
        UnmappedRuleIds = unmappedRuleIds;
    }

    /// <summary>
    /// Gets the issue groups keyed by .editorconfig key.
    /// </summary>
    public Dictionary<string, InspectCodeBaselineIssueGroup> GroupsByKey { get; }

    /// <summary>
    /// Gets the rule IDs that had no .editorconfig key mapping.
    /// </summary>
    public HashSet<string> UnmappedRuleIds { get; }
}
