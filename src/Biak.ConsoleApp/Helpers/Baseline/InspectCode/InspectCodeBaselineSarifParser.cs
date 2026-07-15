// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers.Baseline.InspectCode;

/// <summary>
/// Parses a SARIF report produced by <c>jb inspectcode</c> into <see cref="InspectCodeIssue"/> instances.
/// </summary>
public static class InspectCodeBaselineSarifParser
{
    /// <summary>
    /// Parses a SARIF JSON string and returns all issues found.
    /// </summary>
    /// <param name="sarifJson">Raw SARIF JSON content.</param>
    /// <returns>Parsed issues. Empty when no results are present.</returns>
    public static IReadOnlyList<InspectCodeIssue> Parse(string sarifJson)
    {
        if (string.IsNullOrWhiteSpace(sarifJson))
        {
            return new List<InspectCodeIssue>();
        }

        using JsonDocument doc = JsonDocument.Parse(
            sarifJson,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            }
        );

        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("runs", out JsonElement runs) || runs.ValueKind != JsonValueKind.Array)
        {
            return new List<InspectCodeIssue>();
        }

        if (runs.GetArrayLength() == 0)
        {
            return new List<InspectCodeIssue>();
        }

        JsonElement firstRun = runs[0];

        if (!firstRun.TryGetProperty("results", out JsonElement results) || results.ValueKind != JsonValueKind.Array)
        {
            return new List<InspectCodeIssue>();
        }

        List<InspectCodeIssue> issues = new();

        foreach (JsonElement result in results.EnumerateArray())
        {
            if (!result.TryGetProperty("ruleId", out JsonElement ruleIdElement))
            {
                continue;
            }

            string? ruleId = ruleIdElement.GetString();
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                continue;
            }

            if (!result.TryGetProperty("locations", out JsonElement locations) || locations.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (string? filePath in locations.EnumerateArray().Select(TryGetFilePath))
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                issues.Add(new InspectCodeIssue(ruleId, filePath));
            }
        }

        return issues;
    }

    private static string? TryGetFilePath(JsonElement location)
    {
        if (!location.TryGetProperty("physicalLocation", out JsonElement physicalLocation))
        {
            return null;
        }

        if (!physicalLocation.TryGetProperty("artifactLocation", out JsonElement artifactLocation))
        {
            return null;
        }

        if (!artifactLocation.TryGetProperty("uri", out JsonElement uri))
        {
            return null;
        }

        return uri.GetString();
    }
}
