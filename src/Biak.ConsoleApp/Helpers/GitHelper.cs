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
    /// <returns>git output.</returns>
    public static async Task<string> RunAsync(string arguments)
    {
        using Process process = new();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("GIT ERROR: " + error);
        }

        return output;
    }
}
