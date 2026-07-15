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

            Dictionary<string, List<string>> issuesByEditorconfigKey = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> unmappedRuleIds = new(StringComparer.OrdinalIgnoreCase);

            foreach (InspectCodeIssue issue in issues)
            {
                string? editorconfigKey = ResolveEditorconfigKey(issue.RuleId, ruleIdOverrides);

                if (editorconfigKey is null)
                {
                    unmappedRuleIds.Add(issue.RuleId);
                    continue;
                }

                string relativePath = NormalizePath(issue.FilePath, baseDirectory);

                if (!issuesByEditorconfigKey.TryGetValue(editorconfigKey, out List<string>? files))
                {
                    files = new List<string>();
                    issuesByEditorconfigKey[editorconfigKey] = files;
                }

                if (!files.Contains(relativePath, StringComparer.OrdinalIgnoreCase))
                {
                    files.Add(relativePath);
                }
            }

            foreach (string unmappedRuleId in unmappedRuleIds.OrderBy(x => x))
            {
                Console.WriteLine(
                    InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_WARNING_PREFIX +
                    unmappedRuleId +
                    InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_WARNING_SUFFIX
                );
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE);
                Console.WriteLine(
                    InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND_PREFIX +
                    unmappedRuleId +
                    InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND_SUFFIX
                );
                Console.WriteLine();
            }

            if (issuesByEditorconfigKey.Count == 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            Console.WriteLine(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE);

            StringBuilder sb = new();
            foreach ((string key, List<string> files) in issuesByEditorconfigKey.OrderBy(x => x.Key))
            {
                files.Sort(StringComparer.OrdinalIgnoreCase);
                sb.AppendLine("[{" + string.Join(",", files) + "}]");
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

    private static string? ResolveEditorconfigKey(
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
