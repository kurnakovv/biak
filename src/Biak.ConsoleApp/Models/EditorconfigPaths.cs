// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Get .editorconfig paths model.
/// </summary>
public class EditorconfigPaths
{
    /// <summary>
    /// .editorconfig path.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// .editorconfig-main path.
    /// </summary>
    public string? MainValue { get; set; }
}
