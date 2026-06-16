// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Helper methods for validating and normalizing user-provided file paths.
/// </summary>
public static class PathSafetyHelper
{
    /// <summary>
    /// Resolves <paramref name="filePath"/> against <paramref name="baseDirectory"/> and checks
    /// that the resulting path stays within the base directory.
    /// </summary>
    /// <param name="filePath">User-provided file path.</param>
    /// <param name="baseDirectory">Allowed base directory.</param>
    /// <param name="fullFilePath">Resolved absolute file path.</param>
    /// <param name="relativePath">Resolved path relative to base directory.</param>
    /// <returns><see langword="true"/> when the path is inside base directory; otherwise <see langword="false"/>.</returns>
    public static bool TryResolvePathWithinBaseDirectory(
        string filePath,
        string baseDirectory,
        out string fullFilePath,
        out string relativePath)
    {
        string fullBasePath = Path.GetFullPath(baseDirectory);
        fullFilePath = Path.GetFullPath(filePath, fullBasePath);

        relativePath = Path.GetRelativePath(fullBasePath, fullFilePath);
        return !Path.IsPathRooted(relativePath)
            && !relativePath.Equals("..", StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
    }
}
