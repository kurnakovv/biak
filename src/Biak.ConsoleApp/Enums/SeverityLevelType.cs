// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Enums;

/// <summary>
/// Severity level https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#severity-level docs.
/// </summary>
public enum SeverityLevelType
{
    /// <summary>
    /// Rule is suppressed completely.
    ///
    /// However, for code-style rules, Visual Studio code-generation features still generate code in this style.
    /// </summary>
    None = 1,

    /// <summary>
    /// Violations appear as build errors and cause builds to fail.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Violations appear as build warnings but do not cause builds to fail (unless you have an option set to treat warnings as errors).
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Violations appear as build messages and as suggestions in the Visual Studio IDE. (In Visual Studio, suggestions appear as three gray dots under the first two characters.)
    /// </summary>
    Suggestion = 4,

    /// <summary>
    /// Violations aren't visible to the user.
    ///
    /// However, for code-style rules, Visual Studio code-generation features still generate code in this style. These rules also participate in cleanup and appear in the Quick Actions and Refactorings menu in Visual Studio.
    /// </summary>
    Silent = 5,

    /// <summary>
    /// The default severity of the rule is used. The default severities for each .NET release are listed in the dotnet/sdk repo. In that table, "Disabled" corresponds to none, "Hidden" corresponds to silent, and "Info" corresponds to suggestion.
    /// </summary>
    Default = 6,
}
