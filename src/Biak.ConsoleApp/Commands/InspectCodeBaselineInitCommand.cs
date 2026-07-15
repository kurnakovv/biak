// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak inspectcode-baseline init` command.
/// </summary>
public static class InspectCodeBaselineInitCommand
{
    /// <summary>
    /// Can `dotnet biak inspectcode-baseline init` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        if (args.Length < 2)
        {
            return false;
        }

        return args[0] == CommandArgumentConstant.INSPECTCODE_BASELINE
            && args[1] == CommandArgumentConstant.INIT;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static Task<string> RunAsync(string[]? args = null)
    {
        try
        {
            Console.WriteLine(InspectCodeBaselineInitCommandConstant.INIT_STARTED);

            throw new NotImplementedException();
        }
        catch (Exception ex) when (ex is not BiakApplicationException and not NotImplementedException)
        {
            throw new BiakApplicationException($"{InspectCodeBaselineInitCommandConstant.INIT_FAILED} {ex.Message}");
        }
    }
}
