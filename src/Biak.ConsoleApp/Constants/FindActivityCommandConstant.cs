// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak find-activity`.
/// </summary>
public static class FindActivityCommandConstant
{
    /// <summary>
    /// Enter input criteria message.
    /// </summary>
    public const string ENTER_CRITERIA = "Please enter the desired criteria";

    /// <summary>
    /// Default branch input message.
    /// </summary>
    public const string DEFAULT_BRANCH_INPUT = "Default branch ('main' by default): ";

    /// <summary>
    /// Expiration period input message.
    /// </summary>
    public const string EXPIRATION_PERIOD_INPUT = "Expiration period in days (default: 30, '*' for unlimited): ";

    /// <summary>
    /// Invalid expiration period format message.
    /// </summary>
    public const string INVALID_EXPIRATION_PERIOD_FORMAT = "Invalid expiration period format";

    /// <summary>
    /// Invalid expiration period format message in config file.
    /// </summary>
    public const string INVALID_EXPIRATION_PERIOD_FORMAT_IN_CONFIG = INVALID_EXPIRATION_PERIOD_FORMAT + " in a config file";

    /// <summary>
    /// About file types message.
    /// </summary>
    public const string ABOUT_FILE_TYPES = "About file types https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB";

    /// <summary>
    /// File types input message.
    /// </summary>
    public const string FILE_TYPES_INPUT = "File types (MDR by default, '*' all files): ";

    /// <summary>
    /// File extensions input message.
    /// </summary>
    public const string FILE_EXTENSIONS_INPUT = "File extensions separated by commas ('.cs' by default, '*' all files): ";

    /// <summary>
    /// Exclude specific branches example message.
    /// </summary>
    public const string EXCLUDE_BRANCHES_EXAMPLE = "Exclude specific branches separated by space (e.g., f-1 f-2).";

    /// <summary>
    /// Exclude specific branches with '*' example message.
    /// </summary>
    public const string EXCLUDE_BRANCHES_FILTER_EXAMPLE = "You can use '*' to select multiple similar branches (e.g., f-*).";

    /// <summary>
    /// Exclude specific branches default behaviour message.
    /// </summary>
    public const string EXCLUDE_BRANCHES_DEFAULT_BEHAVIOUR = "By default, no additional branches are excluded.";

    /// <summary>
    /// Exclude branches input message.
    /// </summary>
    public const string EXCLUDE_BRANCHES_INPUT = "Exclude branches: ";

    /// <summary>
    /// Include file paths input message.
    /// </summary>
    public const string INCLUDE_FILE_PATHS_INPUT = "Enter file paths (comma-separated) to process only these files, others will be skipped (default: all files): ";

    /// <summary>
    /// Start message.
    /// </summary>
    public const string START = "Start find activity...";

    /// <summary>
    /// No entries message.
    /// </summary>
    public const string NO_ENTRIES = "No entries";

    /// <summary>
    /// Activity message.
    /// </summary>
    public const string ACTIVITY = "Activity";

    /// <summary>
    /// Inactive branches message.
    /// </summary>
    public const string INACTIVE_BRANCHES = "Inactive branches";

    /// <summary>
    /// File activity via single line.
    /// </summary>
    public const string ACTIVITY_VIA_SINGLE_LINE = "All active files in single line";

    /// <summary>
    /// File activity via single line for dotnet format --exclude command.
    /// </summary>
    public const string ACTIVITY_VIA_SINGLE_LINE_FOR_EXCLUDE = "All active files in single line for `dotnet format --exclude ...` command";

    /// <summary>
    /// All files via biak variable.
    /// </summary>
    public const string ACTIVITY_VIA_VARIABLE = "All active files via variable";

    /// <summary>
    /// Origin prefix.
    /// </summary>
    public const string ORIGIN_PREFIX = "origin/";
}
