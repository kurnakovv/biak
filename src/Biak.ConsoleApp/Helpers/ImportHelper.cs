// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// ^biak^ import "..." helper.
/// </summary>
public static class ImportHelper
{
    private static readonly Regex s_importRegex = new(
        @"\^biak\^\s*import\s*""([^""]+)""",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Replace imports with file content.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <returns>Replaced content with imports.</returns>
    public static async Task<string> ReplaceAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        MatchCollection matches = s_importRegex.Matches(content);
        if (matches.Count == 0)
        {
            return content;
        }

        string biakDir = Path.Join(Directory.GetCurrentDirectory(), ".biak");
        string biakFullPath = Path.GetFullPath(biakDir);

        StringBuilder sb = new(content);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];
            string path = match.Groups[1].Value;

            string? replacement = await ResolveImportAsync(path, biakFullPath);

            if (replacement != null)
            {
                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
            }
        }

        return sb.ToString();
    }

    private static async Task<string?> ResolveImportAsync(string path, string biakFullPath)
    {
        string fullPath = Path.GetFullPath(
            Path.Combine(biakFullPath, path)
        );

        if (!fullPath.StartsWith(biakFullPath, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{ImportConstant.FORBIDDEN_OUTSIDE} {path}");
            return null;
        }

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"{ImportConstant.FILE_NOT_FOUND} {path}");
            return null;
        }

        return await File.ReadAllTextAsync(fullPath);
    }
}
