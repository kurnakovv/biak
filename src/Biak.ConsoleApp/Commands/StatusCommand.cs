// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak status` command.
/// </summary>
public static class StatusCommand
{
    /// <summary>
    /// Can `dotnet biak status` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        bool isStatusCommand = args is [CommandArgumentConstant.STATUS, ..];
        if (!isStatusCommand)
        {
            return false;
        }

        return args is [_] or [_, CommandArgumentConstant.DEBUG_INFO];
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync(AppExecutionContext executionContext, string[] args)
    {
        bool isDebugInfoEnabled = args is [CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO];

        BiakStatusResult result = await BiakStatusHelper.GetAsync(executionContext, isDebugInfoEnabled);
        await executionContext.Out.WriteLineAsync(result.Message);
    }
}
