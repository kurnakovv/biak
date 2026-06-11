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

    // -------------------------------------------------------------------------
    // RemoveBaselineFilters
    // -------------------------------------------------------------------------

    [Fact]
    public void RemoveBaselineFilters_KeepsBlockWhenCodeIsInSet()
    {
        string content = $$"""
[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA2000" });

        Assert.Contains("[{src/File.cs}]", result, StringComparison.Ordinal);
        Assert.Contains("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_RemovesBlockWhenCodeNotInSet()
    {
        string content = $$"""
[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("[{src/File.cs}]", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_KeepsOnlyCodesInSet()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File3.cs}]
dotnet_diagnostic.CS1234.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA1001" });

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{src/File1.cs}]", result, StringComparison.Ordinal);
        Assert.Contains("CA1001", result, StringComparison.Ordinal);
        Assert.Contains("[{src/File2.cs}]", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CS1234", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{src/File3.cs}]", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_RemovesAllBlocksWhenSetIsEmpty()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA1001", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_PreservesNonBaselineSections()
    {
        string content = $$"""
[*.cs]
dotnet_diagnostic.CA9999.severity = error

[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[*]
indent_size = 4
""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.Contains("[*.cs]", result, StringComparison.Ordinal);
        Assert.Contains("CA9999", result, StringComparison.Ordinal);
        Assert.Contains("[*]", result, StringComparison.Ordinal);
        Assert.Contains("indent_size = 4", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{src/File.cs}]", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_WorksWithCrLfLineEndings()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""".ReplaceLineEndings("\r\n");

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA1001" });

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.Contains("CA1001", result, StringComparison.Ordinal);
        Assert.Contains("\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_WorksWithLfLineEndings()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""".ReplaceLineEndings("\n");

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA1001" });

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.Contains("CA1001", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_KeepsAllBlocksWhenAllCodesAreInSet()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA2000", "CA1001" });

        Assert.Equal(content, result);
    }

    [Fact]
    public void RemoveBaselineFilters_ReturnsContentUnchangedWhenNoBaselineBlocks()
    {
        string content = """
[*.cs]
dotnet_diagnostic.CA9999.severity = error
""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.Equal(content, result);
    }

    [Fact]
    public void RemoveBaselineFilters_HandlesMultipleFilesInSectionHeader()
    {
        string content = $$"""
[{src/File1.cs,src/File2.cs,src/File3.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_PrunesResolvedFilesInsideKeptCodeBlock()
    {
        string content = $$"""
[{src/File1.cs,src/File2.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByCode =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["CA2000"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src/File2.cs" },
            };

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CA2000" },
            activeFilesByCode);

        Assert.DoesNotContain("src/File1.cs", result, StringComparison.Ordinal);
        Assert.Contains("[{src/File2.cs}]", result, StringComparison.Ordinal);
        Assert.Contains("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_RemovesCodeBlockWhenNoFilesRemainAfterPruning()
    {
        string content = $$"""
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        IReadOnlyDictionary<string, IReadOnlySet<string>> activeFilesByCode =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["CA2000"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src/OtherFile.cs" },
            };

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CA2000" },
            activeFilesByCode);

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_PreservesContentBeforeAndAfterRemovedBlock()
    {
        string content = $$"""
# top of file

[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

# bottom of file
""";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.Contains("# top of file", result, StringComparison.Ordinal);
        Assert.Contains("# bottom of file", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_CaseInsensitiveCodeMatching()
    {
        string content = $$"""
[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = warning {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}

""";

        // codesToKeep uses lowercase — should still match CA2000
        HashSet<string> codesToKeep = new(StringComparer.OrdinalIgnoreCase) { "ca2000" };

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(content, codesToKeep);

        Assert.Contains("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSynchronizedFiles_ReturnsRemovedAndPrunedFilesGroupedByFile()
    {
        string content =
            "[{src/Fixed.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = suggestion " + WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER + "\\n" +
            "\n" +
            "[{src/StillBroken.cs,src/PartiallyFixed.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = suggestion " + WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER + "\\n" +
            "\n";

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
