// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Helper that runs InspectCode and returns the path to the produced SARIF report.
/// </summary>
public static class InspectCodeBaselineRunHelper
{
    /// <summary>
    /// Runs InspectCode with SARIF output and returns the generated SARIF file path.
    /// </summary>
    /// <param name="executionContext">Provides the context required to perform the operation.</param>
    /// <param name="target">Explicit path to the <c>.slnx</c>, <c>.sln</c>, or <c>.csproj</c> file. When <c>null</c>, auto-discovery is used.</param>
    /// <param name="additionalArgs">Extra arguments forwarded to InspectCode unchanged.</param>
    /// <param name="debugMode">Enables verbose logging for every attempted InspectCode command.</param>
    /// <returns>Absolute path to the generated SARIF report file.</returns>
    public static async Task<string> RunAsync(
        AppExecutionContext executionContext,
        string? target = null,
        IReadOnlyList<string>? additionalArgs = null,
        bool debugMode = false
    )
    {
        string sarifPath = GenerateSarifPath(executionContext.WorkingDirectory);
        bool completedSuccessfully = false;

        try
        {
            string? directoryPath = Path.GetDirectoryName(sarifPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string resolvedTarget = ResolveTarget(target, executionContext.WorkingDirectory);

            IReadOnlyList<ProcessStartInfo> candidates = BuildInspectCodeProcessCandidates(
                resolvedTarget,
                sarifPath,
                executionContext.WorkingDirectory,
                additionalArgs
            );

            bool startedAnyCandidate = false;
            string? errorOutput = null;
            string? lastAttemptErrorOutput = null;
            bool lastAttemptProducedErrorOutput = false;

            for (int i = 0; i < candidates.Count; i++)
            {
                ProcessStartInfo candidate = candidates[i];
                int attemptNumber = i + 1;
                string formattedCommand = FormatCommand(candidate);

                if (debugMode)
                {
                    await executionContext.Out.WriteLineAsync($"InspectCode command attempt {attemptNumber}: {formattedCommand}");
                }

                try
                {
                    (int exitCode, string standardOutput, string standardError) = await RunProcessAsync(candidate);
                    startedAnyCandidate = true;

                    if (exitCode == 0)
                    {
                        errorOutput = null;
                        completedSuccessfully = true;
                        break;
                    }

                    errorOutput = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
                    lastAttemptErrorOutput = errorOutput;
                    lastAttemptProducedErrorOutput = true;

                    if (debugMode)
                    {
                        await executionContext.Out.WriteLineAsync($"InspectCode attempt {attemptNumber} exited with code {exitCode}.");
                        await executionContext.Out.WriteLineAsync();
                    }
                }
                catch (Win32Exception)
                {
                    lastAttemptErrorOutput = null;
                    lastAttemptProducedErrorOutput = false;

                    if (debugMode)
                    {
                        await executionContext.Out.WriteLineAsync($"InspectCode attempt {attemptNumber} failed to start.");
                        await executionContext.Out.WriteLineAsync();
                    }
                }
            }

            if (!startedAnyCandidate)
            {
                if (debugMode)
                {
                    await executionContext.Out.WriteLineAsync();
                }

                throw new BiakApplicationException(InspectCodeBaselineRunHelperConstant.FAILED_TO_START_INSPECTCODE);
            }

            if (errorOutput is not null)
            {
                if (debugMode && lastAttemptProducedErrorOutput && !string.IsNullOrWhiteSpace(lastAttemptErrorOutput))
                {
                    await executionContext.Out.WriteLineAsync("InspectCode last error output:");
                    await executionContext.Out.WriteLineAsync(lastAttemptErrorOutput.Trim());
                    await executionContext.Out.WriteLineAsync();
                }

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
        finally
        {
            if (debugMode && File.Exists(sarifPath))
            {
                string relativeSarifLogPath = Path.GetRelativePath(executionContext.WorkingDirectory, sarifPath);
                await executionContext.Out.WriteLineAsync($"InspectCode SARIF log: {relativeSarifLogPath}");
                await executionContext.Out.WriteLineAsync();
            }

            if (!completedSuccessfully && !debugMode && File.Exists(sarifPath))
            {
                File.Delete(sarifPath);
            }
        }
    }

    private static IReadOnlyList<ProcessStartInfo> BuildInspectCodeProcessCandidates(
        string resolvedTarget,
        string sarifPath,
        string workingDirectory,
        IReadOnlyList<string>? additionalArgs
    )
    {
        return new List<ProcessStartInfo>
        {
            CreateDotnetToolStartInfo(resolvedTarget, sarifPath, workingDirectory, additionalArgs),
            CreateJbStartInfo(resolvedTarget, sarifPath, workingDirectory, additionalArgs),
            CreateNativeInspectCodeStartInfo("InspectCode.exe", resolvedTarget, sarifPath, workingDirectory, additionalArgs),
            CreateNativeInspectCodeStartInfo("inspectcode", resolvedTarget, sarifPath, workingDirectory, additionalArgs),
        };
    }

    private static ProcessStartInfo CreateDotnetToolStartInfo(
        string resolvedTarget,
        string sarifPath,
        string workingDirectory,
        IReadOnlyList<string>? additionalArgs
    )
    {
        ProcessStartInfo psi = CreateProcessStartInfo("dotnet", workingDirectory);
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
        string workingDirectory,
        IReadOnlyList<string>? additionalArgs
    )
    {
        ProcessStartInfo psi = CreateProcessStartInfo("jb", workingDirectory);
        psi.ArgumentList.Add("inspectcode");
        ConfigureInspectCodeRunArguments(psi, resolvedTarget, sarifPath, additionalArgs);
        return psi;
    }

    private static ProcessStartInfo CreateNativeInspectCodeStartInfo(
        string fileName,
        string resolvedTarget,
        string sarifPath,
        string workingDirectory,
        IReadOnlyList<string>? additionalArgs
    )
    {
        ProcessStartInfo psi = CreateProcessStartInfo(fileName, workingDirectory);
        ConfigureInspectCodeRunArguments(psi, resolvedTarget, sarifPath, additionalArgs);
        return psi;
    }

    private static void ConfigureInspectCodeRunArguments(
        ProcessStartInfo psi,
        string resolvedTarget,
        string sarifPath,
        IReadOnlyList<string>? additionalArgs
    )
    {
        psi.ArgumentList.Add(resolvedTarget);
        psi.ArgumentList.Add($"-o={sarifPath}");
        psi.ArgumentList.Add("-f=Sarif");
        AppendAdditionalArgs(psi, additionalArgs);
    }

    private static ProcessStartInfo CreateProcessStartInfo(string fileName, string workingDirectory)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
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

    private static string ResolveTarget(string? target, string baseDir)
    {
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
                    InspectCodeBaselineRunHelperConstant.TARGET_NOT_FOUND_SUFFIX
                );
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

    private static string FormatCommand(ProcessStartInfo psi)
    {
        List<string> parts = [EscapeCommandPart(psi.FileName)];
        parts.AddRange(psi.ArgumentList.Select(EscapeCommandPart));
        return string.Join(" ", parts);
    }

    private static string EscapeCommandPart(string value)
    {
        bool requiresQuotes = string.IsNullOrEmpty(value)
            || value.Any(ch => char.IsWhiteSpace(ch) || ch == '"');
        return requiresQuotes
            ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : value;
    }

    private static string GenerateSarifPath(string workingDirectory)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string guid = Guid.NewGuid().ToString("N");
        string fileName = $"{timestamp}_{guid}.sarif";
        return Path.GetFullPath(Path.Join(InspectCodeBaselineRunHelperConstant.REPORTS_DIRECTORY, fileName), workingDirectory);
    }
}
