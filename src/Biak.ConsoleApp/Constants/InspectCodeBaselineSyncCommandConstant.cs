// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak inspectcode-baseline sync` command constants.
/// </summary>
public static class InspectCodeBaselineSyncCommandConstant
{
    /// <summary>
    /// Default .editorconfig search directory when path is not explicitly provided.
    /// </summary>
    public const string DEFAULT_EDITORCONFIG_SEARCH_DIRECTORY = ".biak";

    /// <summary>
    /// Root .editorconfig path fallback.
    /// </summary>
    public const string DEFAULT_EDITORCONFIG_PATH = ".editorconfig";

    /// <summary>
    /// InspectCode baseline sync started.
    /// </summary>
    public const string SYNC_STARTED = "inspectcode-baseline sync started...";

    /// <summary>
    /// The specified file was not found.
    /// </summary>
    public const string FILE_NOT_FOUND = "The specified .editorconfig file was not found.";

    /// <summary>
    /// The specified path is not allowed.
    /// </summary>
    public const string INVALID_PATH_EDITORCONFIG =
        "The specified file path is not allowed. Use a path within the current project, with a file name that starts with '.editorconfig', and not under a '.editorconfig' directory.";

    /// <summary>
    /// No baseline marker was found in discovered baseline files.
    /// </summary>
    public const string NO_BASELINE_MARKER =
        $"No '{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}' entries were found. Run 'dotnet biak inspectcode-baseline init' first.";

    /// <summary>
    /// All inspectcode baseline filters have been resolved.
    /// </summary>
    public const string ALL_ISSUES_FIXED =
        "Congratulations! All inspectcode baseline filters have been resolved. All baseline entries have been removed from the configuration file.";

    /// <summary>
    /// Failed to sync InspectCode baseline.
    /// </summary>
    public const string SYNC_FAILED = "Failed to sync InspectCode baseline.";
}
