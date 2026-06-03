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
    public static async Task RunAsync()
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "dotnet",
                Arguments = "build --no-incremental -bl:build.binlog",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            using Process process = Process.Start(psi)
                ?? throw new BiakApplicationException(WarningsBaselineInitCommandConstant.FAILED_TO_START_DOTNET_BUILD);

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string details = string.IsNullOrWhiteSpace(error) ? output : error;
                throw new BiakApplicationException($"{WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED} {details}".Trim());
            }

            if (!File.Exists("build.binlog"))
            {
                throw new BiakApplicationException(WarningsBaselineInitCommandConstant.BUILD_BINLOG_NOT_FOUND);
            }

            SL.Build build = SL.BinaryLog.ReadBuild("build.binlog");
            string originalDirectory = Directory.GetCurrentDirectory();

            Console.WriteLine(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE);
            Console.WriteLine("<TreatWarningsAsErrors>true</TreatWarningsAsErrors>");

            Dictionary<string, IReadOnlyList<string>> warnings = build.FindChildrenRecursive<SL.Warning>()
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

            Console.WriteLine();
            Console.WriteLine(".editorconfig");
            foreach ((string code, IReadOnlyList<string> files) in warnings)
            {
                Console.WriteLine("[{" + $"{string.Join(",", files)}" + "}]");
                Console.WriteLine($"dotnet_diagnostic.{code}.severity = suggestion # ^biak^ baseline");
                Console.WriteLine();
            }
        }
        catch (Exception ex) when (ex is not BiakApplicationException)
        {
            throw new BiakApplicationException($"{WarningsBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
    }
}
