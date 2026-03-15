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
    /// Default values for <see cref="SeveritiesToDisable"/> property.
    /// </summary>
    public static readonly SeverityLevelType[] s_defaultSeveritiesToDisable =
    {
        SeverityLevelType.Error,
        SeverityLevelType.Warning,
        SeverityLevelType.Suggestion,
    };

    /// <summary>
    /// Severity level when `dotnet biak disable`.
    /// </summary>
    public SeverityLevelType SeverityWhenDisabled { get; init; } = SeverityLevelType.None;

    /// <summary>
    /// This field will allow users to specify which analyzer severities should be replaced when running the `dotnet biak disable` command.
    /// </summary>
    public IEnumerable<SeverityLevelType> SeveritiesToDisable { get; set; } = s_defaultSeveritiesToDisable;
}
