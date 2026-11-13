// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers;

internal static class FindBranchInputHelper
{
    private static readonly CompositeFormat s_defaultBranchInputFormat =
        CompositeFormat.Parse(SharedFindCommandConstant.TARGET_BRANCH_INPUT);

    internal static async Task<string> RequestAsync(AppExecutionContext executionContext)
    {
        GitResult result = await GitHelper.RunWithModelAsync("branch --show-current", executionContext);
        string? currentBranch = result.ExitCode == 0 ? result.Output.Trim() : null;
        bool hasCurrentBranch = !string.IsNullOrWhiteSpace(currentBranch);

        string prompt = hasCurrentBranch
            ? string.Format(CultureInfo.InvariantCulture, s_defaultBranchInputFormat, currentBranch)
            : SharedFindCommandConstant.EXPLICIT_BRANCH_INPUT;
        await executionContext.Out.WriteAsync(prompt);
        string? branch = await executionContext.In.ReadLineAsync();
        await executionContext.Out.WriteLineAsync();

        if (!string.IsNullOrWhiteSpace(branch))
        {
            return branch;
        }

        if (hasCurrentBranch)
        {
            return currentBranch!;
        }

        throw new BiakApplicationException("Current branch could not be determined. An explicit branch is required.");
    }
}
