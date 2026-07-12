// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

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
    /// <param name="editorconfigMainPath">Path to .biak/.editorconfig-main.</param>
    /// <param name="config">Biak config.</param>
    /// <returns>Enabled .editorconfig content.</returns>
    public static async Task<string> GetEnabledContentAsync(string editorconfigMainPath, BiakConfig config)
    {
        string content = await File.ReadAllTextAsync(editorconfigMainPath);
        content = await ImportHelper.ReplaceAsync(content, config.OnImportFailure);
        content = IncludeExcludeFilterHelper.Apply(content);
        content = VariableHelper.Substitute(content);
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
}
