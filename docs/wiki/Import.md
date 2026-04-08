## 📙 Description
Import specific rule groups (e.g., CA / IDE / SA / Roslynator, etc.) from separate files for better organization and maintainability.

## 💻 Usage
.editorconfig-main
```.editorconfig
root = true

[*]
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

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

## 🗒️ Notes
* Import files only from the `.biak` folder. This is done for security purposes, so that an attacker cannot gain root access and, for example, obtain the contents of secret files `^biak^ import "../../secretFile"` -> `mySuperSecretValue`. If this rule is violated, a warning will be issued and this import will not be replaced.

* If file not found, then a warning will be issued and this import will not be replaced.

* If the import is commented out `# ^biak^ import "..."`, it will not be applied.

## 🔗 Links
* Issues: [#62](https://github.com/kurnakovv/biak/issues/62)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Helpers/ImportHelper.cs)
