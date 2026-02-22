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
        return args.Length == 1 && args.Single() == CommandArgumentConstant.ENABLE;
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

        Console.WriteLine(UIConstant.START_ENABLE);

        string content = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        await File.WriteAllTextAsync(editorconfigPaths.Value, content);

        Console.WriteLine(UIConstant.END_ENABLE);
    }
}
