// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;

namespace Biak.ConsoleApp.IntegrationTests.Tools;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string command,
        string? workingDirectory = null,
        string? standardInput = null);
}

public sealed class ProcessRunner : IProcessRunner
{
    private readonly string _ddlPath;
    private readonly ITestOutputHelper _output;

    public ProcessRunner(ITestOutputHelper output)
    {
        _output = output;

        _ddlPath = Path.Combine(
            Path.GetDirectoryName(ToolProjectPath)!,
            "bin",
#if DEBUG
            "Debug",
#endif
#if RELEASE
            "Release",
#endif
            "net8.0",
            "kurnakovv.biak.dll"
        );
    }

    public static string ToolProjectPath =>
        typeof(ProgramTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(x => x.Key == "ToolProjectPath")
            .Value!;

    public async Task<ProcessResult> RunAsync(
        string command,
        string? workingDirectory = null,
        string? standardInput = null)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = $"\"{_ddlPath}\" {command}",
            WorkingDirectory = workingDirectory ?? string.Empty,
            RedirectStandardInput = standardInput is not null,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process process = Process.Start(psi)!;

        if (standardInput is not null)
        {
            await process.StandardInput.WriteLineAsync(standardInput);
            process.StandardInput.Close();
        }

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        _output.WriteLine($"ExitCode: {process.ExitCode}");

        if (!string.IsNullOrWhiteSpace(output))
        {
            _output.WriteLine("OUTPUT:");
            _output.WriteLine(output);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            _output.WriteLine("ERROR:");
            _output.WriteLine(error);
        }

        return new ProcessResult(process.ExitCode, output, error);
    }
}

public sealed record ProcessResult(
    int ExitCode,
    string Output,
    string Error
);
