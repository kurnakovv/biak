// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak find-conflicts` command.
/// </summary>
public static class FindConflictsCommandConstant
{
    /// <summary>
    /// Branches input message.
    /// </summary>
    public const string BRANCHES_INPUT = "Branches separated by spaces that will be merged with no commit into the default branch (e.g., f-1 f-2): ";

    /// <summary>
    /// Invalid branches format message.
    /// </summary>
    public const string INVALID_BRANCHES_FORMAT = "Invalid branches format";

    /// <summary>
    /// Start message.
    /// </summary>
    public const string START = "Start find conflicts command...";

    /// <summary>
    /// Conflicting files message.
    /// </summary>
    public const string CONFLICTING_FILES = "Conflicting files";

    /// <summary>
    /// Not found branches message.
    /// </summary>
    public const string NOT_FOUND_BRANCHES = "Not found branches";
}
