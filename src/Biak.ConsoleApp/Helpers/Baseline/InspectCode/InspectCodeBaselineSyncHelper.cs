// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Helper for `dotnet biak inspectcode-baseline sync` command.
/// </summary>
public static class InspectCodeBaselineSyncHelper
{
    private static readonly string s_baselineMarkerRegexText =
        Regex.Escape(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER)
            .Replace("\\ ", "[\\t ]*", StringComparison.OrdinalIgnoreCase);

    private static readonly Regex s_inspectCodeBaselineEntryRegex = new(
        @"^\s*(resharper_[a-z0-9_]+)\s*=\s*[^\r\n]*" + s_baselineMarkerRegexText + @"\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex s_inspectCodeBaselineSeverityRegex = new(
        @"^(?<prefix>\s*(resharper_[a-z0-9_]+)\s*=\s*)[^\r\n#]+(?<suffix>\s*" + s_baselineMarkerRegexText + @"\s*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Gets all unique baseline rule keys from valid baseline blocks.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <returns>Unique inspectcode baseline rule keys.</returns>
    public static HashSet<string> GetBaselineRuleKeys(string content)
    {
        return BaselineSyncEditorconfigHelper.GetBaselineIdentifiers(content, TryGetBaselineRuleKey);
    }

    /// <summary>
    /// Removes resolved inspectcode baseline filters and stale file entries.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="keysToKeep">Rule keys to keep.</param>
    /// <param name="activeFilesByRuleKey">Current active files by rule key.</param>
    /// <returns>Synchronized .editorconfig content.</returns>
    public static string RemoveBaselineFilters(
        string content,
        IReadOnlySet<string> keysToKeep,
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByRuleKey)
    {
        return BaselineSyncEditorconfigHelper.RemoveBaselineFilters(
            content,
            keysToKeep,
            TryGetBaselineRuleKey,
            activeFilesByRuleKey);
    }

    /// <summary>
    /// Returns synchronized files and rule keys removed by synchronization.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="keysToKeep">Rule keys to keep.</param>
    /// <param name="activeFilesByRuleKey">Current active files by rule key.</param>
    /// <returns>Map where key is file path and value is removed rule keys.</returns>
    public static IReadOnlyDictionary<string, IReadOnlySet<string>> GetSynchronizedFiles(
        string content,
        IReadOnlySet<string> keysToKeep,
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByRuleKey)
    {
        return BaselineSyncEditorconfigHelper.GetSynchronizedFiles(
            content,
            keysToKeep,
            TryGetBaselineRuleKey,
            activeFilesByRuleKey);
    }

    /// <summary>
    /// Sets configured snapshot severity for inspectcode baseline lines.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <param name="snapshotSeverity">Snapshot severity.</param>
    /// <returns>Updated content with normalized baseline severity.</returns>
    public static string NormalizeBaselineSeverity(string content, string snapshotSeverity)
    {
        string normalizedSeverity = string.IsNullOrWhiteSpace(snapshotSeverity)
            ? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY
            : snapshotSeverity.Trim();

        string newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(new[] { newline }, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            Match match = s_inspectCodeBaselineSeverityRegex.Match(lines[i]);
            if (!match.Success)
            {
                continue;
            }

            lines[i] = $"{match.Groups["prefix"].Value}{normalizedSeverity}{match.Groups["suffix"].Value}";
        }

        return string.Join(newline, lines);
    }

    private static string? TryGetBaselineRuleKey(string line)
    {
        Match match = s_inspectCodeBaselineEntryRegex.Match(line);
        return match.Success
            ? match.Groups[1].Value
            : null;
    }
}
