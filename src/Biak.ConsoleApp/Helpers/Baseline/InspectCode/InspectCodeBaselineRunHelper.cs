// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Helper that runs <c>jb inspectcode</c> and returns the path to the produced SARIF report.
/// </summary>
public static class InspectCodeBaselineRunHelper
{
    /// <summary>
    /// Runs <c>jb inspectcode</c> with SARIF output and returns the path to the produced report file.
    /// </summary>
    /// <param name="target">Explicit path to the <c>.slnx</c>, <c>.sln</c>, or <c>.csproj</c> file. When <c>null</c>, auto-discovery is used.</param>
    /// <param name="additionalArgs">Extra arguments forwarded to <c>jb inspectcode</c> unchanged.</param>
    /// <returns>Absolute path to the produced SARIF report file.</returns>
    public static async Task<string> RunAsync(string? target = null, IReadOnlyList<string>? additionalArgs = null)
    {
        string sarifPath = BuildSarifPath();

        string? directoryPath = Path.GetDirectoryName(sarifPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        ProcessStartInfo psi = new()
        {
            FileName = "jb",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        psi.ArgumentList.Add("inspectcode");
        psi.ArgumentList.Add(ResolveTarget(target));
        psi.ArgumentList.Add($"--output={sarifPath}");
        psi.ArgumentList.Add("-f=Sarif");

        if (additionalArgs is not null)
        {
            foreach (string arg in additionalArgs)
            {
                psi.ArgumentList.Add(arg);
            }
        }

        using Process process = Process.Start(psi)
            ?? throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.FAILED_TO_START_INSPECTCODE);

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

            throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.INSPECTCODE_TIMED_OUT);
        }

        if (process.ExitCode != 0)
        {
            string errorOutput = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
            throw new BiakApplicationException(
                string.IsNullOrWhiteSpace(errorOutput)
                    ? InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED
                    : $"{InspectCodeBaselineRunHelperConstant.INSPECTCODE_FAILED} {errorOutput.Trim()}"
            );
        }

        if (!File.Exists(sarifPath))
        {
            throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.SARIF_REPORT_NOT_FOUND);
        }

        return sarifPath;
    }

    private static string ResolveTarget(string? target)
    {
        string baseDir = Directory.GetCurrentDirectory();

        if (!string.IsNullOrWhiteSpace(target))
        {
            string fullPath = Path.IsPathRooted(target)
                ? target
                : Path.GetFullPath(Path.Combine(baseDir, target));

            if (!File.Exists(fullPath))
            {
                throw new BiakApplicationException(
                    InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_PREFIX +
                    fullPath +
                    InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_SUFFIX);
            }

            return fullPath;
        }

        string[] slnxFiles = Directory.GetFiles(baseDir, "*.slnx");
        if (slnxFiles.Length == 1)
        {
            return slnxFiles[0];
        }

        string[] slnFiles = Directory.GetFiles(baseDir, "*.sln");
        if (slnFiles.Length == 1)
        {
            return slnFiles[0];
        }

        string[] csprojFiles = Directory.GetFiles(baseDir, "*.csproj");
        if (csprojFiles.Length == 1)
        {
            return csprojFiles[0];
        }

        throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.NO_SOLUTION_OR_PROJECT_FOUND);
    }

    private static string BuildSarifPath()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string guid = Guid.NewGuid().ToString("N");
        string fileName = $"{timestamp}_{guid}.sarif";
        return Path.Combine(InspectCodeBaselineRunHelperConstant.REPORTS_DIRECTORY, fileName);
    }
}
