// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// ^biak^ import "..." constants.
/// </summary>
public static class ImportConstant
{
    /// <summary>
    /// Unable to retrieve content from link.
    /// </summary>
    public const string UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK = $"{WARNING} Unable to retrieve content from link";

    /// <summary>
    /// Forbidden outside .biak folder.
    /// </summary>
    public const string FORBIDDEN_OUTSIDE = $"{WARNING} It is forbidden to go beyond the .biak folder:";

    /// <summary>
    /// File not found.
    /// </summary>
    public const string FILE_NOT_FOUND = $"{WARNING} Import file not found:";

    private const string WARNING = "⚠️ Warning:";
}
