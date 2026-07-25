## 📙 Description

Extract duplicate strings (e.g., file paths) into reusable variables.

## 💻 Usage
.biak/.editorconfig-main
```.editorconfig
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
As you can see, `$excludedFiles` is replaced with `TestFile1.cs,TestFile2.cs`.

## 📜 Rules
* Can only be defined within `.biak/.editorconfig-main`
* Supports concatenation of strings using line breaks
    ```.editorconfig
    ^biak^ var excludedFiles = "TestFile1.cs"
        + ",TestFile2.cs"
        + ",TestFile3.cs"
    ;
    ```

## 🔗 Links
* Issues: [#53](https://github.com/kurnakovv/biak/issues/53)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Helpers/VariableHelper.cs)
