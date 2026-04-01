// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class VariableHelperTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData(
        """
        # Replace excludedFiles (real case)
        ^biak^ var excludedFiles = "TestFile1.cs,TestFile2.cs";

        # StyleCop.Analyzers rules
        [*.cs]
        dotnet_diagnostic.SA1025.severity = error
        dotnet_diagnostic.SA1026.severity = error
        dotnet_diagnostic.SA1028.severity = error

        [{$excludedFiles}]
        dotnet_diagnostic.SA1025.severity = none
        dotnet_diagnostic.SA1026.severity = none
        dotnet_diagnostic.SA1028.severity = none

        # Code analysis (CAxxxx) rules
        [*.cs]
        dotnet_diagnostic.CA1724.severity = error
        dotnet_diagnostic.CA1727.severity = error
        dotnet_diagnostic.CA1716.severity = error

        [{$excludedFiles}]
        dotnet_diagnostic.CA1724.severity = none
        dotnet_diagnostic.CA1727.severity = none
        dotnet_diagnostic.CA1716.severity = none
        """,

        """
        # Replace excludedFiles (real case)

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
    [InlineData(
        """
        # Replace excludedFiles (simple)
        ^biak^ var excludedFiles = "TestFile1.cs,TestFile2.cs";

        $excludedFiles
        """,

        """
        # Replace excludedFiles (simple)

        TestFile1.cs,TestFile2.cs
        """
    )]
    [InlineData(
        """
        # Replace excludedFiles (concat)
        ^biak^ var excludedFiles = "TestFile1.cs," + "TestFile2.cs," + "TestFile3.cs";

        $excludedFiles
        """,

        """
        # Replace excludedFiles (concat)

        TestFile1.cs,TestFile2.cs,TestFile3.cs
        """
    )]
    [InlineData(
        """
        # Replace excludedFiles (concat new lines)
        ^biak^ var excludedFiles = "TestFile1.cs,"
            + "TestFile2.cs,"
            + "TestFile3.cs"
        ;


        $excludedFiles
        """,

        """
        # Replace excludedFiles (concat new lines)


        TestFile1.cs,TestFile2.cs,TestFile3.cs
        """
    )]
    [InlineData(
        """
        # Replace 2 variables
        ^biak^ var excludedFiles = "TestFile1.cs,"
            + "TestFile2.cs,"
            + "TestFile3.cs"
        ;

        ^biak^ var excludedFiles1 = "TestFile01.cs,"
            + "TestFile02.cs,"
            + "TestFile03.cs"
        ;


        $excludedFiles
        $excludedFiles1
        """,

        """
        # Replace 2 variables



        TestFile1.cs,TestFile2.cs,TestFile3.cs
        TestFile01.cs,TestFile02.cs,TestFile03.cs
        """
    )]
    [InlineData(
        """
        # Replace excludedFiles with special symbols
        ^biak^ var excludedFiles = "**Services/TestFile1.cs,"
            + "**Controllers/TestFile2.cs,"
            + "**Attributes/TestFile3.cs";

        {$excludedFiles}
        """,

        """
        # Replace excludedFiles with special symbols

        {**Services/TestFile1.cs,**Controllers/TestFile2.cs,**Attributes/TestFile3.cs}
        """
    )]
    public void SubstituteTest(
        string inputContent,
        string outputContent
    )
    {
        string result = VariableHelper.Substitute(inputContent);

        Assert.Equal(outputContent, result);
    }
}
