// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Helpers.Baseline;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak inspectcode-baseline sync` command.
/// </summary>
public static class InspectCodeBaselineSyncCommand
{
    /// <summary>
    /// Can `dotnet biak inspectcode-baseline sync` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        if (args.Length < 2)
        {
            return false;
        }

        bool isCommand = args[0] == CommandArgumentConstant.INSPECTCODE_BASELINE
            && args[1] == CommandArgumentConstant.SYNC;

        if (!isCommand)
        {
            return false;
        }

        if (args.Length == 2)
        {
            return true;
        }

        return CommandArgumentHelper.TryParseOptions(
            args,
            out _,
            new HashSet<string>(StringComparer.Ordinal)
            {
                CommandArgumentConstant.PATH,
            });
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> RunAsync(string[]? args = null)
    {
        string sarifPath = string.Empty;
        string resolvedPath = string.Empty;
        string originalContent = string.Empty;
        string runtimeEditorconfigPath = string.Empty;
        string runtimeEditorconfigOriginalContent = string.Empty;
        bool runtimeEditorconfigWasTemporarilyModified = false;
        bool completedSuccessfully = false;

        try
        {
            Console.WriteLine(InspectCodeBaselineSyncCommandConstant.SYNC_STARTED);
            Console.WriteLine();

            string[] effectiveArgs = args
                ?? new[]
                {
                    CommandArgumentConstant.INSPECTCODE_BASELINE,
                    CommandArgumentConstant.SYNC,
                };

            (_, BiakConfig config) = await BiakConfigHelper.GetAsync();
            InspectCodeBaselineConfig? baselineConfig = config.InspectCodeBaseline;

            string baseDirectory = Directory.GetCurrentDirectory();
            runtimeEditorconfigPath = Path.Join(baseDirectory, ".editorconfig");
            if (!File.Exists(runtimeEditorconfigPath))
            {
                throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.ROOT_EDITORCONFIG_FILE_NOT_FOUND);
            }

            bool hasBiakDirectory = Directory.Exists(Path.Join(baseDirectory, ".biak"));

            if (hasBiakDirectory)
            {
                BiakStatusResult biakStatus = await BiakStatusHelper.GetAsync();
                if (biakStatus.StatusType is not (BiakStatusType.Enabled or BiakStatusType.Disabled))
                {
                    throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.BIAK_STATUS_IS_NOT_SYNCHRONIZED);
                }
            }

            string baselinePath = ResolveBaselinePath(effectiveArgs, baselineConfig, baseDirectory);

            if (!BaselinePathHelper.IsSafe(baselinePath, baseDirectory))
            {
                throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG);
            }

            resolvedPath = Path.GetFullPath(baselinePath, baseDirectory);
            if (!File.Exists(resolvedPath))
            {
                throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.FILE_NOT_FOUND);
            }

            originalContent = await File.ReadAllTextAsync(resolvedPath);

            if (!originalContent.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, StringComparison.Ordinal))
            {
                throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER);
            }

            runtimeEditorconfigOriginalContent = await File.ReadAllTextAsync(runtimeEditorconfigPath);
            string runtimeContentForAnalysis = InspectCodeBaselineSyncHelper.PrepareRuntimeContentForAnalysis(
                runtimeEditorconfigOriginalContent
            );

            if (!string.Equals(runtimeContentForAnalysis, runtimeEditorconfigOriginalContent, StringComparison.Ordinal))
            {
                await File.WriteAllTextAsync(runtimeEditorconfigPath, runtimeContentForAnalysis);
                runtimeEditorconfigWasTemporarilyModified = true;
            }

            sarifPath = await InspectCodeBaselineRunHelper.RunAsync(
                baselineConfig?.Target,
                baselineConfig?.AdditionalArgs);

            string sarifJson = await File.ReadAllTextAsync(sarifPath);
            IReadOnlyList<InspectCodeIssue> issues = InspectCodeBaselineSarifParser.Parse(sarifJson);

            if (runtimeEditorconfigWasTemporarilyModified)
            {
                await File.WriteAllTextAsync(runtimeEditorconfigPath, runtimeEditorconfigOriginalContent);
                runtimeEditorconfigWasTemporarilyModified = false;
            }

            IReadOnlyDictionary<string, string>? ruleIdOverrides = baselineConfig?.RuleIdOverrides;

            InspectCodeBaselineIssuesGroupResult groupResult = InspectCodeBaselineIssuesGrouper.Group(issues, ruleIdOverrides);

            IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByRuleKey = groupResult.GroupsByKey
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlySet<string>)x.Value.Files,
                    StringComparer.OrdinalIgnoreCase);

            HashSet<string> baselineRuleKeys = InspectCodeBaselineSyncHelper.GetRuleKeys(originalContent);
            HashSet<string> activeRuleKeys = activeFilesByRuleKey.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            HashSet<string> keysToKeep = baselineRuleKeys
                .Where(activeRuleKeys.Contains)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            IReadOnlyDictionary<string, IReadOnlySet<string>> synchronizedFiles = InspectCodeBaselineSyncHelper.GetSynchronizedFiles(
                originalContent,
                keysToKeep,
                activeFilesByRuleKey);

            string syncedContent = InspectCodeBaselineSyncHelper.RemoveFilters(
                originalContent,
                keysToKeep,
                activeFilesByRuleKey);

            string snapshotSeverity = baselineConfig?.SnapshotSeverity
                ?? InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY;

            syncedContent = InspectCodeBaselineSyncHelper.NormalizeSeverity(syncedContent, snapshotSeverity);
            await File.WriteAllTextAsync(resolvedPath, syncedContent);
            completedSuccessfully = true;

            HashSet<string> remainingBaselineRuleKeys = InspectCodeBaselineSyncHelper.GetRuleKeys(syncedContent);

            string result;
            if (remainingBaselineRuleKeys.Count == 0)
            {
                result = InspectCodeBaselineSyncCommandConstant.ALL_ISSUES_FIXED;
            }
            else
            {
                int removedCount = baselineRuleKeys.Count - remainingBaselineRuleKeys.Count;
                result = $"Sync complete. Removed {synchronizedFiles.Count} file(s); resolved {removedCount} filter(s). {remainingBaselineRuleKeys.Count} filter(s) still alive.";

                foreach (KeyValuePair<string, IReadOnlySet<string>> synchronizedFile in synchronizedFiles.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                {
                    string keys = string.Join(", ", synchronizedFile.Value.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
                    Console.WriteLine($"{synchronizedFile.Key} ({keys})");
                }

                if (synchronizedFiles.Count > 0)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine(result);
            Console.WriteLine();

            return result;
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{InspectCodeBaselineSyncCommandConstant.SYNC_FAILED} {ex.Message}");
        }
        finally
        {
            if (runtimeEditorconfigWasTemporarilyModified
                && !string.IsNullOrWhiteSpace(runtimeEditorconfigPath)
                && File.Exists(runtimeEditorconfigPath))
            {
                await File.WriteAllTextAsync(runtimeEditorconfigPath, runtimeEditorconfigOriginalContent);
            }

            if (!completedSuccessfully
                && !string.IsNullOrWhiteSpace(resolvedPath)
                && File.Exists(resolvedPath)
                && !string.IsNullOrEmpty(originalContent))
            {
                await File.WriteAllTextAsync(resolvedPath, originalContent);
            }

            if (!string.IsNullOrWhiteSpace(sarifPath) && File.Exists(sarifPath))
            {
                File.Delete(sarifPath);
            }
        }
    }

    private static string ResolveBaselinePath(string[] args, InspectCodeBaselineConfig? baselineConfig, string baseDirectory)
    {
        if (CommandArgumentHelper.TryParseOptions(
            args,
            out Dictionary<string, string> options,
            new HashSet<string>(StringComparer.Ordinal)
            {
                CommandArgumentConstant.PATH,
            })
            && options.TryGetValue(CommandArgumentConstant.PATH, out string? pathFromCli))
        {
            return pathFromCli;
        }

        if (!string.IsNullOrWhiteSpace(baselineConfig?.Path))
        {
            return baselineConfig.Path;
        }

        string biakDirectory = Path.GetFullPath(".biak", baseDirectory);

        if (Directory.Exists(biakDirectory))
        {
            string[] discovered = Directory.GetFiles(biakDirectory, ".editorconfig*")
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string candidate in discovered)
            {
                string content = File.ReadAllText(candidate);
                if (content.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, StringComparison.Ordinal))
                {
                    return Path.GetRelativePath(baseDirectory, candidate);
                }
            }
        }

        string rootEditorconfigPath = Path.GetFullPath(".editorconfig", baseDirectory);
        if (File.Exists(rootEditorconfigPath))
        {
            string content = File.ReadAllText(rootEditorconfigPath);
            if (content.Contains(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, StringComparison.Ordinal))
            {
                return ".editorconfig";
            }
        }

        throw new BiakApplicationException(InspectCodeBaselineSyncCommandConstant.NO_BASELINE_MARKER);
    }
}
