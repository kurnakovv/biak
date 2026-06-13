// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class WarningsBaselineSyncHelperTests
{
    [Theory]
    [InlineData(".editorconfig", true)]
    [InlineData(".biak/.editorconfig-main", true)]
    [InlineData(".editorconfig-legacy", true)]
    [InlineData(".editorconfig-custom", true)]
    [InlineData("src/nested/.editorconfig", true)]
    [InlineData(".editorconfig/test.txt", false)]
    [InlineData("src/.editorconfig/.editorconfig-main", false)]
    [InlineData("test.editorconfig", false)]
    [InlineData("src/test.editorconfig", false)]
    [InlineData("../.editorconfig", false)]
    [InlineData("../../.editorconfig", false)]
    [InlineData("../other-project/.editorconfig", false)]
    [InlineData("appsettings.json", false)]
    public void IsPathSafeTest(string relativePath, bool expected)
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        bool result = WarningsBaselineSyncHelper.IsPathSafe(relativePath, baseDir);

        Assert.Equal(expected, result);
    }

    public static TheoryData<string, string[]> GetBaselineDiagnosticCodesData()
    {
        return new()
        {
            {
                @"
                    [*.cs]
                    dotnet_diagnostic.CA1001.severity = error
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
                    dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
                """,
                ["CA2000"]
            },
            {
                $$"""
                    [{src/File1.cs,src/File2.cs}]
                    dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                    [{src/File3.cs}]
                    dotnet_diagnostic.CA1001.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                    [{src/File4.cs}]
                    dotnet_diagnostic.CS1234.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
                """,
                ["CA2000", "CA1001", "CS1234"]
            },
            {
                $$"""
                    [{src/File1.cs,src/File2.cs}]
                    dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                    [{src/File3.cs}]
                    dotnet_diagnostic.CA1001.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                    [{src/File4.cs}]
                    dotnet_diagnostic.CS1234.severity = suggestion

                    [*.cs]
                    dotnet_diagnostic.CS1234.severity = suggestion
                """,
                ["CA2000", "CA1001"]
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetBaselineDiagnosticCodesData))]
    public void GetBaselineDiagnosticCodesTest(string content, string[] expectedCodes)
    {
        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Equal(expectedCodes.Length, codes.Count);
        foreach (string code in expectedCodes)
        {
            Assert.Contains(code, codes);
        }
    }

    [Theory]
    [InlineData(
        "SingleSuggestionToWarning",
        $"dotnet_diagnostic.CA2000.severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}",
        true,
        $"dotnet_diagnostic.CA2000.severity = warning {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}"
    )]
    [InlineData(
        "ActivateAllBaselineEntries",
        $$"""
        [{src/File1.cs}]
        dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{src/File2.cs}]
        dotnet_diagnostic.CA1001.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
        """,

        true,

        $$"""
        [{src/File1.cs}]
        dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{src/File2.cs}]
        dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
        """
    )]
    [InlineData(
        "ActivateDoesNotChangeNonBaselineSuggestion",
        """
        [*.cs]
        dotnet_diagnostic.CA9999.severity = suggestion
        """,

        true,

        """
        [*.cs]
        dotnet_diagnostic.CA9999.severity = suggestion
        """
    )]
    [InlineData("ActivateWithoutMarkerReturnsSameContent", "root = true", true, "root = true")]
    [InlineData(
        "SingleWarningToSuggestion",
        $"dotnet_diagnostic.CA2000.severity = warning {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}",
        false,
        $"dotnet_diagnostic.CA2000.severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}"
    )]
    [InlineData(
        "DeactivateAllBaselineEntries",
        $$"""
        [{src/File1.cs}]
        dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{src/File2.cs}]
        dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
        """,

        false,

        $$"""
        [{src/File1.cs}]
        dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [{src/File2.cs}]
        dotnet_diagnostic.CA1001.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
        """
    )]
    [InlineData(
        "DeactivateDoesNotChangeNonBaselineWarning",
        """
        [*.cs]
        dotnet_diagnostic.CA9999.severity = warning
        """,

        false,

        """
        [*.cs]
        dotnet_diagnostic.CA9999.severity = warning
        """
    )]
    public void SetBaselineForBuildUpdatesContentAsExpected(string testName, string content, bool activate, string expected)
    {
        _ = testName;
        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("InverseWithoutBaselineEntries", "root = true")]
    [InlineData(
        "InverseWithBaselineAndNonBaselineEntries",
        $$"""
        [{src/File.cs}]
        dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

        [*.cs]
        dotnet_diagnostic.CA9999.severity = warning
        """
    )]
    public void SetBaselineForBuildActivateAndDeactivateAreInverses(string testName, string original)
    {
        _ = testName;
        string roundTripped = WarningsBaselineSyncHelper.SetBaselineForBuild(
            content: WarningsBaselineSyncHelper.SetBaselineForBuild(original, activate: true),
            activate: false
        );

        Assert.Equal(original, roundTripped);
    }

    public static TheoryData<string, string, string[], bool, string[], string[], string?> RemoveBaselineFiltersCodeSelectionData()
    {
        return new()
        {
            {
                "KeepsBlockWhenCodeIsInSet",
                $$"""
                [{src/File.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000"],
                false,
                ["[{src/File.cs}]", "CA2000"],
                Array.Empty<string>(),
                null
            },
            {
                "RemovesBlockWhenCodeNotInSet",
                $$"""
                [{src/File.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                Array.Empty<string>(),
                false,
                Array.Empty<string>(),
                ["[{src/File.cs}]", "CA2000"],
                null
            },
            {
                "KeepsOnlyCodesInSet",
                $$"""
                [{src/File1.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File2.cs}]
                dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File3.cs}]
                dotnet_diagnostic.CS1234.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA1001"],
                false,
                ["CA1001", "[{src/File2.cs}]"],
                ["CA2000", "[{src/File1.cs}]", "CS1234", "[{src/File3.cs}]"],
                null
            },
            {
                "RemovesAllBlocksWhenSetIsEmpty",
                $$"""
                [{src/File1.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File2.cs}]
                dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                Array.Empty<string>(),
                false,
                Array.Empty<string>(),
                ["CA2000", "CA1001", "[{"],
                null
            },
            {
                "KeepsAllBlocksWhenAllCodesAreInSet",
                $$"""
                [{src/File1.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File2.cs}]
                dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000", "CA1001"],
                false,
                Array.Empty<string>(),
                Array.Empty<string>(),
                $$"""
                [{src/File1.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [{src/File2.cs}]
                dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """
            },
            {
                "CaseInsensitiveCodeMatching",
                $$"""
                [{src/File.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["ca2000"],
                true,
                ["CA2000"],
                Array.Empty<string>(),
                null
            },
            {
                "KeepsBlockWithSingleBlankLineBetweenHeaderAndDiagnostic",
                $$"""
                [{src/File.cs}]

                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000"],
                false,
                ["[{src/File.cs}]", "CA2000"],
                Array.Empty<string>(),
                null
            },
            {
                "RemovesBlockWithSingleBlankLineBetweenHeaderAndDiagnostic",
                $$"""
                [{src/File.cs}]

                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                Array.Empty<string>(),
                false,
                Array.Empty<string>(),
                ["[{src/File.cs}]", "CA2000"],
                null
            },
            {
                "KeepsBlockWithMultipleBlankLinesBetweenHeaderAndDiagnostic",
                $$"""
                [{src/File.cs}]



                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000"],
                false,
                ["CA2000"],
                Array.Empty<string>(),
                null
            },
            {
                "IgnoresHeaderAtEndOfFileWhenDiagnosticLineIsMissing",
                """
                [{src/File.cs}]


                """,
                Array.Empty<string>(),
                false,
                ["[{src/File.cs}]"],
                Array.Empty<string>(),
                """
                [{src/File.cs}]


                """
            },
        };
    }

    [Theory]
    [MemberData(nameof(RemoveBaselineFiltersCodeSelectionData))]
    public void RemoveBaselineFiltersCodeSelectionTheory(
        string testName,
        string content,
        string[] codesToKeep,
        bool useCaseInsensitiveSet,
        string[] mustContain,
        string[] mustNotContain,
        string? expectedExact
    )
    {
        _ = testName;
        HashSet<string> codes = new(
            codesToKeep,
            useCaseInsensitiveSet
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal
        );

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(content, codes);

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

    public static TheoryData<string, string, string[], string[], string[], string?> RemoveBaselineFiltersContentPreservationData()
    {
        return new()
        {
            {
                "PreservesNonBaselineSections",
                $$"""
                [*.cs]
                dotnet_diagnostic.CA9999.severity = error

                [{src/File.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                [*]
                indent_size = 4
                """,
                Array.Empty<string>(),
                ["[*.cs]", "CA9999", "[*]", "indent_size = 4"],
                ["CA2000", "[{src/File.cs}]"],
                null
            },
            {
                "ReturnsContentUnchangedWhenNoBaselineBlocks",
                """
                [*.cs]
                dotnet_diagnostic.CA9999.severity = error
                """,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                """
                [*.cs]
                dotnet_diagnostic.CA9999.severity = error
                """
            },
            {
                "PreservesContentBeforeAndAfterRemovedBlock",
                $$"""
                # top of file

                [{src/File.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                # bottom of file
                """,
                Array.Empty<string>(),
                ["# top of file", "# bottom of file"],
                ["CA2000"],
                null
            },
            {
                "HandlesMultipleFilesInSectionHeader",
                $$"""
                [{src/File1.cs,src/File2.cs,src/File3.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                Array.Empty<string>(),
                Array.Empty<string>(),
                ["CA2000", "[{"],
                null
            },
        };
    }

    [Theory]
    [MemberData(nameof(RemoveBaselineFiltersContentPreservationData))]
    public void RemoveBaselineFiltersContentPreservationTheory(
        string testName,
        string content,
        string[] codesToKeep,
        string[] mustContain,
        string[] mustNotContain,
        string? expectedExact
    )
    {
        _ = testName;
        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(content, new HashSet<string>(codesToKeep));

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

    public static TheoryData<string, string, string[], string[], string[], bool> RemoveBaselineFiltersLineEndingsData()
    {
        return new()
        {
            {
                "WorksWithCrLfLineEndings",
                "\r\n",
                ["CA1001"],
                ["CA1001"],
                ["CA2000"],
                true
            },
            {
                "WorksWithLfLineEndings",
                "\n",
                ["CA1001"],
                ["CA1001"],
                ["CA2000", "\r\n"],
                false
            },
        };
    }

    [Theory]
    [MemberData(nameof(RemoveBaselineFiltersLineEndingsData))]
    public void RemoveBaselineFiltersLineEndingsTheory(
        string testName,
        string newline,
        string[] codesToKeep,
        string[] mustContain,
        string[] mustNotContain,
        bool shouldContainCrLf
    )
    {
        _ = testName;
        string content = $$"""
            [{src/File1.cs}]
            dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

            [{src/File2.cs}]
            dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

            """.ReplaceLineEndings(newline);

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(content, new HashSet<string>(codesToKeep));

        foreach (string value in mustContain)
        {
            Assert.Contains(value, result, StringComparison.Ordinal);
        }

        foreach (string value in mustNotContain)
        {
            Assert.DoesNotContain(value, result, StringComparison.Ordinal);
        }

        if (shouldContainCrLf)
        {
            Assert.Contains("\r\n", result, StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain("\r\n", result, StringComparison.Ordinal);
        }
    }

    public static TheoryData<string, string, string[], string[], string[], string[]> RemoveBaselineFiltersActiveFilesPruningData()
    {
        return new()
        {
            {
                "PrunesResolvedFilesInsideKeptCodeBlock",
                $$"""
                [{src/File1.cs,src/File2.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000"],
                ["src/File2.cs"],
                ["[{src/File2.cs}]", "CA2000"],
                ["src/File1.cs"]
            },
            {
                "RemovesCodeBlockWhenNoFilesRemainAfterPruning",
                $$"""
                [{src/File1.cs}]
                dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

                """,
                ["CA2000"],
                ["src/OtherFile.cs"],
                Array.Empty<string>(),
                ["CA2000", "[{"]
            },
        };
    }

    [Theory]
    [MemberData(nameof(RemoveBaselineFiltersActiveFilesPruningData))]
    public void RemoveBaselineFiltersActiveFilesPruningTheory(
        string testName,
        string content,
        string[] codesToKeep,
        string[] activeFilesForCode,
        string[] mustContain,
        string[] mustNotContain
    )
    {
        _ = testName;
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByCode =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["CA2000"] = new HashSet<string>(activeFilesForCode, StringComparer.OrdinalIgnoreCase),
            };

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content,
            new HashSet<string>(codesToKeep, StringComparer.OrdinalIgnoreCase),
            activeFilesByCode);

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
            dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

            [{src/StillBroken.cs,src/PartiallyFixed.cs}]
            dotnet_diagnostic.CA1001.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
            """;

        HashSet<string> codesToKeep = new(StringComparer.OrdinalIgnoreCase) { "CA1001" };
        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByCode =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["CA1001"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src/StillBroken.cs" },
            };

        IReadOnlyDictionary<string, IReadOnlySet<string>> result = WarningsBaselineSyncHelper.GetSynchronizedFiles(
            content,
            codesToKeep,
            activeFilesByCode
        );

        Assert.Equal(2, result.Count);
        Assert.Contains("src/Fixed.cs", result.Keys, StringComparer.Ordinal);
        Assert.Contains("src/PartiallyFixed.cs", result.Keys, StringComparer.Ordinal);
        Assert.Equal(["CA2000"], result["src/Fixed.cs"]);
        Assert.Equal(["CA1001"], result["src/PartiallyFixed.cs"]);
        Assert.DoesNotContain("src/StillBroken.cs", result.Keys, StringComparer.Ordinal);
    }
}
