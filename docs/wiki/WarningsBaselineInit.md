## 📙 Description
🚧 Init warnings baseline

## 💻 Usage

```
dotnet biak warnings-baseline init [--target <path>]
```

After a few moments, something similar should appear:
```
Choose one: add this full snippet to `Directory.Build.props`, or if you configure in a `.csproj` file(s), add only `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` inside an existing `<PropertyGroup>`.
<Project>
	<PropertyGroup>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
	</PropertyGroup>
</Project>

Insert these filters into your .editorconfig file
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

> [!WARNING]
> Do not remove the `# ^biak^ baseline` marker, as it is used for the [Sync](WarningsBaselineSync) command.

## ⚙️ Logic
* Execute a full project build (30-minute timeout), or build an explicit target from `--target <path>`
* If build errors are detected, terminate execution
* Extract warnings from the generated binary log
* Retain only warnings associated with `.cs` and `.vb` source files
* If no applicable warnings are found, terminate execution
* Generate and output the corresponding `.editorconfig` baseline entries

## ❔ Questions

### Why `.cs` / `.vb` extensions only?
If a warning originates from a global project-level configuration, such as a `.csproj` setting (e.g., NU1901), it cannot be configured through an `.editorconfig` rule. In the case of F#, warning filtering via `.editorconfig` is not supported ([SOF](https://stackoverflow.com/questions/3740566/f-suppress-warnings)). Similarly, warning suppression for `.cshtml` (Razor Pages) is subject to limitations, and certain warnings cannot be disabled ([SOF](https://stackoverflow.com/questions/72488529/visual-studio-does-not-suppress-mvc1000-warning)).

### Why `suggestion` and not `warning`?

Unfortunately, if you write `... = warning # ^biak^ baseline` instead of `... = suggestion # ^biak^ baseline` and set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, the filter will not be applied and all warnings will become errors.

## 🔗 Links
* Issues: [#105](https://github.com/kurnakovv/biak/issues/105), [#113](https://github.com/kurnakovv/biak/issues/113)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/WarningsBaselineInitCommand.cs)
