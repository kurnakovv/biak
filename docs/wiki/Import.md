## 📙 Description
Import specific rule groups (e.g., CA / IDE / SA / Roslynator, etc.) from separate files or URLs for better organization and maintainability.

## 💻 Usage
Structure
```
.biak/
├── Categories/
│ ├── .editorconfig-CodeAnalysis-CAxxxx
│ ├── .editorconfig-CodeStyle-IDExxxx
│ ├── .editorconfig-IDisposableAnalyzers
│ ├── .editorconfig-kuker
│ ├── .editorconfig-Roslynator
│ └── .editorconfig-StyleCop
│
├── .editorconfig-main
└── config.json
```

.editorconfig-main
```.editorconfig
root = true

[*]
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

^biak^ import "Categories/.editorconfig-CodeAnalysis-CAxxxx"

^biak^ import "Categories/.editorconfig-CodeStyle-IDExxxx"

^biak^ import "Categories/.editorconfig-IDisposableAnalyzers"

^biak^ import "Categories/.editorconfig-kuker"

^biak^ import "Categories/.editorconfig-Roslynator"

^biak^ import "Categories/.editorconfig-StyleCop"

...
```

Where files at the specified paths are taken and replaced with the contents of these files, for example

.biak/Categories/.editorconfig-Roslynator
```.editorconfig
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
```


After running the [Enable](Enable) / [Disable](Disable) commands, the `.editorconfig` file is converted to:

.editorconfig
```.editorconfig
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
dotnet_diagnostic.rcs0031.severity = error # RCS0031: Put enum member on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0031

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

...
```
As you can see, imports were replaced by content from these same files.

This feature can also be used for links and will work in the same way as for files.
```.editorconfig
root = true

[*]
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

# Code analysis (CAxxxx) rules
^biak^ import https://gist.githubusercontent.com/kurnakovv/7ca966f8099209136f4baa87d804d39c/raw/cfb32e5b1da5c9dcd7340de5f1d166109642979d/.editorconfig

# Code-style (IDExxxx) rules
^biak^ import https://gist.githubusercontent.com/kurnakovv/b396696805339988a547c6d53a0d0254/raw/193bb9d95f8132a690d7a3e51aa8291bd2bb4dba/.editorconfig
```

## 🗒️ Notes
* Import files only from the `.biak` folder. This is done for security purposes, so that an attacker cannot import files outside `.biak` and, for example, obtain the contents of secret files `^biak^ import "../../secretFile"` -> `mySuperSecretValue`. If this rule is violated, a warning will be issued and this import will not be replaced.

* If file not found, then a warning will be issued and this import will not be replaced.

* If the content from the URL could not be obtained, then a warning will be issued and this import will not be replaced.

* If the URL uses HTTP or points to a private network or localhost, then a warning will be issued and this import will not be replaced.

* If the URL response exceeds 5 MB, the import will not be replaced. This restriction is enforced for security reasons.

* If the content type is not text, but an image or another format, then this import will not be replaced.

* If the import is commented out `# ^biak^ import "..."`, it will not be applied.

> [!NOTE]
> You can override the behavior on import failures using the `onImportFailure` config field ([docs](Config)).

## 🔗 Links
* Issues: [#62](https://github.com/kurnakovv/biak/issues/62) | [#64](https://github.com/kurnakovv/biak/issues/64) | [#76](https://github.com/kurnakovv/biak/issues/76)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Helpers/ImportHelper.cs)
