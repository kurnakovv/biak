## 📙 Description
🔒 Always enabled rules | Override selected rules (e.g., formatting rules) so they remain active even when you run the [Disable](Disable) command

## 💻 Usage

.biak/.editorconfig-main
```.editorconfig
root = true

[*]
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

^biak^ always-enabled start
# Method invocation is skipped [InvocationIsSkipped] | https://www.jetbrains.com/help/resharper/InvocationIsSkipped.html
resharper_invocation_is_skipped_highlighting = error

# Part of the code cannot be parsed [NonParsableElement] | https://www.jetbrains.com/help/resharper/NonParsableElement.html
resharper_non_parsable_element_highlighting = error

# Access to a static member of a type via a derived type [AccessToStaticMemberViaDerivedType] | https://www.jetbrains.com/help/resharper/AccessToStaticMemberViaDerivedType.html
resharper_access_to_static_member_via_derived_type_highlighting = error
^biak^ always-enabled end

dotnet_diagnostic.rcs0009.severity = error # RCS0009: Add blank line between declaration and documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009
dotnet_diagnostic.rcs0021.severity = error # RCS0021: Format block's braces on a single line or multiple lines https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021
dotnet_diagnostic.rcs0027.severity = error # RCS0027: Place new line after/before binary operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0027
dotnet_diagnostic.rcs0031.severity = error # RCS0031: Put enum member on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0031

...
```

After running the [Disable](Disable) command, the `.editorconfig` file is converted to:

.editorconfig
```.editorconfig
root = true

[*]
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

# Method invocation is skipped [InvocationIsSkipped] | https://www.jetbrains.com/help/resharper/InvocationIsSkipped.html
resharper_invocation_is_skipped_highlighting = error

# Part of the code cannot be parsed [NonParsableElement] | https://www.jetbrains.com/help/resharper/NonParsableElement.html
resharper_non_parsable_element_highlighting = error

# Access to a static member of a type via a derived type [AccessToStaticMemberViaDerivedType] | https://www.jetbrains.com/help/resharper/AccessToStaticMemberViaDerivedType.html
resharper_access_to_static_member_via_derived_type_highlighting = error

dotnet_diagnostic.rcs0009.severity = none # RCS0009: Add blank line between declaration and documentation comment https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009
dotnet_diagnostic.rcs0021.severity = none # RCS0021: Format block's braces on a single line or multiple lines https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021
dotnet_diagnostic.rcs0027.severity = none # RCS0027: Place new line after/before binary operator https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0027
dotnet_diagnostic.rcs0031.severity = none # RCS0031: Put enum member on its own line https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0031

...
```
As you can see, the rules inside the `^biak^ always-enabled` section keep their original severity values.

## 🔗 Links
* Issues: [#118](https://github.com/kurnakovv/biak/issues/118)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Helpers/AlwaysEnabledRulesHelper.cs)
