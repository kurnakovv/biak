// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Enums;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Include / Exclude filter helper.
/// </summary>
public static class IncludeExcludeFilterHelper
{
    private static readonly Regex s_blockRegex = new(
        @"\^biak\^\s*include\s*(\[[^\]]+\])\s*" +
        @"\^biak\^\s*exclude\s*(\[[^\]]+\])\s*" +
        "(.*?)" +
        @"\^biak\^\s*END include/exclude",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    /// <summary>
    /// Apply rules to all C# [*.cs] files except selected ones (e.g., [{TestClass1.cs,TestClass2.cs}]).
    /// <code>
    ///     ^biak^ include [File paths to include]
    ///     ^biak^ exclude[File paths to exclude]
    ///
    ///     ... rules to include / exclude
    ///
    ///     ^ biak ^ END include/exclude
    ///
    ///
    ///     [File paths to include]
    ///     ... included rules
    ///
    ///     [File paths to exclude]
    ///     ... exclude rules
    /// </code>
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <returns>Content with applied filters.</returns>
    public static string Apply(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        return s_blockRegex.Replace(
            content,
            match =>
            {
                string includePath = match.Groups[1].Value.Trim();
                string excludePath = match.Groups[2].Value.Trim();
                string rulesBlock = match.Groups[3].Value.Trim('\r', '\n');

                StringBuilder sb = new();

                sb.AppendLine(includePath);
                sb.AppendLine(rulesBlock);

                sb.AppendLine();

                sb.AppendLine(excludePath);
                sb.AppendLine(
                    SeverityHelper.Disable(
                        content: rulesBlock,
                        severitiesToDisable: Enum.GetValues<SeverityLevelType>(),
                        severityWhenDisabled: SeverityLevelType.None
                    )
                );

                return sb.ToString().TrimEnd();
            }
        );
    }
}
