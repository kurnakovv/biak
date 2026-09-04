// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Result of an InspectCode baseline run.
/// </summary>
/// <param name="SarifPath">Path to the generated temporary SARIF file.</param>
/// <param name="ExecutedCommand">The command line that successfully executed InspectCode.</param>
/// <param name="PreservedSarifPath">Path to the preserved SARIF copy when debug mode is enabled.</param>
public sealed record InspectCodeBaselineRunResult(string SarifPath, string ExecutedCommand, string? PreservedSarifPath);
