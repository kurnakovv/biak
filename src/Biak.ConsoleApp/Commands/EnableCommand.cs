// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak enable` command.
/// </summary>
public static class EnableCommand
{
    /// <summary>
    /// Can `dotnet biak enable` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args is [CommandArgumentConstant.ENABLE];
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync(AppExecutionContext executionContext)
    {
        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(executionContext: executionContext);

        if (editorconfigPaths.MainValue == null || editorconfigPaths.Value == null)
        {
            return;
        }

        (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync(executionContext: executionContext);
        if (message != null)
        {
            await executionContext.Out.WriteLineAsync(message);
        }

        await executionContext.Out.WriteLineAsync(UIConstant.START_ENABLE);

        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        string content = await EditorconfigHelper.GetEnabledContentAsync(editorconfigMainContent, config, executionContext);
        await File.WriteAllTextAsync(editorconfigPaths.Value, content);

        await executionContext.Out.WriteLineAsync(UIConstant.END_ENABLE);
    }
}
