// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak inspectcode-baseline init` command.
/// </summary>
public static class InspectCodeBaselineInitCommand
{
    /// <summary>
    /// Can `dotnet biak inspectcode-baseline init` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        if (args.Length < 2)
        {
            return false;
        }

        return args[0] == CommandArgumentConstant.INSPECTCODE_BASELINE
            && args[1] == CommandArgumentConstant.INIT;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Generated editorconfig baseline content printed to console.</returns>
    public static async Task<string> RunAsync(string[]? args = null)
    {
        string sarifPath = string.Empty;

        try
        {
            Console.WriteLine(InspectCodeBaselineInitCommandConstant.INIT_STARTED);

            _ = args;

            (_, BiakConfig config) = await BiakConfigHelper.GetAsync();
            InspectCodeBaselineConfig? baselineConfig = config.InspectCodeBaseline;

            sarifPath = await InspectCodeBaselineRunHelper.RunAsync(
                baselineConfig?.Target,
                baselineConfig?.AdditionalArgs);

            string sarifJson = await File.ReadAllTextAsync(sarifPath);
            IReadOnlyList<InspectCodeIssue> issues = InspectCodeBaselineSarifParser.Parse(sarifJson);

            if (issues.Count == 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            string baseDirectory = Directory.GetCurrentDirectory();
            string snapshotSeverity = baselineConfig?.SnapshotSeverity
                ?? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY;

            IReadOnlyDictionary<string, string>? ruleIdOverrides = baselineConfig?.RuleIdOverrides;

            Dictionary<string, List<string>> issuesByMappedEditorconfigKey = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<string>> ruleIdsByMappedEditorconfigKey = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> unmappedRuleIds = new(StringComparer.OrdinalIgnoreCase);

            foreach (InspectCodeIssue issue in issues)
            {
                string? mappedEditorconfigKey = FindMappedEditorconfigKey(issue.RuleId, ruleIdOverrides);
                if (mappedEditorconfigKey is null)
                {
                    unmappedRuleIds.Add(issue.RuleId);
                    continue;
                }

                string relativePath = NormalizePath(issue.FilePath, baseDirectory);

                if (!issuesByMappedEditorconfigKey.TryGetValue(mappedEditorconfigKey, out List<string>? files))
                {
                    files = new List<string>();
                    issuesByMappedEditorconfigKey[mappedEditorconfigKey] = files;
                }

                if (!ruleIdsByMappedEditorconfigKey.TryGetValue(mappedEditorconfigKey, out HashSet<string>? ruleIds))
                {
                    ruleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    ruleIdsByMappedEditorconfigKey[mappedEditorconfigKey] = ruleIds;
                }

                ruleIds.Add(issue.RuleId);

                if (!files.Contains(relativePath, StringComparer.OrdinalIgnoreCase))
                {
                    files.Add(relativePath);
                }
            }

            if (unmappedRuleIds.Count > 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER);
                Console.WriteLine(string.Join(", ", unmappedRuleIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));

                Console.WriteLine();
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE);
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND);
                Console.WriteLine();
            }

            if (issuesByMappedEditorconfigKey.Count == 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            Console.WriteLine(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE);

            StringBuilder sb = new();
            foreach ((string key, List<string> files) in issuesByMappedEditorconfigKey.OrderBy(x => x.Key))
            {
                files.Sort(StringComparer.OrdinalIgnoreCase);

                string ruleId = ruleIdsByMappedEditorconfigKey[key]
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .First();

                InspectCodeRuleMetadata? metadata = InspectCodeRuleMetadataHelper.Get(ruleId);

                sb.AppendLine("[{" + string.Join(",", files) + "}]");

                if (metadata is not null)
                {
                    sb.AppendLine($"# {metadata.Title} [{ruleId}] | {metadata.Reference}");
                }

                sb.AppendLine($"{key} = {snapshotSeverity} {InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}");
                sb.AppendLine();
            }

            string result = sb.ToString();
            Console.WriteLine(result);
            return result;
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{InspectCodeBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(sarifPath) && File.Exists(sarifPath))
            {
                File.Delete(sarifPath);
            }
        }
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

    private static string NormalizePath(string filePath, string baseDirectory)
    {
        // SARIF URIs may be absolute (file:///...) or relative paths — normalize both.
        string path = filePath;

        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            path = new Uri(path).LocalPath;
        }

        if (Path.IsPathRooted(path))
        {
            path = Path.GetRelativePath(baseDirectory, path);
        }

        return path.Replace(Path.DirectorySeparatorChar, '/');
    }
}
