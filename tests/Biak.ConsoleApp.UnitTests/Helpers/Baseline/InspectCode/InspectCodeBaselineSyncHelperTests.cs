// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers.Baseline.InspectCode;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.UnitTests.Helpers.Baseline.InspectCode;

public class InspectCodeBaselineSyncHelperTests
{
    public static TheoryData<string, string[]> GetRuleKeysData()
    {
        return new()
        {
            {
                @"
                    [*.cs]
                    resharper_field_can_be_made_read_only_local_highlighting = error
                ",
                Array.Empty<string>()
            },
            {
                string.Empty,
                Array.Empty<string>()
            },
            {
                $$"""
                    [{src/File.cs}]
                    resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                ["resharper_field_can_be_made_read_only_local_highlighting"]
            },
            {
                $$"""
                    [{src/File1.cs}]
                    resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                    [{src/File2.cs}]
                    resharper_replace_with_string_is_null_or_empty_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                    [{src/File3.cs}]
                    resharper_unused_member_local_highlighting = warning
                """,
                [
                    "resharper_field_can_be_made_read_only_local_highlighting",
                    "resharper_replace_with_string_is_null_or_empty_highlighting"
                ]
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetRuleKeysData))]
    public void GetRuleKeysReturnsExpectedResults(string content, string[] expectedRuleKeys)
    {
        HashSet<string> ruleKeys = InspectCodeBaselineSyncHelper.GetRuleKeys(content);

        Assert.Equal(expectedRuleKeys.Length, ruleKeys.Count);
        foreach (string ruleKey in expectedRuleKeys)
        {
            Assert.Contains(ruleKey, ruleKeys);
        }
    }

    public static TheoryData<string, string, string> PrepareRuntimeContentForAnalysisData()
    {
        return new()
        {
            {
                "SingleBaselineEntryBecomesErrorWithoutMarker",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                """
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = error
                """
            },
            {
                "MultipleBaselineEntriesBecomeError",
                $$"""
                [{src/File1.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = none {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                [{src/File2.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                """
                [{src/File1.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = error

                [{src/File2.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting = error
                """
            },
            {
                "DoesNotChangeNonBaselineEntries",
                """
                [*.cs]
                resharper_unused_member_local_highlighting = warning
                """,
                """
                [*.cs]
                resharper_unused_member_local_highlighting = warning
                """
            },
        };
    }

    [Theory]
    [MemberData(nameof(PrepareRuntimeContentForAnalysisData))]
    public void PrepareRuntimeContentForAnalysisUpdatesContentAsExpected(string testName, string content, string expected)
    {
        _ = testName;
        string result = InspectCodeBaselineSyncHelper.PrepareRuntimeContentForAnalysis(content);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PrepareRuntimeContentForAnalysisPreservesLfLineEndings()
    {
        string content = $$"""
            [{src/File1.cs}]
            resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

            [{src/File2.cs}]
            resharper_replace_with_string_is_null_or_empty_highlighting = none {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
            """.ReplaceLineEndings("\n");

        string result = InspectCodeBaselineSyncHelper.PrepareRuntimeContentForAnalysis(content);

        Assert.Contains("\n", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\r\n", result, StringComparison.Ordinal);
        Assert.Contains("= error", result, StringComparison.Ordinal);
        Assert.DoesNotContain(InspectCodeBaselineInitCommandConstant.BASELINE_MARKER, result, StringComparison.Ordinal);
    }

    public static TheoryData<string, string, string, string> NormalizeSeverityData()
    {
        return new()
        {
            {
                "AppliesConfiguredSeverity",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                "none",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = none {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            },
            {
                "UsesDefaultSeverityForWhitespaceInput",
                $$"""
                [{src/File.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                "  ",
                $$"""
                [{src/File.cs}]
                resharper_replace_with_string_is_null_or_empty_highlighting = {{InspectCodeBaselineConfig.DEFAULT_SNAPSHOT_SEVERITY}} {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            },
            {
                "TrimsConfiguredSeverity",
                $$"""
                [{src/File.cs}]
                resharper_unused_member_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """,
                "  warning  ",
                $$"""
                [{src/File.cs}]
                resharper_unused_member_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                """
            },
            {
                "DoesNotChangeNonBaselineEntries",
                """
                [*.cs]
                resharper_unused_member_local_highlighting = warning
                """,
                "none",
                """
                [*.cs]
                resharper_unused_member_local_highlighting = warning
                """
            },
        };
    }

    [Theory]
    [MemberData(nameof(NormalizeSeverityData))]
    public void NormalizeSeverityUpdatesContentAsExpected(string testName, string content, string snapshotSeverity, string expected)
    {
        _ = testName;
        string result = InspectCodeBaselineSyncHelper.NormalizeSeverity(content, snapshotSeverity);

        Assert.Equal(expected, result);
    }

    public static TheoryData<string, string, string[], string[], string[], string[], string?> RemoveFiltersData()
    {
        return new()
        {
            {
                "KeepsBlockWhenRuleKeyIsInSet",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                """,
                ["resharper_field_can_be_made_read_only_local_highlighting"],
                ["src/File.cs"],
                ["[{src/File.cs}]", "resharper_field_can_be_made_read_only_local_highlighting"],
                Array.Empty<string>(),
                null
            },
            {
                "RemovesBlockWhenRuleKeyNotInSet",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                """,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                ["[{src/File.cs}]", "resharper_field_can_be_made_read_only_local_highlighting"],
                null
            },
            {
                "PrunesResolvedFilesInsideKeptRuleBlock",
                $$"""
                [{src/File1.cs,src/File2.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                """,
                ["resharper_field_can_be_made_read_only_local_highlighting"],
                ["src/File2.cs"],
                ["[{src/File2.cs}]", "resharper_field_can_be_made_read_only_local_highlighting"],
                ["src/File1.cs,src/File2.cs", "[{src/File1.cs}]"],
                null
            },
            {
                "RemovesBlockWhenNoFilesRemainAfterPruning",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = warning {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

                """,
                ["resharper_field_can_be_made_read_only_local_highlighting"],
                ["src/OtherFile.cs"],
                Array.Empty<string>(),
                ["[{src/File.cs}]", "resharper_field_can_be_made_read_only_local_highlighting"],
                null
            },
            {
                "PreservesManualSectionWithExtraPropertiesAfterDiagnostic",
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                resharper_field_can_be_made_read_only_local_highlighting.style = strict
                """,
                Array.Empty<string>(),
                Array.Empty<string>(),
                ["[{src/File.cs}]", "style = strict"],
                Array.Empty<string>(),
                $$"""
                [{src/File.cs}]
                resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
                resharper_field_can_be_made_read_only_local_highlighting.style = strict
                """
            },
        };
    }

    [Theory]
    [MemberData(nameof(RemoveFiltersData))]
    public void RemoveFiltersUpdatesContentAsExpected(
        string testName,
        string content,
        string[] ruleKeysToKeep,
        string[] activeFilesForRuleKey,
        string[] mustContain,
        string[] mustNotContain,
        string? expectedExact)
    {
        _ = testName;
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByRuleKey =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["resharper_field_can_be_made_read_only_local_highlighting"] = new HashSet<string>(
                    activeFilesForRuleKey,
                    StringComparer.OrdinalIgnoreCase
                ),
            };

        string result = InspectCodeBaselineSyncHelper.RemoveFilters(
            content,
            new HashSet<string>(ruleKeysToKeep, StringComparer.OrdinalIgnoreCase),
            activeFilesByRuleKey);

        if (expectedExact is not null)
        {
            Assert.Equal(expectedExact, result);
        }

        foreach (string value in mustContain)
        {
            Assert.Contains(value, result, StringComparison.Ordinal);
        }

        foreach (string value in mustNotContain)
        {
            Assert.DoesNotContain(value, result, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void GetSynchronizedFilesReturnsRemovedAndPrunedFilesGroupedByFile()
    {
        string content = $$"""
            [{src/Fixed.cs}]
            resharper_field_can_be_made_read_only_local_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}

            [{src/StillBroken.cs,src/PartiallyFixed.cs}]
            resharper_replace_with_string_is_null_or_empty_highlighting = suggestion {{InspectCodeBaselineInitCommandConstant.BASELINE_MARKER}}
            """;

        HashSet<string> ruleKeysToKeep = new(StringComparer.OrdinalIgnoreCase)
        {
            "resharper_replace_with_string_is_null_or_empty_highlighting",
        };
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByRuleKey =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["resharper_replace_with_string_is_null_or_empty_highlighting"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "src/StillBroken.cs",
                },
            };

        IReadOnlyDictionary<string, IReadOnlySet<string>> result = InspectCodeBaselineSyncHelper.GetSynchronizedFiles(
            content,
            ruleKeysToKeep,
            activeFilesByRuleKey);

        Assert.Equal(2, result.Count);
        Assert.Contains("src/Fixed.cs", result.Keys, StringComparer.Ordinal);
        Assert.Contains("src/PartiallyFixed.cs", result.Keys, StringComparer.Ordinal);
        Assert.Equal(["resharper_field_can_be_made_read_only_local_highlighting"], result["src/Fixed.cs"]);
        Assert.Equal(["resharper_replace_with_string_is_null_or_empty_highlighting"], result["src/PartiallyFixed.cs"]);
        Assert.DoesNotContain("src/StillBroken.cs", result.Keys);
    }
}
