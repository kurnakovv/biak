// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class WarningsBaselineSyncHelperTests
{
    // -------------------------------------------------------------------------
    // HasBaselineMarker
    // -------------------------------------------------------------------------

    [Fact]
    public void HasBaselineMarker_ReturnsTrueWhenMarkerPresent()
    {
        string content = "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline";

        Assert.True(WarningsBaselineSyncHelper.HasBaselineMarker(content));
    }

    [Fact]
    public void HasBaselineMarker_ReturnsFalseWhenMarkerAbsent()
    {
        string content = "dotnet_diagnostic.CA2000.severity = suggestion";

        Assert.False(WarningsBaselineSyncHelper.HasBaselineMarker(content));
    }

    [Fact]
    public void HasBaselineMarker_ReturnsFalseForEmptyString()
    {
        Assert.False(WarningsBaselineSyncHelper.HasBaselineMarker(string.Empty));
    }

    [Fact]
    public void HasBaselineMarker_ReturnsTrueForMultilineContentWithMarker()
    {
        string content = @"
[*.cs]
dotnet_diagnostic.CA1001.severity = error

[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline
";

        Assert.True(WarningsBaselineSyncHelper.HasBaselineMarker(content));
    }

    // -------------------------------------------------------------------------
    // IsPathSafe
    // -------------------------------------------------------------------------

    [Fact]
    public void IsPathSafe_ReturnsTrueForFileInBaseDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.True(WarningsBaselineSyncHelper.IsPathSafe(".editorconfig", baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsTrueForEditorconfigMainFileInBaseDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.True(WarningsBaselineSyncHelper.IsPathSafe(".editorconfig-main", baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsTrueForEditorconfigLegacyFileInBaseDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.True(WarningsBaselineSyncHelper.IsPathSafe(".editorconfig-legacy", baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsTrueForFileInSubdirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.True(WarningsBaselineSyncHelper.IsPathSafe(
            Path.Join("src", "nested", ".editorconfig"),
            baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsFalseForParentDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.False(WarningsBaselineSyncHelper.IsPathSafe(
            Path.Join("..", ".editorconfig"),
            baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsFalseForAncestorDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.False(WarningsBaselineSyncHelper.IsPathSafe(
            Path.Join("..", "..", ".editorconfig"),
            baseDir));
    }

    [Fact]
    public void IsPathSafe_ReturnsFalseForSiblingDirectory()
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        Assert.False(WarningsBaselineSyncHelper.IsPathSafe(
            Path.Join("..", "other-project", ".editorconfig"),
            baseDir));
    }

    // -------------------------------------------------------------------------
    // GetBaselineDiagnosticCodes
    // -------------------------------------------------------------------------

    [Fact]
    public void GetBaselineDiagnosticCodes_ReturnsEmptySetForContentWithoutBaseline()
    {
        string content = @"
[*.cs]
dotnet_diagnostic.CA1001.severity = error
";

        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Empty(codes);
    }

    [Fact]
    public void GetBaselineDiagnosticCodes_ReturnsEmptySetForEmptyContent()
    {
        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(string.Empty);

        Assert.Empty(codes);
    }

    [Fact]
    public void GetBaselineDiagnosticCodes_ReturnsSingleCodeForOneEntry()
    {
        string content = @"
[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline
";

        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Single(codes);
        Assert.Contains("CA2000", codes);
    }

    [Fact]
    public void GetBaselineDiagnosticCodes_ReturnsAllCodesForMultipleEntries()
    {
        string content = @"
[{src/File1.cs,src/File2.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline

[{src/File3.cs}]
dotnet_diagnostic.CA1001.severity = suggestion # ^biak^ baseline

[{src/File4.cs}]
dotnet_diagnostic.CS1234.severity = suggestion # ^biak^ baseline
";

        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Equal(3, codes.Count);
        Assert.Contains("CA2000", codes);
        Assert.Contains("CA1001", codes);
        Assert.Contains("CS1234", codes);
    }

    [Fact]
    public void GetBaselineDiagnosticCodes_DeduplicatesSameCodeAppearingTwice()
    {
        string content = @"
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline

[{src/File2.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline
";

        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Single(codes);
        Assert.Contains("CA2000", codes);
    }

    [Fact]
    public void GetBaselineDiagnosticCodes_IgnoresNonBaselineDiagnosticLines()
    {
        string content = @"
[*.cs]
dotnet_diagnostic.CA9999.severity = error
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline
";

        HashSet<string> codes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(content);

        Assert.Single(codes);
        Assert.Contains("CA2000", codes);
        Assert.DoesNotContain("CA9999", codes);
    }

    // -------------------------------------------------------------------------
    // SetBaselineForBuild
    // -------------------------------------------------------------------------

    [Fact]
    public void SetBaselineForBuild_WhenActivated_ReplacesSuggestionWithWarning()
    {
        string content = "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline";
        string expected = "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: true);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetBaselineForBuild_WhenActivated_ReplacesAllOccurrences()
    {
        string content = @"
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = suggestion # ^biak^ baseline
";
        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: true);

        Assert.DoesNotContain("suggestion # ^biak^ baseline", result, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(result, "warning # ^biak^ baseline"));
    }

    [Fact]
    public void SetBaselineForBuild_WhenActivated_DoesNotChangeNonBaselineLines()
    {
        string content = @"
[*.cs]
dotnet_diagnostic.CA9999.severity = suggestion
";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: true);

        Assert.Equal(content, result);
    }

    [Fact]
    public void SetBaselineForBuild_WhenActivated_ReturnsContentUnchangedWhenNoBaselineMarkers()
    {
        string content = "root = true";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: true);

        Assert.Equal(content, result);
    }

    [Fact]
    public void SetBaselineForBuild_WhenDeactivated_ReplacesWarningWithSuggestion()
    {
        string content = "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline";
        string expected = "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: false);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetBaselineForBuild_WhenDeactivated_ReplacesAllOccurrences()
    {
        string content = @"
[{src/File1.cs}]
dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline

[{src/File2.cs}]
dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline
";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: false);

        Assert.DoesNotContain("warning # ^biak^ baseline", result, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(result, "suggestion # ^biak^ baseline"));
    }

    [Fact]
    public void SetBaselineForBuild_WhenDeactivated_DoesNotChangeNonBaselineWarningLines()
    {
        string content = @"
[*.cs]
dotnet_diagnostic.CA9999.severity = warning
";

        string result = WarningsBaselineSyncHelper.SetBaselineForBuild(content, activate: false);

        Assert.Equal(content, result);
    }

    [Fact]
    public void SetBaselineForBuild_ActivateAndDeactivateAreInverses()
    {
        string original = @"
[{src/File.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline

[*.cs]
dotnet_diagnostic.CA9999.severity = warning
";

        string roundTripped = WarningsBaselineSyncHelper.SetBaselineForBuild(
            WarningsBaselineSyncHelper.SetBaselineForBuild(original, activate: true),
            activate: false);

        Assert.Equal(original, roundTripped);
    }

    // -------------------------------------------------------------------------
    // RemoveBaselineFilters
    // -------------------------------------------------------------------------

    [Fact]
    public void RemoveBaselineFilters_KeepsBlockWhenCodeIsInSet()
    {
        string content =
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA2000" });

        Assert.Contains("[{src/File.cs}]", result, StringComparison.Ordinal);
        Assert.Contains("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_RemovesBlockWhenCodeNotInSet()
    {
        string content =
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("[{src/File.cs}]", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_KeepsOnlyCodesInSet()
    {
        string content =
            "[{src/File1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[{src/File2.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[{src/File3.cs}]\n" +
            "dotnet_diagnostic.CS1234.severity = warning # ^biak^ baseline\n" +
            "\n";

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
        string content =
            "[{src/File1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[{src/File2.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA1001", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_PreservesNonBaselineSections()
    {
        string content =
            "[*.cs]\n" +
            "dotnet_diagnostic.CA9999.severity = error\n" +
            "\n" +
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[*]\n" +
            "indent_size = 4\n";

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
        string content =
            "[{src/File1.cs}]\r\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\r\n" +
            "\r\n" +
            "[{src/File2.cs}]\r\n" +
            "dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline\r\n" +
            "\r\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA1001" });

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.Contains("CA1001", result, StringComparison.Ordinal);
        Assert.Contains("\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_WorksWithLfLineEndings()
    {
        string content =
            "[{src/File1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[{src/File2.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA1001" });

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.Contains("CA1001", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_KeepsAllBlocksWhenAllCodesAreInSet()
    {
        string content =
            "[{src/File1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "[{src/File2.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string> { "CA2000", "CA1001" });

        Assert.Equal(content, result);
    }

    [Fact]
    public void RemoveBaselineFilters_ReturnsContentUnchangedWhenNoBaselineBlocks()
    {
        string content =
            "[*.cs]\n" +
            "dotnet_diagnostic.CA9999.severity = error\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.Equal(content, result);
    }

    [Fact]
    public void RemoveBaselineFilters_HandlesMultipleFilesInSectionHeader()
    {
        string content =
            "[{src/File1.cs,src/File2.cs,src/File3.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("[{", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_PrunesResolvedFilesInsideKeptCodeBlock()
    {
        string content =
            "[{src/File1.cs,src/File2.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

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
        string content =
            "[{src/File1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

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
        string content =
            "# top of file\n" +
            "\n" +
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n" +
            "# bottom of file\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.Contains("# top of file", result, StringComparison.Ordinal);
        Assert.Contains("# bottom of file", result, StringComparison.Ordinal);
        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_HandlesSuggestionSeverityInBaseline()
    {
        // Baseline entries may be in "suggestion" form (before activation)
        string content =
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline\n" +
            "\n";

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(
            content, new HashSet<string>());

        Assert.DoesNotContain("CA2000", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBaselineFilters_CaseInsensitiveCodeMatching()
    {
        string content =
            "[{src/File.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = warning # ^biak^ baseline\n" +
            "\n";

        // codesToKeep uses lowercase — should still match CA2000
        HashSet<string> codesToKeep = new(StringComparer.OrdinalIgnoreCase) { "ca2000" };

        string result = WarningsBaselineSyncHelper.RemoveBaselineFilters(content, codesToKeep);

        Assert.Contains("CA2000", result, StringComparison.Ordinal);
    }

    // -------------------------------------------------------------------------
    // Full round-trip scenario
    // -------------------------------------------------------------------------

    [Fact]
    public void FullSyncRoundTrip_FixedWarningsAreRemovedAndRemainingArePreserved()
    {
        // Simulate the .editorconfig as written by the init command
        string original = @"
[{src/Fixed.cs}]
dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline

[{src/StillBroken.cs}]
dotnet_diagnostic.CA1001.severity = suggestion # ^biak^ baseline

[*.cs]
dotnet_diagnostic.CA9999.severity = error
";

        // Step 1: activate for build
        string activated = WarningsBaselineSyncHelper.SetBaselineForBuild(original, activate: true);

        Assert.Contains("= warning # ^biak^ baseline", activated, StringComparison.Ordinal);

        // Step 2: simulate build — only CA1001 is still a warning
        HashSet<string> baselineCodes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(original);
        HashSet<string> activeWarningCodes = new(StringComparer.OrdinalIgnoreCase) { "CA1001" };
        HashSet<string> codesToKeep = baselineCodes
            .Where(c => activeWarningCodes.Contains(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Step 3: remove resolved filters and deactivate
        string synced = WarningsBaselineSyncHelper.RemoveBaselineFilters(original, codesToKeep);
        synced = WarningsBaselineSyncHelper.SetBaselineForBuild(synced, activate: false);

        // CA2000 (fixed) must be gone, CA1001 (still active) must remain
        Assert.DoesNotContain("CA2000", synced, StringComparison.Ordinal);
        Assert.DoesNotContain("[{src/Fixed.cs}]", synced, StringComparison.Ordinal);
        Assert.Contains("CA1001", synced, StringComparison.Ordinal);
        Assert.Contains("[{src/StillBroken.cs}]", synced, StringComparison.Ordinal);
        // Non-baseline section must be untouched
        Assert.Contains("[*.cs]", synced, StringComparison.Ordinal);
        Assert.Contains("CA9999", synced, StringComparison.Ordinal);
        // All remaining entries must be back to suggestion
        Assert.DoesNotContain("= warning # ^biak^ baseline", synced, StringComparison.Ordinal);
        Assert.Contains("= suggestion # ^biak^ baseline", synced, StringComparison.Ordinal);
    }

    [Fact]
    public void FullSyncRoundTrip_AllWarningsFixedProducesNoBaselineBlocks()
    {
        string original =
            "[{src/Fixed1.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline\n" +
            "\n" +
            "[{src/Fixed2.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = suggestion # ^biak^ baseline\n" +
            "\n";

        HashSet<string> baselineCodes = WarningsBaselineSyncHelper.GetBaselineDiagnosticCodes(original);
        HashSet<string> activeWarningCodes = new(StringComparer.OrdinalIgnoreCase); // no warnings left
        HashSet<string> codesToKeep = baselineCodes
            .Where(c => activeWarningCodes.Contains(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string synced = WarningsBaselineSyncHelper.RemoveBaselineFilters(original, codesToKeep);
        synced = WarningsBaselineSyncHelper.SetBaselineForBuild(synced, activate: false);

        Assert.False(WarningsBaselineSyncHelper.HasBaselineMarker(synced));
        Assert.DoesNotContain("[{", synced, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSynchronizedFiles_ReturnsRemovedAndPrunedFilesGroupedByFile()
    {
        string content =
            "[{src/Fixed.cs}]\n" +
            "dotnet_diagnostic.CA2000.severity = suggestion # ^biak^ baseline\n" +
            "\n" +
            "[{src/StillBroken.cs,src/PartiallyFixed.cs}]\n" +
            "dotnet_diagnostic.CA1001.severity = suggestion # ^biak^ baseline\n" +
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

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
