// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands.Baseline.InspectCode;

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
        return args is [CommandArgumentConstant.INSPECTCODE_BASELINE, CommandArgumentConstant.INIT];
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <returns>Generated editorconfig baseline content printed to console.</returns>
    public static async Task<string> RunAsync(AppExecutionContext executionContext)
    {
        string sarifPath = string.Empty;
        bool isDebugModeEnabled = false;

        try
        {
            await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.INIT_STARTED);

            (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync(executionContext: executionContext);
            if (message is not null)
            {
                await executionContext.Out.WriteLineAsync(message);
                await executionContext.Out.WriteLineAsync();
            }
            InspectCodeBaselineConfig? baselineConfig = config.InspectCodeBaseline;
            isDebugModeEnabled = baselineConfig?.DebugMode == true;

            sarifPath = await InspectCodeBaselineRunHelper.RunAsync(
                executionContext,
                baselineConfig?.Target,
                baselineConfig?.AdditionalArgs,
                isDebugModeEnabled
            );

            string sarifJson = await File.ReadAllTextAsync(sarifPath);
            IReadOnlyList<InspectCodeIssue> issues = InspectCodeBaselineSarifParser.Parse(sarifJson);

            if (issues.Count == 0)
            {
                await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            string snapshotSeverity = baselineConfig?.SnapshotSeverity
                ?? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY;

            IReadOnlyDictionary<string, string>? ruleIdOverrides = baselineConfig?.RuleIdOverrides;

            InspectCodeBaselineIssuesGroupResult groupResult = InspectCodeBaselineIssuesGrouper.Group(issues, ruleIdOverrides);

            if (groupResult.UnmappedRuleIds.Count > 0)
            {
                await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER);
                await executionContext.Out.WriteLineAsync(string.Join(", ", groupResult.UnmappedRuleIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));

                await executionContext.Out.WriteLineAsync();
                await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE);
                await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND);
                await executionContext.Out.WriteLineAsync();
            }

            if (groupResult.GroupsByKey.Count == 0)
            {
                await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            await executionContext.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE);

            StringBuilder sb = new();
            foreach ((string key, InspectCodeBaselineIssueGroup group) in groupResult.GroupsByKey.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
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
            await executionContext.Out.WriteLineAsync(result);
            return result;
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{InspectCodeBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
        finally
        {
            if (!isDebugModeEnabled
                && !string.IsNullOrWhiteSpace(sarifPath)
                && File.Exists(sarifPath))
            {
                File.Delete(sarifPath);
            }
        }
    }
}
