// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Enums;

/// <summary>
/// Failure behavior type.
/// </summary>
public enum FailureBehaviorType
{
    /// <summary>
    /// Do nothing, no logs, no exit code.
    /// </summary>
    Nothing = 1,

    /// <summary>
    /// Write warning message, but without exit code.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error message with exit code.
    /// </summary>
    Error = 3,
}
