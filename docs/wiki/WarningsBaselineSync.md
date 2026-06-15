## 📙 Description
🚧 Sync warnings baseline | Removes current filters when no warnings are found in the corresponding files. This feature is especially useful when the filter list becomes too large because of a high number of files and is difficult to clean up manually.

## 💻 Usage

```
dotnet biak warnings-baseline sync [--path <path>]
```

Let's imagine you have the following configuration:
```.editorconfig
[{VisualBasicProject/Module1.vb}]
dotnet_diagnostic.BC40000.severity = suggestion # ^biak^ baseline

[{DerivedClassCS0649.cs}]
dotnet_diagnostic.CS0108.severity = suggestion # ^biak^ baseline

[{ProgramCS0168Warning.cs}]
dotnet_diagnostic.CS0168.severity = suggestion # ^biak^ baseline

[{MyClassCS0169.cs}]
dotnet_diagnostic.CS0169.severity = suggestion # ^biak^ baseline

[{ProgramCS0219Warning.cs}]
dotnet_diagnostic.CS0219.severity = suggestion # ^biak^ baseline

[{ProgramCS0612.cs}]
dotnet_diagnostic.CS0612.severity = suggestion # ^biak^ baseline

[{DerivedClassCS0649.cs}]
dotnet_diagnostic.CS0649.severity = suggestion # ^biak^ baseline

[{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]
dotnet_diagnostic.CS8618.severity = suggestion # ^biak^ baseline
```

Then you fix the warnings in the `DerivedClassCS0649.cs` and `MyTestForlder/MyTestModel1.cs` files.

After running the command, the following output is displayed:
```
warnings-baseline sync started...

DerivedClassCS0649.cs (CS0108, CS0649)
MyTestForlder/MyTestModel1.cs (CS8618)

Sync complete. Removed 2 file(s); resolved 2 filter(s). 6 filter(s) still alive.
```

And the `.editorconfig` file is synchronized.

## ⚙️ Logic
* Find the config file (`.biak/.editorconfig-main`; if it is not found, use `.editorconfig`) or get the file path from `--path <path>`
* Check that the file name starts with `.editorconfig` and the resolved path does not go beyond the root directory.
* Check that the file contains the `# ^biak^ baseline` marker
* Change baseline filters in the config file from `suggestion` to `warning`
* Execute a full project build (30-minute timeout)
* If build errors are detected, terminate execution
* Extract warnings from the generated binary log
* Keep only warnings associated with `.cs` and `.vb` source files
* Sync the config file
* Change baseline filters in the config file from `warning` back to `suggestion`

## 🔗 Links
* Issues: [#107](https://github.com/kurnakovv/biak/issues/107)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/WarningsBaselineSyncCommand.cs)
