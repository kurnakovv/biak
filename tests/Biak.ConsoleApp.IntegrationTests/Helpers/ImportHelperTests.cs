// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class ImportHelperTests
{
    [Theory]
    [InlineData("", "", null)]
    [InlineData(
        """
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import "Categories/.editorconfig-Roslynator"

        ^biak^ import "Categories/.editorconfig-StyleCop"
        """,

        """
        root = true
        
        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true
        
        [*.cs]
        ##
        ## Roslynator
        ##
        # All rules here https://josefpihrt.github.io/docs/roslynator/configuration

        # Disable all rules
        dotnet_analyzer_diagnostic.category-roslynator.severity = none

        # Row length limits
        roslynator_max_line_length = 200
        dotnet_diagnostic.rcs0056.severity = error

        # VS extension https://marketplace.visualstudio.com/items?itemName=PaulHarrington.EditorGuidelines
        guidelines = 200
        guidelines_style = 1px dotted purple

        dotnet_diagnostic.rcs0009.severity = error # RCS0009: Add blank line between declaration and documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009
        dotnet_diagnostic.rcs0021.severity = error # RCS0021: Format block's braces on a single line or multiple lines https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021
        dotnet_diagnostic.rcs0027.severity = error # RCS0027: Place new line after/before binary operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0027


        [*.cs]
        # https://gist.github.com/kurnakovv/70a5d76dc5f3eb9ef114b182283cb407
        ##
        ## StyleCop.Analyzers
        ##
        # All rules here https://github.com/DotNetAnalyzers/StyleCopAnalyzers/tree/master/documentation

        # Disable all rules
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.AlternativeRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.DocumentationRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.LayoutRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.MaintainabilityRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.NamingRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.OrderingRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.ReadabilityRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.SpacingRules.severity = none
        dotnet_analyzer_diagnostic.category-StyleCop.CSharp.SpecialRules.severity = none


        dotnet_diagnostic.SA1001.severity = error # https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1001.md Use <GenerateDocumentationFile>true</GenerateDocumentationFile>
        dotnet_diagnostic.SA1002.severity = error # https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
        dotnet_diagnostic.SA1003.severity = error # https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1003.md

        """,
        null
    )]
    [InlineData(
        """
        # Out .biak folder
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import "../Categories/.editorconfig-Roslynator"

        ^biak^ import "../../Categories/.editorconfig-StyleCop"
        """,

        """
        # Out .biak folder
        root = true
        
        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true
        
        ^biak^ import "../Categories/.editorconfig-Roslynator"
        
        ^biak^ import "../../Categories/.editorconfig-StyleCop"
        """,
        ImportConstant.FORBIDDEN_OUTSIDE
    )]
    [InlineData(
        """
        # Invalid import path
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import "invalidPath1"

        ^biak^ import "invalidPath2"
        """,

        """
        # Invalid import path
        root = true
        
        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true
        
        ^biak^ import "invalidPath1"
        
        ^biak^ import "invalidPath2"
        """,
        ImportConstant.FILE_NOT_FOUND
    )]
    [InlineData(
        """
        # Commented import
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        # ^biak^ import "Categories/.editorconfig-Roslynator"

        # ^biak^ import "Categories/.editorconfig-Roslynator"
        """,

        """
        # Commented import
        root = true
        
        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true
        
        # ^biak^ import "Categories/.editorconfig-Roslynator"
        
        # ^biak^ import "Categories/.editorconfig-Roslynator"
        """,
        null
    )]
    public async Task ReplaceTestAsync(
        string inputContent,
        string outputContent,
        string? outputMessage
    )
    {
        string originalDirectory = Directory.GetCurrentDirectory();

        TestDirectory testDir = new($"{nameof(ImportHelperTests)}_{nameof(ReplaceTestAsync)}");

        string biakCategoriesDir = Path.Join(testDir.Value, ".biak", "Categories");
        Directory.CreateDirectory(biakCategoriesDir);

        string templateEditorconfigRoslynator = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Import",
            ".biak",
            "Categories",
            ".editorconfig-Roslynator"
        );

        File.Copy(
            sourceFileName: templateEditorconfigRoslynator,
            destFileName: Path.Join(biakCategoriesDir, ".editorconfig-Roslynator"),
            overwrite: true
        );

        string templateEditorconfigStyleCop = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Import",
            ".biak",
            "Categories",
            ".editorconfig-StyleCop"
        );

        File.Copy(
            sourceFileName: templateEditorconfigStyleCop,
            destFileName: Path.Join(biakCategoriesDir, ".editorconfig-StyleCop"),
            overwrite: true
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string result = await ImportHelper.ReplaceAsync(inputContent);

            Assert.Equal(outputContent, result);

            if (outputMessage != null)
            {
                string outputResult = output.ToString();
                Assert.Contains(outputMessage, outputResult, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Console.SetOut(originalOut);
        }
    }
}
