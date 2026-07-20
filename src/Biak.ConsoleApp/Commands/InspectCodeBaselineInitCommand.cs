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

            string snapshotSeverity = baselineConfig?.SnapshotSeverity
                ?? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY;

            IReadOnlyDictionary<string, string>? ruleIdOverrides = baselineConfig?.RuleIdOverrides;

            InspectCodeBaselineIssuesGroupResult groupResult = InspectCodeBaselineIssuesGrouper.Group(issues, ruleIdOverrides);

            if (groupResult.UnmappedRuleIds.Count > 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER);
                Console.WriteLine(string.Join(", ", groupResult.UnmappedRuleIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));

                Console.WriteLine();
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE);
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND);
                Console.WriteLine();
            }

            if (groupResult.GroupsByKey.Count == 0)
            {
                Console.WriteLine(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            Console.WriteLine(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE);

            StringBuilder sb = new();
            foreach ((string key, InspectCodeBaselineIssueGroup group) in groupResult.GroupsByKey.OrderBy(x => x.Key))
            {
                IOrderedEnumerable<string> sortedFiles = group.Files.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

                InspectCodeRuleMetadata? metadata = InspectCodeRuleMetadataHelper.Get(group.RuleId);

                if (metadata is not null)
                {
                    sb.AppendLine($"# {metadata.Title} [{group.RuleId}] | {metadata.Reference}");
                }

                sb.AppendLine("[{" + string.Join(",", sortedFiles) + "}]");
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
}
