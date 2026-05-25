// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp;

/// <summary>
/// Program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main.
    /// </summary>
    /// <param name="args">args.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(DocsConstant.GREETING);
        }
        else if (args.Length == 1 && args[0] == CommandArgumentConstant.HELP)
        {
            Console.WriteLine(DocsConstant.HELP);
        }
        else if (SetupCommand.IsRunnable(args))
        {
            await SetupCommand.RunAsync();
        }
        else if (DisableCommand.IsRunnable(args))
        {
            await DisableCommand.RunAsync();
        }
        else if (EnableCommand.IsRunnable(args))
        {
            await EnableCommand.RunAsync();
        }
        else if (FindActivityCommand.IsRunnable(args))
        {
            await FindActivityCommand.RunAsync();
        }
        else if (FindConflictsCommand.IsRunnable(args))
        {
            await FindConflictsCommand.RunAsync();
        }
        else
        {
            Console.WriteLine(UIConstant.NO_COMMAND);
        }
    }
}
