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
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync()
    {
        EditorconfigPaths editorconfigPaths = SetupHelper.GetEditorconfigPaths();

        if (editorconfigPaths.MainValue == null || editorconfigPaths.Value == null)
        {
            return;
        }

        Console.WriteLine(UIConstant.START_DISABLE);

        string content = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync();
        if (message != null)
        {
            Console.WriteLine(message);
        }
        content = SeverityHelper.Disable(content, config.SeveritiesToDisable, config.SeverityWhenDisabled);
        content = EditorconfigHelper.AddAttentionBanners(content);
        await File.WriteAllTextAsync(editorconfigPaths.Value, content);

        Console.WriteLine(UIConstant.END_DISABLE);
    }
}
