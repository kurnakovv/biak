// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// `dotnet biak inspectcode-baseline init` command constants.
/// </summary>
public static class InspectCodeBaselineInitCommandConstant
{
    /// <summary>
    /// InspectCode baseline initialization started.
    /// </summary>
    public const string INIT_STARTED = "inspectcode-baseline init started...";

    /// <summary>
    /// No issues found in the InspectCode report.
    /// </summary>
    public const string NO_ISSUES_FOUND = "No InspectCode issues found. Nothing to generate.";

    /// <summary>
    /// Insert baseline filters into .editorconfig note.
    /// </summary>
    public const string INSERT_FILTERS_NOTE = "Insert these filters into your .editorconfig file:";

    /// <summary>
    /// Baseline marker appended to every generated editorconfig entry.
    /// </summary>
    public const string BASELINE_MARKER = "# ^biak^ baseline";

    /// <summary>
    /// Failed to initialize InspectCode baseline.
    /// </summary>
    public const string INIT_FAILED = "Failed to initialize InspectCode baseline.";

    /// <summary>
    /// Warning message prefix for rules not found in the built-in mapping (before the rule id).
    /// </summary>
    public const string RULE_NOT_MAPPED_WARNING_PREFIX = "Warning: rule '";

    /// <summary>
    /// Warning message suffix for rules not found in the built-in mapping (after the rule id).
    /// </summary>
    public const string RULE_NOT_MAPPED_WARNING_SUFFIX = "' was not found in the built-in InspectCode rule mapping.";

    /// <summary>
    /// Suggestion to open a GitHub issue for an unmapped rule.
    /// </summary>
    public const string RULE_NOT_MAPPED_OPEN_ISSUE = "Please open a GitHub issue at https://github.com/kurnakovv/biak/issues so we can add it.";

    /// <summary>
    /// Local workaround hint for an unmapped rule — first part (before the rule id).
    /// </summary>
    public const string RULE_NOT_MAPPED_LOCAL_WORKAROUND_PREFIX =
        "As a local workaround add it to your .biak/config.json:\n" +
        "  \"inspectCodeBaseline\": {{\n" +
        "    \"ruleIdOverrides\": {{\n" +
        "      \"";

    /// <summary>
    /// Local workaround hint for an unmapped rule — second part (after the rule id).
    /// </summary>
    public const string RULE_NOT_MAPPED_LOCAL_WORKAROUND_SUFFIX =
        "\": \"resharper_your_rule_editorconfig_key_highlighting\"\n" +
        "    }}\n" +
        "  }}";
}
