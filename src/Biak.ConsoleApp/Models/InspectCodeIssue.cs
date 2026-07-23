// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// A single InspectCode diagnostic issue.
/// </summary>
public sealed class InspectCodeIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InspectCodeIssue"/> class.
    /// </summary>
    /// <param name="ruleId">InspectCode rule identifier.</param>
    /// <param name="filePath">Relative file path where the issue was found.</param>
    public InspectCodeIssue(string ruleId, string filePath)
    {
        RuleId = ruleId;
        FilePath = filePath;
    }

    /// <summary>
    /// InspectCode rule identifier (e.g. <c>ConvertToConstant.Local</c>).
    /// </summary>
    public string RuleId { get; }

    /// <summary>
    /// Relative file path where the issue was found.
    /// </summary>
    public string FilePath { get; }
}
