// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class IncludeExcludeFilterHelperTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData(
        """
        # Roslynator
        [*.cs]
        dotnet_diagnostic.rcs0009.severity = error # RCS0009: Add blank line between declaration and documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009
        dotnet_diagnostic.rcs0021.severity = error # RCS0021: Format block's braces on a single line or multiple lines https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021
        dotnet_diagnostic.rcs0027.severity = error # RCS0027: Place new line after/before binary operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0027

        # StyleCop.Analyzers rules
        ^biak^ include [*.cs]
        ^biak^ exclude [{TestFile1.cs,TestFile2.cs}]

        dotnet_diagnostic.SA1025.severity = error
        dotnet_diagnostic.SA1026.severity = error
        dotnet_diagnostic.SA1028.severity = error

        ^biak^ END include/exclude

        # Code analysis (CAxxxx) rules
        ^biak^ include [*.cs]
        ^biak^ exclude [{TestFile1.cs,TestFile2.cs}]

        dotnet_diagnostic.CA1724.severity = error
        dotnet_diagnostic.CA1727.severity = error
        dotnet_diagnostic.CA1716.severity = error

        ^biak^ END include/exclude

        """,

        """
        # Roslynator
        [*.cs]
        dotnet_diagnostic.rcs0009.severity = error # RCS0009: Add blank line between declaration and documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009
        dotnet_diagnostic.rcs0021.severity = error # RCS0021: Format block's braces on a single line or multiple lines https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021
        dotnet_diagnostic.rcs0027.severity = error # RCS0027: Place new line after/before binary operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0027

        # StyleCop.Analyzers rules
        [*.cs]
        dotnet_diagnostic.SA1025.severity = error
        dotnet_diagnostic.SA1026.severity = error
        dotnet_diagnostic.SA1028.severity = error

        [{TestFile1.cs,TestFile2.cs}]
        dotnet_diagnostic.SA1025.severity = none
        dotnet_diagnostic.SA1026.severity = none
        dotnet_diagnostic.SA1028.severity = none

        # Code analysis (CAxxxx) rules
        [*.cs]
        dotnet_diagnostic.CA1724.severity = error
        dotnet_diagnostic.CA1727.severity = error
        dotnet_diagnostic.CA1716.severity = error

        [{TestFile1.cs,TestFile2.cs}]
        dotnet_diagnostic.CA1724.severity = none
        dotnet_diagnostic.CA1727.severity = none
        dotnet_diagnostic.CA1716.severity = none

        """
    )]
    public void ApplyTest(
        string inputContent,
        string outputContent
    )
    {
        string result = IncludeExcludeFilterHelper.Apply(inputContent);

        Assert.Equal(outputContent, result);
    }
}
