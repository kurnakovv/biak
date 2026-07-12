// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// .editorconfig helper.
/// </summary>
public static class EditorconfigHelper
{
    /// <summary>
    /// Gets enabled .editorconfig content generated from .biak/.editorconfig-main.
    /// </summary>
    /// <param name="content">.editorconfig-main content.</param>
    /// <param name="config">Biak config.</param>
    /// <returns>Enabled .editorconfig content.</returns>
    public static async Task<string> GetEnabledContentAsync(string content, BiakConfig config)
    {
        content = await ImportHelper.ReplaceAsync(content, config.OnImportFailure);
        content = IncludeExcludeFilterHelper.Apply(content);
        content = VariableHelper.Substitute(content);
        content = RemoveAlwaysEnabledMarkers(content);
        return AddAttentionBanners(content);
    }

    /// <summary>
    /// Gets disabled .editorconfig content generated from .biak/.editorconfig-main.
    /// </summary>
    /// <param name="content">.editorconfig-main content.</param>
    /// <param name="config">Biak config.</param>
    /// <returns>Disabled .editorconfig content.</returns>
    public static async Task<string> GetDisabledContentAsync(string content, BiakConfig config)
    {
        content = await ImportHelper.ReplaceAsync(content, config.OnImportFailure);
        content = IncludeExcludeFilterHelper.Apply(content);
        content = SeverityHelper.Disable(
            content: content,
            severitiesToDisable: config.SeveritiesToDisable ?? BiakConfig.DefaultSeveritiesToDisable,
            severityWhenDisabled: config.SeverityWhenDisabled
        );
        content = VariableHelper.Substitute(content);
        content = RemoveAlwaysEnabledMarkers(content);
        return AddAttentionBanners(content);
    }

    /// <summary>
    /// Add attention banners to up and bottom with LF/CRLF support.
    /// </summary>
    /// <param name="content">File content.</param>
    /// <returns>Updated file content.</returns>
    public static string AddAttentionBanners(string content)
    {
        string newline = content.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";

        string up = EditorconfigConstant.UP_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);
        string bottom = EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);

        return up + content + bottom;
    }

    internal static string RemoveAlwaysEnabledMarkers(string content)
    {
        return Regex.Replace(
            content,
            @"^[ \t]*\^biak\^[ \t]*always-enabled\s+(start|end)[ \t]*\r?\n?",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );
    }
}
