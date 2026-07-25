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
        return args.Length == 2
            && args[0] == CommandArgumentConstant.INSPECTCODE_BASELINE
            && args[1] == CommandArgumentConstant.INIT;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <returns>Generated editorconfig baseline content printed to console.</returns>
    public static async Task<string> RunAsync(AppExecutionContext? executionContext = null)
    {
        AppExecutionContext context = executionContext ?? AppExecutionContext.CreateDefault();
        string sarifPath = string.Empty;

        try
        {
            await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.INIT_STARTED);

            (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync(executionContext: context);
            if (message is not null)
            {
                await context.Out.WriteLineAsync(message);
                await context.Out.WriteLineAsync();
            }
            InspectCodeBaselineConfig? baselineConfig = config.InspectCodeBaseline;

            sarifPath = await InspectCodeBaselineRunHelper.RunAsync(
                baselineConfig?.Target,
                baselineConfig?.AdditionalArgs,
                context);

            string sarifJson = await File.ReadAllTextAsync(sarifPath);
            IReadOnlyList<InspectCodeIssue> issues = InspectCodeBaselineSarifParser.Parse(sarifJson);

            if (issues.Count == 0)
            {
                await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            string snapshotSeverity = baselineConfig?.SnapshotSeverity
                ?? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY;

            IReadOnlyDictionary<string, string>? ruleIdOverrides = baselineConfig?.RuleIdOverrides;

            InspectCodeBaselineIssuesGroupResult groupResult = InspectCodeBaselineIssuesGrouper.Group(issues, ruleIdOverrides);

            if (groupResult.UnmappedRuleIds.Count > 0)
            {
                await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULES_NOT_MAPPED_WARNING_HEADER);
                await context.Out.WriteLineAsync(string.Join(", ", groupResult.UnmappedRuleIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));

                await context.Out.WriteLineAsync();
                await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_OPEN_ISSUE);
                await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.RULE_NOT_MAPPED_LOCAL_WORKAROUND);
                await context.Out.WriteLineAsync();
            }

            if (groupResult.GroupsByKey.Count == 0)
            {
                await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND);
                return InspectCodeBaselineInitCommandConstant.NO_ISSUES_FOUND;
            }

            await context.Out.WriteLineAsync(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE);

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
            await context.Out.WriteLineAsync(result);
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
