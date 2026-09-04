// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers.Baseline.Warnings;
using Biak.ConsoleApp.Models;
using SL = Microsoft.Build.Logging.StructuredLogger;

namespace Biak.ConsoleApp.Commands.Baseline.Warnings;

/// <summary>
/// `dotnet biak warnings-baseline init` command.
/// </summary>
public static class WarningsBaselineInitCommand
{
    /// <summary>
    /// Can `dotnet biak warnings-baseline init` command be run.
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
            && args[1] == CommandArgumentConstant.INIT;

        if (!isCommand)
        {
            return false;
        }

        return args is [_, _]
            || (args is [_, _, CommandArgumentConstant.TARGET, var target] && !string.IsNullOrWhiteSpace(target));
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> RunAsync(AppExecutionContext executionContext, string[]? args = null)
    {
        try
        {
            await executionContext.Out.WriteLineAsync(WarningsBaselineInitCommandConstant.INIT_STARTED);

            string? buildTarget = ResolveBuildTarget(args);
            string baseDirectory = executionContext.WorkingDirectory;
            string buildBinlogPath = Path.GetFullPath(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH, baseDirectory);

            SL.Build build = await WarningsBaselineBuildHelper.BuildAndReadBuildAsync(
                buildBinlogPath,
                executionContext,
                buildTarget
            );

            string originalDirectory = baseDirectory;

            Dictionary<string, IReadOnlyList<string>> warnings = WarningsBaselineBuildHelper.GetSourceWarnings(build)
                .GroupBy(x => x.Code)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    y => (IReadOnlyList<string>)y
                        .Select(x => Path.GetRelativePath(originalDirectory, x.File).Replace(Path.DirectorySeparatorChar, '/'))
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList()
                );

            if (warnings.Count == 0)
            {
                await executionContext.Out.WriteLineAsync(WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND);
                return WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND;
            }

            await executionContext.Out.WriteLineAsync(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE);
            await executionContext.Out.WriteLineAsync(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_CONFIGURATION);
            await executionContext.Out.WriteLineAsync();

            await executionContext.Out.WriteLineAsync(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE);
            StringBuilder editorconfigSb = new();
            foreach ((string code, IReadOnlyList<string> files) in warnings)
            {
                editorconfigSb.AppendLine("[{" + $"{string.Join(",", files)}" + "}]");
                editorconfigSb.AppendLine($"dotnet_diagnostic.{code}.severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}");
                editorconfigSb.AppendLine();
            }
            string result = editorconfigSb.ToString();
            await executionContext.Out.WriteLineAsync(result);
            return result;
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{WarningsBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
        finally
        {
            string buildBinlogPath = Path.GetFullPath(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH, executionContext.WorkingDirectory);

            if (File.Exists(buildBinlogPath))
            {
                File.Delete(buildBinlogPath);
            }
        }
    }

    private static string? ResolveBuildTarget(string[]? args)
    {
        return args is [_, _, CommandArgumentConstant.TARGET, var target] && !string.IsNullOrWhiteSpace(target)
            ? target
            : null;
    }
}
