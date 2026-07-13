// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Biak.ConsoleApp.Helpers;

internal static class AlwaysEnabledRulesHelper
{
    private const string ALWAYS_ENABLED_PREFIX = "__biak_always_enabled_severity_";

    private static readonly Regex s_alwaysEnabledStartRegex = new(
        @"^[ \t]*\^biak\^[ \t]*always-enabled\s+start[ \t]*\r?$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    private static readonly Regex s_alwaysEnabledEndRegex = new(
        @"^[ \t]*\^biak\^[ \t]*always-enabled\s+end[ \t]*\r?$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    private static readonly Regex s_severityValueRegex = new(
        @"=\s*(none|silent|suggestion|warning|error)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    internal static string ProtectSeverities(string content, out Dictionary<string, string> placeholders)
    {
        Dictionary<string, string> localPlaceholders = new();

        if (string.IsNullOrEmpty(content))
        {
            placeholders = localPlaceholders;
            return content;
        }

        int markerIndex = 0;
        int searchStart = 0;

        while (true)
        {
            Match startMatch = s_alwaysEnabledStartRegex.Match(content, searchStart);
            if (!startMatch.Success)
            {
                break;
            }

            Match endMatch = s_alwaysEnabledEndRegex.Match(content, startMatch.Index + startMatch.Length);
            if (!endMatch.Success)
            {
                break;
            }

            int blockStart = startMatch.Index + startMatch.Length;
            int blockLength = endMatch.Index - blockStart;
            if (blockLength > 0)
            {
                string blockContent = content.Substring(blockStart, blockLength);
                string updatedBlock = s_severityValueRegex.Replace(
                    blockContent,
                    m =>
                    {
                        string placeholder = ALWAYS_ENABLED_PREFIX + markerIndex++;
                        localPlaceholders[placeholder] = m.Groups[1].Value;
                        return m.Value.Replace(m.Groups[1].Value, placeholder, StringComparison.Ordinal);
                    }
                );

                content = string.Concat(content.AsSpan(0, blockStart), updatedBlock.AsSpan(), content.AsSpan(endMatch.Index));
                searchStart = blockStart + updatedBlock.Length + endMatch.Length;
            }
            else
            {
                searchStart = endMatch.Index + endMatch.Length;
            }
        }

        placeholders = localPlaceholders;
        return content;
    }
}
