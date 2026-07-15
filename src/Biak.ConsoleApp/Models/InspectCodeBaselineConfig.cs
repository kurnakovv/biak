// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// <c>inspectCodeBaseline</c> section of <c>.biak/config.json</c>.
/// </summary>
public sealed class InspectCodeBaselineConfig
{
    /// <summary>
    /// Default snapshot severity applied to baseline entries.
    /// </summary>
    public const string DEFAULT_SNAPSHOT_SEVERITY = "suggestion";

    /// <summary>
    /// Explicit path to the <c>.slnx</c>, <c>.sln</c>, or <c>.csproj</c> file passed to <c>jb inspectcode</c>.
    /// When set, auto-discovery of the solution/project file is skipped.
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Target baseline file path.
    /// Precedence: <c>--path</c> CLI flag → this value → default discovery logic.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Fixed severity written to baseline entries during <c>init</c> / <c>sync</c>.
    /// Defaults to <c>suggestion</c>.
    /// </summary>
    public string SnapshotSeverity { get; init; } = DEFAULT_SNAPSHOT_SEVERITY;

    /// <summary>
    /// Custom mapping of biak RuleId to InspectCode editorconfig key.
    /// Overrides built-in mappings from <c>InspectCodeRuleMetadataHelper</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string>? RuleIdOverrides { get; init; }

    /// <summary>
    /// Raw arguments forwarded to <c>jb inspectcode</c> unchanged and in order.
    /// </summary>
    public IReadOnlyList<string>? AdditionalArgs { get; init; }
}
