// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak warnings-baseline sync` command constants.
/// </summary>
public static class WarningsBaselineSyncCommandConstant
{
    /// <summary>
    /// Build binary log file path.
    /// </summary>
    public const string BUILD_BINLOG_PATH = "biak-sync-build.binlog";

    /// <summary>
    /// Main .editorconfig path inside .biak folder.
    /// </summary>
    public const string DEFAULT_EDITORCONFIG_MAIN_PATH = ".biak/.editorconfig-main";

    /// <summary>
    /// Root .editorconfig path.
    /// </summary>
    public const string DEFAULT_EDITORCONFIG_PATH = ".editorconfig";

    /// <summary>
    /// Warnings-baseline sync started.
    /// </summary>
    public const string SYNC_STARTED = "warnings-baseline sync started...";

    /// <summary>
    /// The specified file was not found.
    /// </summary>
    public const string FILE_NOT_FOUND = "The specified .editorconfig file was not found.";

    /// <summary>
    /// Default configuration files were not found.
    /// </summary>
    public const string DEFAULT_CONFIGURATION_FILE_NOT_FOUND =
        $"Configuration file was not found. Checked '{DEFAULT_EDITORCONFIG_MAIN_PATH}' and '{DEFAULT_EDITORCONFIG_PATH}'.";

    /// <summary>
    /// The file path escapes the current directory.
    /// </summary>
    public const string PATH_ESCAPES_DIRECTORY =
        "The specified file path is not allowed because it escapes the current directory. " +
        "Please use a path within the current project.";

    /// <summary>
    /// The file does not contain the baseline marker.
    /// </summary>
    public const string NO_BASELINE_MARKER =
        $"The specified .editorconfig file does not contain any '{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}' entries. " +
        "Run 'dotnet biak warnings-baseline init' first.";

    /// <summary>
    /// All baseline warnings have been fixed.
    /// </summary>
    public const string ALL_WARNINGS_FIXED =
        "Congratulations! All baseline warnings have been fixed. All filters have been removed from the configuration file.";

    /// <summary>
    /// Failed to synchronize warnings baseline.
    /// </summary>
    public const string SYNC_FAILED = "Failed to synchronize warnings baseline.";
}
