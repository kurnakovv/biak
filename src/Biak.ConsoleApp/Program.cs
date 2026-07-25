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
        AppExecutionContext executionContext = AppExecutionContext.CreateDefault();

        await MainInnerAsync(args, executionContext);
    }

    /// <summary>
    /// Main inner for tests.
    /// </summary>
    /// <param name="args">args.</param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task MainInnerAsync(string[] args, AppExecutionContext? executionContext = null)
    {
        executionContext ??= AppExecutionContext.CreateDefault();

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
            await SetupCommand.RunAsync(executionContext);
        }
        else if (DisableCommand.IsRunnable(args))
        {
            await DisableCommand.RunAsync(executionContext);
        }
        else if (EnableCommand.IsRunnable(args))
        {
            await EnableCommand.RunAsync(executionContext);
        }
        else if (StatusCommand.IsRunnable(args))
        {
            await StatusCommand.RunAsync(args, executionContext);
        }
        else if (FindActivityCommand.IsRunnable(args))
        {
            await FindActivityCommand.RunAsync(executionContext);
        }
        else if (FindConflictsCommand.IsRunnable(args))
        {
            await FindConflictsCommand.RunAsync(executionContext);
        }
        else if (WarningsBaselineInitCommand.IsRunnable(args))
        {
            await WarningsBaselineInitCommand.RunAsync(args, executionContext);
        }
        else if (WarningsBaselineSyncCommand.IsRunnable(args))
        {
            await WarningsBaselineSyncCommand.RunAsync(args, executionContext);
        }
        else if (InspectCodeBaselineInitCommand.IsRunnable(args))
        {
            await InspectCodeBaselineInitCommand.RunAsync(executionContext);
        }
        else if (InspectCodeBaselineSyncCommand.IsRunnable(args))
        {
            await InspectCodeBaselineSyncCommand.RunAsync(args, executionContext);
        }
        else
        {
            Console.WriteLine(UIConstant.NO_COMMAND);
        }
    }
}
