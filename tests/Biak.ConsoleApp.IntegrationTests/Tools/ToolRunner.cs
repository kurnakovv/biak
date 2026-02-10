// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Biak.ConsoleApp.IntegrationTests.Tools;

public sealed class ToolRunner
{
    private readonly string _toolPath;

    public ToolRunner(string toolPath)
    {
        _toolPath = toolPath;
    }

    public async Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(string args)
    {
        ProcessStartInfo psi = new()
        {
            FileName = Path.Combine(_toolPath, "dotnet-biak"),
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process process = Process.Start(psi)!;

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, stdout, stderr);
    }
}
