## 📙 Description
🚧 Sync inspect code baseline | Removes baseline filters when matching Inspect Code issues are no longer present in the corresponding files.

## 💻 Usage

```
dotnet biak inspectcode-baseline sync [--path <path>]
```

Let's imagine you have the following configuration:
```.editorconfig
# Field can be made readonly (private accessibility) [FieldCanBeMadeReadOnly.Local] | https://www.jetbrains.com/help/resharper/FieldCanBeMadeReadOnly.Local.html
[{ServiceA.cs,ServiceC.cs}]
resharper_field_can_be_made_read_only_local_highlighting = suggestion # ^biak^ inspectcode-baseline

# Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
[{ServiceD.cs}]
resharper_replace_with_string_is_null_or_empty_highlighting = suggestion # ^biak^ inspectcode-baseline
```

Then you fix the issue in `ServiceD.cs`.

After running the command, the following output is displayed:
```
inspectcode-baseline sync started...

ServiceD.cs (resharper_replace_with_string_is_null_or_empty_highlighting)

Sync complete. Removed 1 file(s); resolved 1 filter(s). 1 filter(s) still alive.
```

And your baseline file is synchronized.

## ⚙️ Logic
* Require root `.editorconfig` in the current directory (used for runtime analysis).
* If `.biak` exists, require synchronized biak state (`enable` or `disable`).
* Resolve baseline file path with precedence:
  * `--path <path>`
  * `inspectCodeBaseline.path` from `.biak/config.json`
  * auto-discovery in `.biak/.editorconfig*` files containing marker
  * root `.editorconfig` containing marker
* Validate path safety (must stay inside project and file name must start with `.editorconfig`).
* Verify that baseline marker exists: `# ^biak^ inspectcode-baseline`.
* Temporarily prepare runtime `.editorconfig` for analysis:
  * baseline entries are converted to `error` (marker removed at runtime copy)
  * original root `.editorconfig` is restored after analysis
* Run Inspect Code and parse active issues from SARIF.
* Remove resolved filters / stale file entries from baseline.
* Normalize remaining baseline severity to `inspectCodeBaseline.snapshotSeverity` (default `suggestion`).

## 🔗 Links
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/Baseline/InspectCode/InspectCodeBaselineSyncCommand.cs)
* Issues: [#123](https://github.com/kurnakovv/biak/issues/123)
