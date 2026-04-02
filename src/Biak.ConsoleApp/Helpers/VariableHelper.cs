// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.RegularExpressions;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Provides helper methods for working with biak variables.
/// </summary>
public static class VariableHelper
{
    /// <summary>
    /// Substitutes variable references.
    /// <code>
    ///     ^biak^ var excludedFiles = "TestFile1.cs,TestFile2.cs";
    ///
    ///     [{$excludedFiles}] -> [{TestFile1.cs,TestFile2.cs}]
    /// </code>
    /// </summary>
    /// <param name="content">Content with variables.</param>
    /// <returns>Content with substituted variables.</returns>
    public static string Substitute(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        Dictionary<string, string> variables = new();

        Regex varRegex = new(
            @"^[ \t]*\^biak\^\s*var\s+(?<name>\w+)\s*=\s*(?<value>.*?);\r?\n?",
            RegexOptions.Singleline | RegexOptions.Multiline
        );

        foreach (Match match in varRegex.Matches(content))
        {
            string name = match.Groups["name"].Value;
            string rawValue = match.Groups["value"].Value;

            variables[name] = ConcatValue(rawValue);
        }

        content = varRegex.Replace(content, string.Empty);

        foreach (KeyValuePair<string, string> kv in variables)
        {
            content = Regex.Replace(content, $@"\${kv.Key}\b", kv.Value);
        }

        return content;
    }

    private static string ConcatValue(string raw)
    {
        string[] parts = raw.Split('+', StringSplitOptions.RemoveEmptyEntries);

        StringBuilder sb = new();

        foreach (string part in parts.Select(x => x.Trim()))
        {
            string trimmed = part;

            if (trimmed.StartsWith('\"') && trimmed.EndsWith('\"'))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }

            sb.Append(trimmed);
        }

        return sb.ToString();
    }
}
