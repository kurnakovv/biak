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

    [InlineData(
        """
        # URL imports
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/1d3a3421a7149e9040547b8c8bc1a4bb/raw/eac6b5ac9de284d1f98e4783fdadfc20775e8a48/.editorconfig

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/ff955cd224602b738f697f3246fcda92/raw/1646b6563a79c4d2e78ae654152a3082c10428f6/.editorconfig
        """,

        """
        # URL imports
        root = true
        
        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ##
        ## IDisposableAnalyzers rules
        ##
        # All rules here https://github.com/DotNetAnalyzers/IDisposableAnalyzers

        dotnet_diagnostic.IDISP001.severity = suggestion # Dispose created
        dotnet_diagnostic.IDISP002.severity = suggestion # Dispose member
        dotnet_diagnostic.IDISP003.severity = suggestion # Dispose previous before re-assigning
        dotnet_diagnostic.IDISP004.severity = suggestion # Don't ignore created IDisposable
        dotnet_diagnostic.IDISP005.severity = suggestion # Return type should indicate that the value should be disposed
        dotnet_diagnostic.IDISP006.severity = suggestion # Implement IDisposable
        dotnet_diagnostic.IDISP007.severity = suggestion # Don't dispose injected
        dotnet_diagnostic.IDISP008.severity = suggestion # Don't assign member with injected and created disposables
        dotnet_diagnostic.IDISP009.severity = suggestion # Add IDisposable interface
        dotnet_diagnostic.IDISP010.severity = suggestion # Call base.Dispose(disposing)
        dotnet_diagnostic.IDISP011.severity = suggestion # Don't return disposed instance
        dotnet_diagnostic.IDISP012.severity = suggestion # Property should not return created disposable
        dotnet_diagnostic.IDISP013.severity = suggestion # Await in using
        dotnet_diagnostic.IDISP014.severity = suggestion # Use a single instance of HttpClient
        dotnet_diagnostic.IDISP015.severity = suggestion # Member should not return created and cached instance
        dotnet_diagnostic.IDISP016.severity = suggestion # Don't use disposed instance
        dotnet_diagnostic.IDISP017.severity = suggestion # Prefer using
        dotnet_diagnostic.IDISP018.severity = suggestion # Call SuppressFinalize
        dotnet_diagnostic.IDISP019.severity = suggestion # Call SuppressFinalize
        dotnet_diagnostic.IDISP020.severity = suggestion # Call SuppressFinalize(this)
        dotnet_diagnostic.IDISP021.severity = suggestion # Call this.Dispose(true)
        dotnet_diagnostic.IDISP022.severity = suggestion # Call this.Dispose(false)
        dotnet_diagnostic.IDISP023.severity = suggestion # Don't use reference types in finalizer context
        dotnet_diagnostic.IDISP024.severity = suggestion # Don't call GC.SuppressFinalize(this) when the type is sealed and has no finalizer
        dotnet_diagnostic.IDISP025.severity = suggestion # Class with no virtual dispose method should be sealed
        dotnet_diagnostic.IDISP026.severity = suggestion # Class with no virtual DisposeAsyncCore method should be sealed

        ## **END** IDisposableAnalyzers rules **END**
        
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
        dotnet_diagnostic.rcs0031.severity = error # RCS0031: Put enum member on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0031
        dotnet_diagnostic.rcs0033.severity = error # RCS0033: Put statement on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0033
        dotnet_diagnostic.rcs0034.severity = error # RCS0034: Put type parameter constraint on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0034
        dotnet_diagnostic.rcs0041.severity = error # RCS0041: Remove new line between 'if' keyword and 'else' keyword https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0041
        dotnet_diagnostic.rcs0050.severity = error # RCS0050: Add blank line before top declaration https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0050
        dotnet_diagnostic.rcs0053.severity = error # RCS0053: Fix formatting of a list https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0053
        dotnet_diagnostic.rcs1006.severity = error # RCS1006: Merge 'else' with nested 'if' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1006
        dotnet_diagnostic.rcs1033.severity = error # RCS1033: Remove redundant boolean literal https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1033
        dotnet_diagnostic.rcs1034.severity = error # RCS1034: Remove redundant 'sealed' modifier https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1034
        dotnet_diagnostic.rcs1043.severity = error # RCS1043: Remove 'partial' modifier from type with a single part https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1043
        dotnet_diagnostic.rcs1046.severity = error # RCS1046: Asynchronous method name should end with 'Async' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1046
        dotnet_diagnostic.rcs1047.severity = error # RCS1047: Non-asynchronous method name should not end with 'Async' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1047
        dotnet_diagnostic.rcs1055.severity = error # RCS1055: Unnecessary semicolon at the end of declaration https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1055
        dotnet_diagnostic.rcs1061.severity = error # RCS1061: Merge 'if' with nested 'if' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1061
        dotnet_diagnostic.rcs1071.severity = error # RCS1071: Remove redundant base constructor call https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1071
        dotnet_diagnostic.rcs1073.severity = error # RCS1073: Convert 'if' to 'return' statement https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1073
        dotnet_diagnostic.rcs1074.severity = error # RCS1074: Remove redundant constructor https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1074
        dotnet_diagnostic.rcs1081.severity = error # RCS1081: Split variable declaration https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1081
        dotnet_diagnostic.rcs1097.severity = error # RCS1097: Remove redundant 'ToString' call https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1097
        dotnet_diagnostic.rcs1098.severity = error # RCS1098: Constant values should be placed on right side of comparisons https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1098
        dotnet_diagnostic.rcs1099.severity = error # RCS1099: Default label should be the last label in a switch section https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1099
        dotnet_diagnostic.rcs1103.severity = error # RCS1103: Convert 'if' to assignment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1103
        dotnet_diagnostic.rcs1104.severity = error # RCS1104: Simplify conditional expression https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1104
        dotnet_diagnostic.rcs1105.severity = error # RCS1105: Unnecessary interpolation https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1105
        dotnet_diagnostic.rcs1108.severity = error # RCS1108: Add 'static' modifier to all partial class declarations https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1108
        dotnet_diagnostic.rcs1113.severity = error # RCS1113: Use 'string.IsNullOrEmpty' method https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1113
        dotnet_diagnostic.rcs1114.severity = error # RCS1114: Remove redundant delegate creation https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1114
        dotnet_diagnostic.rcs1124.severity = warning # RCS1124: Inline local variable https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1124
        dotnet_diagnostic.rcs1129.severity = error # RCS1129: Remove redundant field initialization https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1129
        dotnet_diagnostic.rcs1133.severity = error # RCS1133: Remove redundant Dispose/Close call https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1133
        dotnet_diagnostic.rcs1136.severity = error # RCS1136: Merge switch sections with equivalent content https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1136
        dotnet_diagnostic.rcs1146.severity = error # RCS1146: Use conditional access https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1146
        dotnet_diagnostic.rcs1154.severity = error # RCS1154: Sort enum members https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1154
        dotnet_diagnostic.rcs1161.severity = error # RCS1161: Enum should declare explicit values https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1161
        dotnet_diagnostic.rcs1166.severity = error # RCS1166: Value type object is never equal to null https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1166
        dotnet_diagnostic.rcs1170.severity = error # RCS1170: Use read-only auto-implemented property https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1170
        dotnet_diagnostic.rcs1172.severity = error # RCS1172: Use 'is' operator instead of 'as' operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1172
        dotnet_diagnostic.rcs1182.severity = error # RCS1182: Remove redundant base interface https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1182
        dotnet_diagnostic.rcs1188.severity = error # RCS1188: Remove redundant auto-property initialization https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1188
        dotnet_diagnostic.rcs1190.severity = error # RCS1190: Join string expressions https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1190
        dotnet_diagnostic.rcs1192.severity = error # RCS1192: Unnecessary usage of verbatim string literal https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1192
        dotnet_diagnostic.rcs1196.severity = error # RCS1196: Call extension method as instance method https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1196
        dotnet_diagnostic.rcs1197.severity = error # RCS1197: Optimize StringBuilder.Append/AppendLine call https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1197
        dotnet_diagnostic.rcs1199.severity = error # RCS1199: Unnecessary null check https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1199
        dotnet_diagnostic.rcs1200.severity = error # RCS1200: Call 'Enumerable.ThenBy' instead of 'Enumerable.OrderBy' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1200
        dotnet_diagnostic.rcs1211.severity = error # RCS1211: Remove unnecessary 'else' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1211
        dotnet_diagnostic.rcs1215.severity = error # RCS1215: Expression is always equal to true/false https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1215
        dotnet_diagnostic.rcs1220.severity = error # RCS1220: Use pattern matching instead of combination of 'is' operator and cast operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1220
        dotnet_diagnostic.rcs1229.severity = error # RCS1229: Use async/await when necessary https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1229
        dotnet_diagnostic.rcs1232.severity = error # RCS1232: Order elements in documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1232
        dotnet_diagnostic.rcs1236.severity = error # RCS1236: Use exception filter https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1236
        dotnet_diagnostic.rcs1239.severity = error # RCS1239: Use 'for' statement instead of 'while' statement https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1239
        dotnet_diagnostic.rcs1243.severity = error # RCS1243: Duplicate word in a comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1243
        dotnet_diagnostic.rcs1251.severity = error # RCS1251: Remove unnecessary braces from record declaration https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1251
        dotnet_diagnostic.rcs1259.severity = error # RCS1259: Remove empty syntax https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1259
        dotnet_diagnostic.rcs1261.severity = error # RCS1261: Resource can be disposed asynchronously https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1261/
        dotnet_diagnostic.rcs1262.severity = error # RCS1262: Unnecessary raw string literal https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1262
        dotnet_diagnostic.rcs1263.severity = error # RCS1263: Invalid reference in a documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1263
        dotnet_diagnostic.rcs1266.severity = error # RCS1266: Use raw string literal https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1266

        roslynator_block_braces_style = single_line_when_empty

        [{**Endpoint.cs,**Controller.cs,**Hub*}]
        dotnet_diagnostic.rcs1046.severity = none # RCS1046: Asynchronous method name should end with 'Async' https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1046
        
        """,
        null
    )]
    [InlineData(
        """
        # Invalid URL imports
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/invalidPath

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/invalidPath
        """,

        """
        # Invalid URL imports
        root = true

        [*]
        insert_final_newline = true
        indent_style = space
        indent_size = 4
        trim_trailing_whitespace = true

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/invalidPath

        ^biak^ import https://gist.githubusercontent.com/kurnakovv/invalidPath
        """,
        ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK
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

    [Theory]
    [InlineData("^biak^ import http://example.com/file.txt")]
    [InlineData("^biak^ import https://127.0.0.1/secret.txt")]
    [InlineData("^biak^ import https://[::1]/secret.txt")]
    [InlineData("^biak^ import http://10.0.0.1")]
    [InlineData("^biak^ import http://172.16.0.1")]
    [InlineData("^biak^ import http://172.31.255.255")]
    [InlineData("^biak^ import http://192.168.0.1")]
    [InlineData("^biak^ import http://169.254.0.1")]
    [InlineData("^biak^ import https://10.0.0.1/file.txt")]
    [InlineData("^biak^ import https://172.16.0.1/file.txt")]
    [InlineData("^biak^ import https://192.168.0.1/file.txt")]
    [InlineData("^biak^ import https://169.254.0.1/file.txt")]
    [InlineData("^biak^ import https://172.31.255.255/file.txt")]
    [InlineData("^biak^ import https://169.254.254.1/file.txt")]
    [InlineData("^biak^ import https://localhost/file.txt")]
    [InlineData("^biak^ import https://example.invalid/file.txt")]
    public async Task ReplaceTestInvalidUrlAsync(string input)
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            string result = await ImportHelper.ReplaceAsync(input);

            Assert.Equal(input, result);

            string outputResult = output.ToString();
            Assert.Contains(ImportConstant.URL_NOT_ALLOWED, outputResult, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
