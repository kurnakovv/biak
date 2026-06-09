// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// All consts command argumets.
/// </summary>
public static class CommandArgumentConstant
{
    /// <summary>
    /// Help info.
    /// </summary>
    public const string HELP = "--help";

    /// <summary>
    /// Setup biak env (create .biak folder with .editorconfig-main file).
    /// </summary>
    public const string SETUP = "setup";

    /// <summary>
    /// Enable .editorconfig rules.
    /// </summary>
    public const string ENABLE = "enable";

    /// <summary>
    /// Disable .editorconfig rules.
    /// </summary>
    public const string DISABLE = "disable";

    /// <summary>
    /// Find active branches with files.
    /// </summary>
    public const string FIND_ACTIVITY = "find-activity";

    /// <summary>
    /// Find git conflicts files.
    /// </summary>
    public const string FIND_CONFLICTS = "find-conflicts";

    /// <summary>
    /// Warnings baseline.
    /// </summary>
    public const string WARNINGS_BASELINE = "warnings-baseline";

    /// <summary>
    /// Init.
    /// </summary>
    public const string INIT = "init";

    /// <summary>
    /// Sync.
    /// </summary>
    public const string SYNC = "sync";
}
