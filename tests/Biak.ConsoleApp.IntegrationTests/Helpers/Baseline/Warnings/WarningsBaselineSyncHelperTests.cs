// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers.Baseline.Warnings;

namespace Biak.ConsoleApp.IntegrationTests.Helpers.Baseline.Warnings;

public class WarningsBaselineSyncHelperTests
{
    [Fact]
    public void RemoveFiltersShouldRemoveBlockWhenCodeIsNotInActiveFilesByCode()
    {
        const string CONTENT = $$"""
            [{src/File.cs}]
            dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
            """;

        string result = WarningsBaselineSyncHelper.RemoveFilters(
            CONTENT,
            new HashSet<string>(["CA2000"], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        );

        Assert.DoesNotContain("[{src/File.cs}]", result, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet_diagnostic.CA2000.severity", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveFiltersShouldIgnoreBlockWhenDiagnosticLineHasInvalidFormat()
    {
        const string CONTENT = """
            [{src/File.cs}]
            dotnet_diagnostic.CA2000.severity = suggestion
            """;

        string result = WarningsBaselineSyncHelper.RemoveFilters(
            CONTENT,
            new HashSet<string>(["CA2000"], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        );

        Assert.Equal(CONTENT, result);
    }

    [Fact]
    public void GetSynchronizedFilesShouldTreatAllSectionFilesAsRemovedWhenCodeIsNotInActiveFilesByCode()
    {
        const string CONTENT = $$"""
            [{src/File1.cs,src/File2.cs}]
            dotnet_diagnostic.CA2000.severity = suggestion {{WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}}
            """;

        IReadOnlyDictionary<string, IReadOnlySet<string>> result = WarningsBaselineSyncHelper.GetSynchronizedFiles(
            CONTENT,
            new HashSet<string>(["CA2000"], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        );

        Assert.Equal(2, result.Count);
        Assert.Contains("src/File1.cs", result.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("src/File2.cs", result.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("CA2000", result["src/File1.cs"]);
        Assert.Contains("CA2000", result["src/File2.cs"]);
    }
}
