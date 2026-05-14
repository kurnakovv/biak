// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// git helper.
/// </summary>
public static class GitHelper
{
    /// <summary>
    /// Run git + arguments command.
    /// </summary>
    /// <param name="arguments">Arguments after git.</param>
    /// <param name="ignoreExitCode">Ignore exit code (Do not throw exit code 1).</param>
    /// <returns>git output.</returns>
    public static async Task<string> RunAsync(string arguments, bool ignoreExitCode = false)
    {
        using Process process = new();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask);

        await process.WaitForExitAsync();

        string output = await outputTask;
        string error = await errorTask;

        if (!ignoreExitCode && process.ExitCode != 0)
        {
            Console.WriteLine("GIT ERROR: " + error);
            Environment.Exit(1);
        }

        return output;
    }
}
