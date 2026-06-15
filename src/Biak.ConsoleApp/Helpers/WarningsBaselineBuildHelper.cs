// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using SL = Microsoft.Build.Logging.StructuredLogger;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Helper for building the current solution and reading the generated binlog.
/// </summary>
public static class WarningsBaselineBuildHelper
{
    private static readonly HashSet<string> s_sourceFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".cs", ".vb" };

    /// <summary>
    /// Runs <c>dotnet build</c>, validates the generated binlog and returns parsed build data.
    /// </summary>
    /// <param name="buildBinlogPath">Binlog path to write and read.</param>
    /// <returns>Parsed MSBuild structured log build object.</returns>
    public static async Task<SL.Build> BuildAndReadBuildAsync(string buildBinlogPath)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = $"build --no-incremental /p:TreatWarningsAsErrors=false -bl:\"{buildBinlogPath}\"", 
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process process = Process.Start(psi)
            ?? throw new BiakApplicationException(WarningsBaselineBuildConstant.FAILED_TO_START_DOTNET_BUILD);

        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        using CancellationTokenSource timeoutCts = new(TimeSpan.FromMinutes(30));
        string standardOutput;
        string standardError;
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
            standardOutput = await standardOutputTask.WaitAsync(timeoutCts.Token);
            standardError = await standardErrorTask.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw new BiakApplicationException(WarningsBaselineBuildConstant.DOTNET_BUILD_TIMED_OUT);
        }

        if (process.ExitCode != 0)
        {
            string errorOutput = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
            throw new BiakApplicationException(
                string.IsNullOrWhiteSpace(errorOutput)
                    ? WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED
                    : $"{WarningsBaselineBuildConstant.DOTNET_BUILD_FAILED} {errorOutput.Trim()}"
            );
        }

        if (!File.Exists(buildBinlogPath))
        {
            throw new BiakApplicationException($"{WarningsBaselineBuildConstant.BUILD_BINLOG_NOT_FOUND} Path: '{buildBinlogPath}'.");
        }

        SL.Build build = SL.BinaryLog.ReadBuild(buildBinlogPath);

        if (build.FindChildrenRecursive<SL.Error>().Any())
        {
            throw new BiakApplicationException(WarningsBaselineBuildConstant.BUILD_CONTAINS_ERRORS);
        }

        return build;
    }

    /// <summary>
    /// Returns warnings from source files only and excludes entries without code/file data.
    /// </summary>
    /// <param name="build">Parsed build data.</param>
    /// <returns>Filtered warnings for C# and VB source files.</returns>
    public static IEnumerable<SL.Warning> GetSourceWarnings(SL.Build build)
    {
        return build.FindChildrenRecursive<SL.Warning>()
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Code) &&
                !string.IsNullOrWhiteSpace(x.File) &&
                s_sourceFileExtensions.Contains(Path.GetExtension(x.File))
            );
    }
}
