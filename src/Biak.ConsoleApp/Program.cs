// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Commands.Baseline.InspectCode;
using Biak.ConsoleApp.Commands.Baseline.Warnings;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

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
        AppExecutionContext context = AppExecutionContext.CreateDefault();

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
            await SetupCommand.RunAsync(context);
        }
        else if (DisableCommand.IsRunnable(args))
        {
            await DisableCommand.RunAsync(context);
        }
        else if (EnableCommand.IsRunnable(args))
        {
            await EnableCommand.RunAsync(context);
        }
        else if (StatusCommand.IsRunnable(args))
        {
            await StatusCommand.RunAsync(args, context);
        }
        else if (FindActivityCommand.IsRunnable(args))
        {
            await FindActivityCommand.RunAsync(context);
        }
        else if (FindConflictsCommand.IsRunnable(args))
        {
            await FindConflictsCommand.RunAsync(context);
        }
        else if (WarningsBaselineInitCommand.IsRunnable(args))
        {
            await WarningsBaselineInitCommand.RunAsync(args, context);
        }
        else if (WarningsBaselineSyncCommand.IsRunnable(args))
        {
            await WarningsBaselineSyncCommand.RunAsync(args, context);
        }
        else if (InspectCodeBaselineInitCommand.IsRunnable(args))
        {
            await InspectCodeBaselineInitCommand.RunAsync(context);
        }
        else if (InspectCodeBaselineSyncCommand.IsRunnable(args))
        {
            await InspectCodeBaselineSyncCommand.RunAsync(args, context);
        }
        else
        {
            Console.WriteLine(UIConstant.NO_COMMAND);
        }
    }
}
