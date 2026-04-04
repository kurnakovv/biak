## 📙 Description
Apply rules to all C# `[*.cs]` files except selected ones (e.g., `[{TestClass1.cs,TestClass2.cs}]`).

## 💻 Usage
.editorconfig-main
```.editorconfig
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

...
```

After running the [Enable](Enable) / [Disable](Disable) commands, the `.editorconfig` file is converted to:

```.editorconfig
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

...
```
As you can see, the rules are applied to all files except the specifically excluded ones.

It is also recommended to use the [variable functionality](Variables) together with this feature to avoid duplicating paths.

# ❔ Why
* It is currently not possible to exclude multiple files using built-in syntax. Only `[!name]` (for a single file) or `{s1,s2,s3}` (without `!`) are supported.

* If done manually, you would need to duplicate rules, which can lead to bugs and maintenance issues.

## 🔗 Links
* Issues: [#59](https://github.com/kurnakovv/biak/issues/59)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Helpers/IncludeExcludeFilterHelper.cs)
