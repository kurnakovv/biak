// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;

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
        GitResult model = await RunWithModelAsync(arguments);

        if (model.ExitCode != 0)
        {
            throw new BiakApplicationException(GitHelperConstant.GIT_ERROR + model.Error);
        }

        return model.Output;
    }

    /// <summary>
    /// Run without exit code throw.
    /// </summary>
    /// <param name="arguments">Arguments after git.</param>
    /// <returns>A <see cref="GitResult"/> containing the exit code, standard output, and standard error from git.</returns>
    public static async Task<GitResult> RunWithModelAsync(string arguments)
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

        return new GitResult
        {
            ExitCode = process.ExitCode,
            Output = await outputTask,
            Error = await errorTask,
        };
    }
}

/// <summary>
/// git result model.
/// </summary>
public class GitResult
{
    /// <summary>
    /// Exit code.
    /// </summary>
    public int ExitCode { get; init; }

    /// <summary>
    /// Output.
    /// </summary>
    public string Output { get; init; } = null!;

    /// <summary>
    /// Error.
    /// </summary>
    public string Error { get; init; } = null!;
}
