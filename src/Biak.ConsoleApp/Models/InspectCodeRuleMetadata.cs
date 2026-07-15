// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Metadata for an InspectCode rule.
/// </summary>
public sealed class InspectCodeRuleMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InspectCodeRuleMetadata"/> class.
    /// </summary>
    /// <param name="editorconfigConfigKey">.editorconfig key for the rule.</param>
    /// <param name="title">Human-readable rule title.</param>
    /// <param name="reference">Reference URL or documentation identifier.</param>
    public InspectCodeRuleMetadata(string editorconfigConfigKey, string title, string reference)
    {
        EditorconfigConfigKey = editorconfigConfigKey;
        Title = title;
        Reference = reference;
    }

    /// <summary>
    /// .editorconfig key for the rule.
    /// </summary>
    public string EditorconfigConfigKey { get; init; }

    /// <summary>
    /// Human-readable rule title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Reference URL or documentation identifier.
    /// </summary>
    public string Reference { get; init; }
}
