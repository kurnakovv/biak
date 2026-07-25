// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Provides helper methods for setup-related operations, such as locating configuration files required by the
/// application.
/// </summary>
public static class SetupHelper
{
    /// <summary>
    /// Get .editorconfig paths.
    /// </summary>
    /// <param name="executionContext">Execution context with working-directory settings; when <c>null</c>, the default context is used.</param>
    /// <param name="suppressConsoleOutput">When true, does not write missing-file diagnostics to console output.</param>
    /// <returns>.editorconfig paths.</returns>
    public static async Task<EditorconfigPaths> GetEditorconfigPathsAsync(AppExecutionContext executionContext, bool suppressConsoleOutput = false)
    {
        EditorconfigPaths result = new();

        string currentDirectory = executionContext.WorkingDirectory;

        string editorconfigMainPath = Path.Join(currentDirectory, ".biak", ".editorconfig-main");

        if (!File.Exists(editorconfigMainPath))
        {
            if (!suppressConsoleOutput)
            {
                await executionContext.Out.WriteLineAsync(UIConstant.BIAK_NOT_INITIALIZED);
                await executionContext.Out.WriteLineAsync(UIConstant.RUN_BIAK_SETUP);
            }

            return result;
        }
        result.MainValue = editorconfigMainPath;

        string editorConfigPath = Path.Join(currentDirectory, ".editorconfig");

        if (!File.Exists(editorConfigPath))
        {
            if (!suppressConsoleOutput)
            {
                await executionContext.Out.WriteLineAsync(UIConstant.EDITORCONFIG_NOT_FOUND + editorConfigPath);
            }

            return result;
        }
        result.Value = editorConfigPath;

        return result;
    }
}
