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
    private static readonly SeverityLevelType[] s_defaultSeveritiesToDisable =
    {
        SeverityLevelType.Error,
        SeverityLevelType.Warning,
        SeverityLevelType.Suggestion,
    };

    /// <summary>
    /// Default values for <see cref="SeveritiesToDisable"/> property.
    /// </summary>
    public static IReadOnlyList<SeverityLevelType> DefaultSeveritiesToDisable { get; } =
        Array.AsReadOnly(s_defaultSeveritiesToDisable);

    /// <summary>
    /// Severity level when `dotnet biak disable`.
    /// </summary>
    public SeverityLevelType SeverityWhenDisabled { get; init; } = SeverityLevelType.None;

    /// <summary>
    /// This field will allow users to specify which analyzer severities should be replaced when running the `dotnet biak disable` command.
    /// </summary>
    public IEnumerable<SeverityLevelType>? SeveritiesToDisable { get; set; } = DefaultSeveritiesToDisable;

    /// <summary>
    /// Controls what happens when an import cannot be resolved, for example because a file is missing or a URL is blocked or unreachable. Default is Warning.
    /// </summary>
    public FailureBehaviorType OnImportFailure { get; init; } = FailureBehaviorType.Warning;

    /// <summary>
    /// dotnet biak find-activity settings.
    /// </summary>
    public FindActivityInputConfigModel? FindActivity { get; init; }
}

/// <summary>
/// Find activity config settings model.
/// </summary>
public class FindActivityInputConfigModel
{
    /// <summary>
    /// Default git branch.
    /// </summary>
    public string? DefaultBranch { get; init; }

    /// <summary>
    /// Expiration period in days.
    /// </summary>
    public string? ExpirationPeriod { get; init; }

    /// <summary>
    /// Git file types.
    /// </summary>
    public string? FileTypes { get; init; }

    /// <summary>
    /// File extensions by commas.
    /// </summary>
    public string? FileExtensions { get; init; }

    /// <summary>
    /// Exclude branches.
    /// </summary>
    public string? ExcludeBranches { get; init; }

    /// <summary>
    /// Include only this file paths.
    /// </summary>
    public string? IncludedFilePaths { get; init; }

    /// <summary>
    /// Save output to file log.
    /// </summary>
    public bool? SaveOutput { get; init; }
}
