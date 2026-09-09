## 📙 Description
🚧 Init inspect code baseline

## 💻 Usage

```
dotnet biak inspectcode-baseline init
```

After a few moments, something similar should appear:
```
inspectcode-baseline init started...

Insert these filters into your .editorconfig file:
# Convert property into auto-property [ConvertToAutoProperty] | https://www.jetbrains.com/help/resharper/ConvertToAutoProperty.html
[{ServiceE.cs}]
resharper_convert_to_auto_property_highlighting = suggestion # ^biak^ inspectcode-baseline

# Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
[{ServiceD.cs}]
resharper_replace_with_string_is_null_or_empty_highlighting = suggestion # ^biak^ inspectcode-baseline
```

> [!WARNING]
> Do not remove the `# ^biak^ inspectcode-baseline` marker, as it is used by the [Sync](InspectCodeBaselineSync) command.

## ⚙️ Logic
* Run Inspect Code and produce a SARIF report.
  * Candidate launch order: `dotnet tool run jb inspectcode` -> `jb inspectcode` -> `InspectCode.exe` -> `inspectcode`.
  * Timeout: 30 minutes.
* Resolve target automatically from current directory (`*.slnx` -> `*.sln` -> `*.csproj`) or from `.biak/config.json` (`inspectCodeBaseline.target`).
* If `inspectCodeBaseline.debugMode` is `true`:
  * print each attempted InspectCode candidate command (in launch order)
  * print the generated SARIF path (when the file exists)
  * keep the generated SARIF file in `.biak/logs/inspectcode-baseline`
* If `inspectCodeBaseline.debugMode` is `false`, delete the generated SARIF after processing.
* Parse SARIF issues (`ruleId` + file locations).
* Map `ruleId` to `.editorconfig` key using built-in metadata.
* Apply optional `inspectCodeBaseline.ruleIdOverrides` from config.
* Group issues by mapped key and generate baseline filters.
* Print warning for unmapped rules and suggest local override config.

## ❔ Questions

### Why are some rules skipped?
JetBrains does not provide a public API for InspectCode rule metadata, so this project stores rule key/value mappings directly in code. Because of that, mapping data may be incomplete or outdated, and if a `ruleId` is not found in the built-in mapping and not provided in `ruleIdOverrides`, it cannot be converted to a valid `.editorconfig` key and is skipped.

## 🔗 Links
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/Baseline/InspectCode/InspectCodeBaselineInitCommand.cs)
* Issues: [#123](https://github.com/kurnakovv/biak/issues/123)
