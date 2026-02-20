// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class SeverityHelperTests
{
    [Theory]
    [InlineData("dotnet_diagnostic.CA2000.severity = error", "dotnet_diagnostic.CA2000.severity = none")]
    [InlineData("dotnet_diagnostic.CA1001.severity = warning", "dotnet_diagnostic.CA1001.severity = none")]
    [InlineData("dotnet_diagnostic.CA1707.severity = suggestion", "dotnet_diagnostic.CA1707.severity = none")]
    [InlineData("dotnet_diagnostic.CA9999.severity = NONE", "dotnet_diagnostic.CA9999.severity = NONE")]
    public void Disable_ReplacesSeverityWithNone(string input, string expected)
    {
        string result = SeverityHelper.Disable(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Disable_DoesNotChangeNonSeverityLines()
    {
        string input = @"
root = true
[*.cs]
indent_style = space
indent_size = 4
";

        string result = SeverityHelper.Disable(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void Disable_ReplacesMultipleSeveritiesInOneContent()
    {
        string input = @"
dotnet_diagnostic.CA2000.severity = error
dotnet_diagnostic.CA1001.severity = warning
dotnet_diagnostic.CA1707.severity = suggestion
";
        string expected = @"
dotnet_diagnostic.CA2000.severity = none
dotnet_diagnostic.CA1001.severity = none
dotnet_diagnostic.CA1707.severity = none
";

        string result = SeverityHelper.Disable(input);

        Assert.Equal(expected, result);
    }
}
