// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Helpers.Baseline;

/// <summary>
/// Shared helper for safe baseline file path validation.
/// </summary>
public static class BaselinePathHelper
{
    private static readonly char[] s_pathSeparators = new[] { '/', '\\' };

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="filePath"/> resolves to a location
    /// inside <paramref name="baseDirectory"/> and points to a file whose name starts with <c>.editorconfig</c>.
    /// </summary>
    /// <param name="filePath">Path to validate.</param>
    /// <param name="baseDirectory">Base directory that the path must stay within.</param>
    /// <returns><see langword="true"/> when the path is safe; otherwise <see langword="false"/>.</returns>
    public static bool IsSafe(string filePath, string baseDirectory)
    {
        if (!PathSafetyHelper.TryResolvePathWithinBaseDirectory(filePath, baseDirectory, out string fullFile, out string relativePath))
        {
            return false;
        }

        string fileName = Path.GetFileName(fullFile);
        if (!fileName.StartsWith(".editorconfig", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string[] segments = relativePath.Split(s_pathSeparators, StringSplitOptions.RemoveEmptyEntries);

        return !segments
            .Take(Math.Max(segments.Length - 1, 0))
            .Contains(".editorconfig", StringComparer.OrdinalIgnoreCase);
    }
}
