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

        // ToDo: Get correct .net version
        _ddlPath = Directory
            .EnumerateFiles(
                Path.GetDirectoryName(toolProjectPath)!,
                "kurnakovv.biak.*",
                SearchOption.AllDirectories
            )
            .First(f => f.EndsWith(".exe") || f.EndsWith(".dll"));
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
        _output.WriteLine("Test message from biak!");

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
        await process.WaitForExitAsync();

        Assert.True(process.ExitCode == 0, output);
        Assert.Contains(DocsConstant.GREETING, output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidCommandNoCommandMessageAsync()
    {
        _output.WriteLine("Test message from biak!");

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
        await process.WaitForExitAsync();

        _output.WriteLine(output);

        Assert.True(process.ExitCode == 0, output);
        Assert.Contains(DocsConstant.NO_COMMAND, output, StringComparison.OrdinalIgnoreCase);
    }
}
