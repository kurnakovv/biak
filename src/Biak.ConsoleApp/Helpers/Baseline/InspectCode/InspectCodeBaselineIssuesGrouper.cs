// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Groups <see cref="InspectCodeIssue"/> items by their mapped .editorconfig key.
/// </summary>
public static class InspectCodeBaselineIssuesGrouper
{
    /// <summary>
    /// Groups issues by mapped .editorconfig key.
    /// </summary>
    /// <param name="issues">Parsed issues from SARIF.</param>
    /// <param name="ruleIdOverrides">Optional per-rule .editorconfig key overrides.</param>
    /// <returns>
    /// Grouped issues by .editorconfig key and rule IDs that had no mapping.
    /// </returns>
    public static InspectCodeBaselineIssuesGroupResult Group(
        IReadOnlyList<InspectCodeIssue> issues,
        IReadOnlyDictionary<string, string>? ruleIdOverrides)
    {
        Dictionary<string, InspectCodeBaselineIssueGroup> groupsByKey =
            new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> unmappedRuleIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (InspectCodeIssue issue in issues)
        {
            string? mappedEditorconfigKey = FindMappedEditorconfigKey(issue.RuleId, ruleIdOverrides);
            if (mappedEditorconfigKey is null)
            {
                unmappedRuleIds.Add(issue.RuleId);
                continue;
            }

            if (!groupsByKey.TryGetValue(mappedEditorconfigKey, out InspectCodeBaselineIssueGroup? group))
            {
                group = new InspectCodeBaselineIssueGroup(issue.RuleId);
                groupsByKey[mappedEditorconfigKey] = group;
            }

            group.Files.Add(issue.FilePath.Replace(Path.DirectorySeparatorChar, '/'));
        }

        return new InspectCodeBaselineIssuesGroupResult(groupsByKey, unmappedRuleIds);
    }

    private static string? FindMappedEditorconfigKey(
        string ruleId,
        IReadOnlyDictionary<string, string>? overrides)
    {
        if (overrides is not null && overrides.TryGetValue(ruleId, out string? overrideKey))
        {
            return overrideKey;
        }

        return InspectCodeRuleMetadataHelper.Get(ruleId)?.EditorconfigConfigKey;
    }
}
