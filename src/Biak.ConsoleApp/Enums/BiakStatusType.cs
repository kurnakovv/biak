// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Enums;

/// <summary>
/// Biak status values.
/// </summary>
public enum BiakStatusType
{
    /// <summary>
    /// Status cannot be determined because setup/config/editorconfig is broken.
    /// </summary>
    Broken = 0,

    /// <summary>
    /// .editorconfig reflects enabled rules from .biak/.editorconfig-main.
    /// </summary>
    Enabled = 1,

    /// <summary>
    /// .editorconfig reflects disabled rules from .biak/.editorconfig-main.
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// .editorconfig is present but does not match enabled or disabled generated content.
    /// </summary>
    Unsynchronised = 3,
}
