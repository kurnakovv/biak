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
        bool isStatusCommand = args.Length > 0 && args[0] == CommandArgumentConstant.STATUS;
        if (!isStatusCommand)
        {
            return false;
        }

        return args.Length == 1
            || (args.Length == 2 && args[1] == CommandArgumentConstant.DEBUG_INFO);
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync(string[] args)
    {
        bool isDebugInfoEnabled = args?.Length == 2
            && args[0] == CommandArgumentConstant.STATUS
            && args[1] == CommandArgumentConstant.DEBUG_INFO;

        EditorconfigPaths editorconfigPaths = SetupHelper.GetEditorconfigPaths(suppressConsoleOutput: true);

        if (editorconfigPaths.MainValue == null)
        {
            Console.WriteLine(
                isDebugInfoEnabled
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.BIAK_NOT_INITIALIZED + " " + UIConstant.RUN_BIAK_SETUP
                    : UIConstant.STATUS_BROKEN
            );
            return;
        }

        if (editorconfigPaths.Value == null)
        {
            Console.WriteLine(
                isDebugInfoEnabled
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.EDITORCONFIG_NOT_FOUND + Path.Join(Directory.GetCurrentDirectory(), ".editorconfig")
                    : UIConstant.STATUS_BROKEN
            );
            return;
        }

        (string? message, BiakConfig config) = await BiakConfigHelper.GetAsync();
        if (message != null)
        {
            Console.WriteLine(
                isDebugInfoEnabled
                    ? UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + message
                    : UIConstant.STATUS_BROKEN
            );
            return;
        }

        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigPaths.MainValue);
        string currentContent = NormalizeLineEndings(await File.ReadAllTextAsync(editorconfigPaths.Value));
        string disabledContent = NormalizeLineEndings(
            await EditorconfigHelper.GetDisabledContentAsync(editorconfigMainContent, config)
        );

        if (string.Equals(currentContent, disabledContent, StringComparison.Ordinal))
        {
            Console.WriteLine(UIConstant.STATUS_DISABLED);
            return;
        }

        string enabledContent = NormalizeLineEndings(
            await EditorconfigHelper.GetEnabledContentAsync(editorconfigMainContent, config)
        );

        Console.WriteLine(
            string.Equals(currentContent, enabledContent, StringComparison.Ordinal)
                ? UIConstant.STATUS_ENABLED
                : UIConstant.STATUS_UNSYNCHRONISED
        );
    }

    private static string NormalizeLineEndings(string content)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
    }
}
