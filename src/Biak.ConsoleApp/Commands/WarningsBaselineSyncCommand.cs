// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using SL = Microsoft.Build.Logging.StructuredLogger;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak warnings-baseline sync` command.
/// </summary>
public static class WarningsBaselineSyncCommand
{
    /// <summary>
    /// Can `dotnet biak warnings-baseline sync` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        if (args.Length < 2)
        {
            return false;
        }

        bool isCommand = args[0] == CommandArgumentConstant.WARNINGS_BASELINE
            && args[1] == CommandArgumentConstant.SYNC;

        if (!isCommand)
        {
            return false;
        }

        return args.Length == 2
            || (args.Length == 4 && args[2] == CommandArgumentConstant.PATH && !string.IsNullOrWhiteSpace(args[3]));
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> RunAsync(string[] args)
    {
        string baseDirectory = Directory.GetCurrentDirectory();
        string editorConfigPath = ResolveEditorConfigPath(args, baseDirectory);
        string resolvedPath = Path.GetFullPath(editorConfigPath, baseDirectory);
        string originalContent = string.Empty;
        bool baselineWasActivated = false;
        bool completedSuccessfully = false;

        try
        {
            Console.WriteLine(WarningsBaselineSyncCommandConstant.SYNC_STARTED);
            Console.WriteLine();

            if (!WarningsBaselineSyncHelper.IsPathSafe(editorConfigPath, baseDirectory))
            {
                throw new BiakApplicationException(WarningsBaselineSyncCommandConstant.INVALID_PATH_EDITORCONFIG);
            }

            if (!File.Exists(resolvedPath))
            {
                throw new BiakApplicationException(WarningsBaselineSyncCommandConstant.FILE_NOT_FOUND);
            }

            originalContent = await File.ReadAllTextAsync(resolvedPath);

            if (!originalContent.Contains(WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER, StringComparison.Ordinal))
            {
                throw new BiakApplicationException(WarningsBaselineSyncCommandConstant.NO_BASELINE_MARKER);
            }

            // Activate baseline entries as warnings so the compiler emits them during the build.
            string activatedContent = WarningsBaselineSyncHelper.SetBaselineForBuild(originalContent, activate: true);
            await File.WriteAllTextAsync(resolvedPath, activatedContent);
            baselineWasActivated = true;

            SL.Build build = await WarningsBaselineBuildHelper.BuildAndReadBuildAsync(
                WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH
            );

            List<SL.Warning> sourceWarnings = WarningsBaselineBuildHelper.GetSourceWarnings(build).ToList();

            HashSet<string> activeWarningCodes = sourceWarnings
                .Select(x => x.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByCode = sourceWarnings
                .GroupBy(x => x.Code)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlySet<string>)x
                        .Select(warning => Path.GetRelativePath(baseDirectory, warning.File).Replace(Path.DirectorySeparatorChar, '/'))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase
                );

            HashSet<string> baselineCodes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(originalContent);

            // Keep only filters whose code is still an active warning.
            HashSet<string> codesToKeep = baselineCodes
                .Where(c => activeWarningCodes.Contains(c))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            IReadOnlyDictionary<string, IReadOnlySet<string>> synchronizedFiles = WarningsBaselineSyncHelper.GetSynchronizedFiles(
                originalContent,
                codesToKeep,
                activeFilesByCode
            );

            string syncedContent = WarningsBaselineSyncHelper.RemoveBaselineFilters(originalContent, codesToKeep, activeFilesByCode);
            syncedContent = WarningsBaselineSyncHelper.SetBaselineForBuild(syncedContent, activate: false);

            await File.WriteAllTextAsync(resolvedPath, syncedContent);
            completedSuccessfully = true;

            HashSet<string> remainingBaselineCodes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(syncedContent);

            string result;
            if (remainingBaselineCodes.Count == 0)
            {
                result = WarningsBaselineSyncCommandConstant.ALL_WARNINGS_FIXED;
            }
            else
            {
                int removedCount = baselineCodes.Count - remainingBaselineCodes.Count;
                result = $"Sync complete. Removed {synchronizedFiles.Count} file(s); resolved {removedCount} filter(s). {remainingBaselineCodes.Count} filter(s) still alive.";

                foreach (KeyValuePair<string, IReadOnlySet<string>> synchronizedFile in synchronizedFiles.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                {
                    string codes = string.Join(", ", synchronizedFile.Value.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
                    Console.WriteLine($"{synchronizedFile.Key} ({codes})");
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
            throw new BiakApplicationException($"{WarningsBaselineSyncCommandConstant.SYNC_FAILED} {ex.Message}");
        }
        finally
        {
            if (baselineWasActivated && !completedSuccessfully)
            {
                await File.WriteAllTextAsync(resolvedPath, originalContent);
            }

            if (File.Exists(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH))
            {
                File.Delete(WarningsBaselineSyncCommandConstant.BUILD_BINLOG_PATH);
            }
        }
    }

    private static string ResolveEditorConfigPath(string[] args, string baseDirectory)
    {
        if (args.Length == 4)
        {
            return args[3];
        }

        string mainEditorconfigPath = WarningsBaselineSyncCommandConstant.DEFAULT_EDITORCONFIG_MAIN_PATH;
        string rootEditorconfigPath = WarningsBaselineSyncCommandConstant.DEFAULT_EDITORCONFIG_PATH;

        if (File.Exists(Path.GetFullPath(mainEditorconfigPath, baseDirectory)))
        {
            return mainEditorconfigPath;
        }

        if (File.Exists(Path.GetFullPath(rootEditorconfigPath, baseDirectory)))
        {
            return rootEditorconfigPath;
        }

        throw new BiakApplicationException(WarningsBaselineSyncCommandConstant.DEFAULT_CONFIGURATION_FILE_NOT_FOUND);
    }
}
