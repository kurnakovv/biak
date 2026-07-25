// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak disable` command.
/// </summary>
public static class DisableCommand
{
    /// <summary>
    /// Can `dotnet biak disable` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args.Length == 1 && args[0] == CommandArgumentConstant.DISABLE;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="executionContext">Execution context with working directory for command execution.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync(AppExecutionContext? executionContext = null)
    {
        AppExecutionContext context = executionContext ?? AppExecutionContext.CreateDefault();
        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(executionContext: context);

        if (editorconfigPaths.MainValue == null || editorconfigPaths.Value == null)
        {
            return;
        }

        await context.Out.WriteLineAsync(UIConstant.START_DISABLE);

        (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync(executionContext: context);
        if (message != null)
        {
            await context.Out.WriteLineAsync(message);
        }

        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        string content = await EditorconfigHelper.GetDisabledContentAsync(editorconfigMainContent, config, context);
        await File.WriteAllTextAsync(editorconfigPaths.Value, content);

        await context.Out.WriteLineAsync(UIConstant.END_DISABLE);
    }
}
