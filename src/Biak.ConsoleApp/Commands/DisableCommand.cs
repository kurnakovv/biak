// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;

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
        return args.Length == 1 && args.Single() == CommandArgumentConstant.DISABLE;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        string editorconfigMainPath = Path.Join(currentDirectory, ".biak", ".editorconfig-main");

        if (!File.Exists(editorconfigMainPath))
        {
            Console.WriteLine(UIConstant.BIAK_NOT_INITIALIZED);
            Console.WriteLine(UIConstant.RUN_BIAK_SETUP);
            return;
        }

        string editorConfigPath = Path.Join(currentDirectory, ".editorconfig");

        if (!File.Exists(editorConfigPath))
        {
            Console.WriteLine(UIConstant.EDITORCONFIG_NOT_FOUND + editorConfigPath);
            return;
        }

        Console.WriteLine(UIConstant.START_DISABLE);

        string content = await File.ReadAllTextAsync(editorconfigMainPath);
        string disabledContent = SeverityHelper.Disable(content);

        await File.WriteAllTextAsync(editorConfigPath, disabledContent);

        Console.WriteLine(UIConstant.END_DISABLE);
    }
}
