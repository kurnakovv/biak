// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Helper that runs InspectCode and returns the path to the produced SARIF report.
/// </summary>
public static class InspectCodeBaselineRunHelper
{
    /// <summary>
    /// Runs InspectCode with SARIF output and returns the path to the produced report file.
    /// </summary>
    /// <param name="target">Explicit path to the <c>.slnx</c>, <c>.sln</c>, or <c>.csproj</c> file. When <c>null</c>, auto-discovery is used.</param>
    /// <param name="additionalArgs">Extra arguments forwarded to InspectCode unchanged.</param>
    /// <returns>Absolute path to the produced SARIF report file.</returns>
    public static async Task<string> RunAsync(string? target = null, IReadOnlyList<string>? additionalArgs = null)
    {
        string sarifPath = GenerateSarifPath();

        string? directoryPath = Path.GetDirectoryName(sarifPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string resolvedTarget = ResolveTarget(target);

        IReadOnlyList<ProcessStartInfo> candidates = BuildInspectCodeProcessCandidates(
            resolvedTarget,
            sarifPath,
            additionalArgs);

        bool startedAnyCandidate = false;
        string? errorOutput = null;

        foreach (ProcessStartInfo candidate in candidates)
        {
            try
            {
                (int exitCode, string standardOutput, string standardError) = await RunProcessAsync(candidate);
                startedAnyCandidate = true;

                if (exitCode == 0)
                {
                    errorOutput = null;
                    break;
                }

                errorOutput = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
            }
            catch (Win32Exception)
            {
                // Candidate executable is not available, try next one.
            }
        }

        if (!startedAnyCandidate)
        {
            throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.FAILED_TO_START_INSPECTCODE);
        }

        if (errorOutput is not null)
        {
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

    private static IReadOnlyList<ProcessStartInfo> BuildInspectCodeProcessCandidates(
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs)
    {
        return new List<ProcessStartInfo>()
        {
            CreateDotnetToolStartInfo(resolvedTarget, sarifPath, additionalArgs),
            CreateJbStartInfo(resolvedTarget, sarifPath, additionalArgs),
            CreateNativeInspectCodeStartInfo("InspectCode.exe", resolvedTarget, sarifPath, additionalArgs),
            CreateNativeInspectCodeStartInfo("inspectcode", resolvedTarget, sarifPath, additionalArgs),
        };
    }

    private static ProcessStartInfo CreateDotnetToolStartInfo(
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs)
    {
        ProcessStartInfo psi = CreateProcessStartInfo("dotnet");
        psi.ArgumentList.Add("tool");
        psi.ArgumentList.Add("run");
        psi.ArgumentList.Add("jb");
        psi.ArgumentList.Add("inspectcode");
        ConfigureInspectCodeRunArguments(psi, resolvedTarget, sarifPath, additionalArgs);
        return psi;
    }

    private static ProcessStartInfo CreateJbStartInfo(
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs)
    {
        ProcessStartInfo psi = CreateProcessStartInfo("jb");
        psi.ArgumentList.Add("inspectcode");
        ConfigureInspectCodeRunArguments(psi, resolvedTarget, sarifPath, additionalArgs);
        return psi;
    }

    private static ProcessStartInfo CreateNativeInspectCodeStartInfo(
        string fileName,
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs)
    {
        ProcessStartInfo psi = CreateProcessStartInfo(fileName);
        ConfigureInspectCodeRunArguments(psi, resolvedTarget, sarifPath, additionalArgs);
        return psi;
    }

    private static void ConfigureInspectCodeRunArguments(
        ProcessStartInfo psi,
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs)
    {
        psi.ArgumentList.Add(resolvedTarget);
        psi.ArgumentList.Add($"-o={sarifPath}");
        psi.ArgumentList.Add("-f=Sarif");
        AppendAdditionalArgs(psi, additionalArgs);
    }

    private static ProcessStartInfo CreateProcessStartInfo(string fileName)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
    }

    private static void AppendAdditionalArgs(ProcessStartInfo psi, IReadOnlyList<string>? additionalArgs)
    {
        if (additionalArgs is null)
        {
            return;
        }

        foreach (string arg in additionalArgs)
        {
            psi.ArgumentList.Add(arg);
        }
    }

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> RunProcessAsync(ProcessStartInfo psi)
    {
        using Process process = Process.Start(psi)
            ?? throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.FAILED_TO_START_INSPECTCODE);

        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        using CancellationTokenSource timeoutCts = new(TimeSpan.FromMinutes(30));

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
            string standardOutput = await standardOutputTask.WaitAsync(timeoutCts.Token);
            string standardError = await standardErrorTask.WaitAsync(timeoutCts.Token);
            return (process.ExitCode, standardOutput, standardError);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.INSPECTCODE_TIMED_OUT);
        }
    }

    private static string ResolveTarget(string? target)
    {
        string baseDir = Directory.GetCurrentDirectory();

        if (!string.IsNullOrWhiteSpace(target))
        {
            string fullPath = Path.IsPathRooted(target)
                ? target
                : Path.GetFullPath(Path.Join(baseDir, target));

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

    private static string GenerateSarifPath()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string guid = Guid.NewGuid().ToString("N");
        string fileName = $"{timestamp}_{guid}.sarif";
        return Path.Join(InspectCodeBaselineRunHelperConstant.REPORTS_DIRECTORY, fileName);
    }
}
