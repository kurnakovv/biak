// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using SL = Microsoft.Build.Logging.StructuredLogger;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak warnings-baseline init` command.
/// </summary>
public static class WarningsBaselineInitCommand
{
    private static readonly HashSet<string> s_sourceFileExtensions = new() { ".cs", ".vb", ".fs" };

    /// <summary>
    /// Can `dotnet biak warnings-baseline init` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args.Length == 2 && args[0] == CommandArgumentConstant.WARNINGS_BASELINE && args[1] == CommandArgumentConstant.INIT;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<string> RunAsync()
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "dotnet",
                Arguments = $"build --no-incremental -bl:{WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH}",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
            };

            using Process process = Process.Start(psi)
                ?? throw new BiakApplicationException(WarningsBaselineInitCommandConstant.FAILED_TO_START_DOTNET_BUILD);

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new BiakApplicationException(WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED);
            }

            if (!File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH))
            {
                throw new BiakApplicationException(WarningsBaselineInitCommandConstant.BUILD_BINLOG_NOT_FOUND);
            }

            SL.Build build = SL.BinaryLog.ReadBuild(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH);

            if (build.FindChildrenRecursive<SL.Error>().Any())
            {
                throw new BiakApplicationException(WarningsBaselineInitCommandConstant.BUILD_CONTAINS_ERRORS);
            }

            string originalDirectory = Directory.GetCurrentDirectory();

            Dictionary<string, IReadOnlyList<string>> warnings = build.FindChildrenRecursive<SL.Warning>()
                .Where(x => s_sourceFileExtensions.Contains(Path.GetExtension(x.File)))
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
                return string.Empty;
            }

            Console.WriteLine(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE);
            Console.WriteLine(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_CONFIGURATION);
            Console.WriteLine();

            Console.WriteLine(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE);
            System.Text.StringBuilder editorconfigSb = new();
            foreach ((string code, IReadOnlyList<string> files) in warnings)
            {
                editorconfigSb.AppendLine("[{" + $"{string.Join(",", files)}" + "}]");
                editorconfigSb.AppendLine($"dotnet_diagnostic.{code}.severity = suggestion # ^biak^ baseline");
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
}
