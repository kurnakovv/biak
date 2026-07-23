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
    public const string BASELINE_MARKER = "# ^biak^ inspectcode-baseline";

    /// <summary>
    /// Failed to initialize InspectCode baseline.
    /// </summary>
    public const string INIT_FAILED = "Failed to initialize InspectCode baseline.";

    /// <summary>
    /// Header for the list of rules not found in the built-in mapping.
    /// </summary>
    public const string RULES_NOT_MAPPED_WARNING_HEADER = "Warning: these rules were not found in the built-in InspectCode rule mapping:";

    /// <summary>
    /// Suggestion to open a GitHub issue for an unmapped rule.
    /// </summary>
    public const string RULE_NOT_MAPPED_OPEN_ISSUE = "Please open a GitHub issue at https://github.com/kurnakovv/biak/issues so we can add it.";

    /// <summary>
    /// Local workaround hint for unmapped rules, including examples for two rules.
    /// </summary>
    public const string RULE_NOT_MAPPED_LOCAL_WORKAROUND =
        "As a local workaround add it to your .biak/config.json:\n" +
        "  \"inspectCodeBaseline\": {\n" +
        "    \"ruleIdOverrides\": {\n" +
        "      \"RuleId1\": \"resharper_your_rule1_editorconfig_key_highlighting\",\n" +
        "      \"RuleId2\": \"resharper_your_rule2_editorconfig_key_highlighting\"\n" +
        "    }\n" +
        "  }";
}
