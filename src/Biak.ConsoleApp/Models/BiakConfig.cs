// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Enums;

namespace Biak.ConsoleApp.Models;

/// <summary>
/// .biak/config.json model.
/// </summary>
public class BiakConfig
{
    /// <summary>
    /// Severity level when `dotnet biak disable`.
    /// </summary>
    public SeverityLevelType SeverityWhenDisabled { get; init; } = SeverityLevelType.None;
}
