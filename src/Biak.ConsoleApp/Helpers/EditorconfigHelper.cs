// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// .editorconfig helper.
/// </summary>
public static class EditorconfigHelper
{
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
