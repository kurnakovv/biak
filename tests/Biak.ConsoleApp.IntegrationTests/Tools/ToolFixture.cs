// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;

namespace Biak.ConsoleApp.IntegrationTests.Tools;

public sealed class ToolFixture : IAsyncLifetime
{
    private readonly string _toolProject;

    public ToolFixture()
    {
        _toolProject =
            typeof(ProgramTests).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(x => x.Key == "ToolProjectPath")
                .Value!;
    }

    public string ToolPath { get; } =
        Path.Combine(Path.GetTempPath(), "biak-tool-" + Guid.NewGuid());

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(ToolPath);

        await RunAsync("dotnet", $"pack \"{_toolProject}\" -c Debug -o ./nupkg");
        await RunAsync(
            "dotnet",
            $"tool install kurnakovv.biak " +
            $"--add-source ./nupkg " +
            $"--tool-path \"{ToolPath}\""
        );
    }

    public async Task DisposeAsync()
    {
        try
        {
            await RunAsync("dotnet", $"tool uninstall kurnakovv.biak --tool-path \"{ToolPath}\"");
        }
        catch
        {
            /* ignore */
        }

        if (Directory.Exists(ToolPath))
        {
            Directory.Delete(ToolPath, recursive: true);
        }
    }

    private static async Task RunAsync(string file, string args)
    {
        using Process p = Process.Start(new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        })!;

        string stdout = await p.StandardOutput.ReadToEndAsync();
        string stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException(stdout + Environment.NewLine + stderr);
        }
    }
}
