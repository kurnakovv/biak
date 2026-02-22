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
    /// <returns>.editorconfig paths.</returns>
    public static EditorconfigPaths GetEditorconfigPaths()
    {
        EditorconfigPaths result = new();

        string currentDirectory = Directory.GetCurrentDirectory();

        string editorconfigMainPath = Path.Join(currentDirectory, ".biak", ".editorconfig-main");

        if (!File.Exists(editorconfigMainPath))
        {
            Console.WriteLine(UIConstant.BIAK_NOT_INITIALIZED);
            Console.WriteLine(UIConstant.RUN_BIAK_SETUP);
            return result;
        }
        result.MainValue = editorconfigMainPath;

        string editorConfigPath = Path.Join(currentDirectory, ".editorconfig");

        if (!File.Exists(editorConfigPath))
        {
            Console.WriteLine(UIConstant.EDITORCONFIG_NOT_FOUND + editorConfigPath);
            return result;
        }
        result.Value = editorConfigPath;

        return result;
    }
}
