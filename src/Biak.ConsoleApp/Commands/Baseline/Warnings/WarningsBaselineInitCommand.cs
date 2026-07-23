// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers.Baseline.Warnings;
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

        return args.Length == 2
            || (args.Length == 4 && args[2] == CommandArgumentConstant.TARGET && !string.IsNullOrWhiteSpace(args[3]));
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> RunAsync(string[]? args = null)
    {
        try
        {
            Console.WriteLine(WarningsBaselineInitCommandConstant.INIT_STARTED);

            string? buildTarget = ResolveBuildTarget(args);

            SL.Build build = await WarningsBaselineBuildHelper.BuildAndReadBuildAsync(
                WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH,
                buildTarget
            );

            string originalDirectory = Directory.GetCurrentDirectory();

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
                Console.WriteLine(WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND);
                return WarningsBaselineInitCommandConstant.NO_WARNINGS_FOUND;
            }

            Console.WriteLine(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE);
            Console.WriteLine(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_CONFIGURATION);
            Console.WriteLine();

            Console.WriteLine(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE);
            StringBuilder editorconfigSb = new();
            foreach ((string code, IReadOnlyList<string> files) in warnings)
            {
                editorconfigSb.AppendLine("[{" + $"{string.Join(",", files)}" + "}]");
                editorconfigSb.AppendLine($"dotnet_diagnostic.{code}.severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}");
                editorconfigSb.AppendLine();
            }
            string result = editorconfigSb.ToString();
            Console.WriteLine(result);
            return result;
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{WarningsBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
        finally
        {
            if (File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH))
            {
                File.Delete(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH);
            }
        }
    }

    private static string? ResolveBuildTarget(string[]? args)
    {
        return args?.Length == 4
            && args[2] == CommandArgumentConstant.TARGET
            && !string.IsNullOrWhiteSpace(args[3])
            ? args[3]
            : null;
    }
}
