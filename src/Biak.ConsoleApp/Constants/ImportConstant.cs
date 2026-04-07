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
    /// Forbidden outside .biak folder.
    /// </summary>
    public const string FORBIDDEN_OUTSIDE = $"{WARNING} It is forbidden to go beyond the .biak folder:";

    /// <summary>
    /// File not found.
    /// </summary>
    public const string FILE_NOT_FOUND = $"{WARNING} Import file not found:";

    private const string WARNING = "⚠️ Warning:";

    /////// <summary>
    /////// File not found text.
    /////// </summary>
    /////// <param name="path">file path.</param>
    /////// <returns>const text.</returns>
    ////public static string GetFileNotFound(string path)
    ////{
    ////    return $"{WARNING} Import file not found: {path}";
    ////}
}
