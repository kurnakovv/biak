// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using Biak.ConsoleApp.Constants;
using Xunit.Abstractions;

namespace Biak.ConsoleApp.IntegrationTests;

public class ProgramTests
{
    private readonly string _ddlPath;
    private readonly ITestOutputHelper _output;

    public ProgramTests(ITestOutputHelper output)
    {
        string toolProjectPath = Path.GetFullPath(ToolProjectPath);

        _ddlPath = Path.Combine(
            Path.GetDirectoryName(toolProjectPath)!,
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
        _output = output;
    }

    public static string ToolProjectPath =>
        typeof(ProgramTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(x => x.Key == "ToolProjectPath")
            .Value!;

    [Fact]
    public async Task NoArgumentsGreetingAsync()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = $"\"{_ddlPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        // Act
        using Process process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        string standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        _output.WriteLine($"{nameof(output)}: {output}");
        _output.WriteLine($"{nameof(standardError)}: {standardError}");

        Assert.True(process.ExitCode == 0, output);
        Assert.Contains(DocsConstant.GREETING, output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidCommandNoCommandMessageAsync()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = $"\"{_ddlPath}\" invalidCommand",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        // Act
        using Process process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        string standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        _output.WriteLine($"{nameof(output)}: {output}");
        _output.WriteLine($"{nameof(standardError)}: {standardError}");

        Assert.True(process.ExitCode == 0, output);
        Assert.Contains(DocsConstant.NO_COMMAND, output, StringComparison.OrdinalIgnoreCase);
    }
}
