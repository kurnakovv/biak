## 📙 Description
🚧 Init warnings baseline

## 💻 Usage

```
dotnet biak warnings-baseline init
```

After a few moments, something similar should appear:
```
Add this configuration to Directory.Build.props or to all .csproj files
<Project>
	<PropertyGroup>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
	</PropertyGroup>
</Project>

Insert this filters to your .editorconfig file
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

## ⚙️ Logic
* Execute a full project build (30-minute timeout)
* If build errors are detected, terminate execution
* Extract warnings from the generated binary log
* Retain only warnings associated with `.cs` and `.vb` source files
* If no applicable warnings are found, terminate execution
* Generate and output the corresponding `.editorconfig` baseline entries

## Why `.cs` / `.vb` extensions only?
If a warning originates from a global project-level configuration, such as a `.csproj` setting (e.g., NU1901), it cannot be configured through an `.editorconfig` rule. In the case of F#, warning filtering via `.editorconfig` is not supported ([SOF](https://stackoverflow.com/questions/3740566/f-suppress-warnings)). Similarly, warning suppression for `.cshtml` (Razor Pages) is subject to limitations, and certain warnings cannot be disabled ([SOF](https://stackoverflow.com/questions/72488529/visual-studio-does-not-suppress-mvc1000-warning)).

## 🔗 Links
* Issues: [#105](https://github.com/kurnakovv/biak/issues/105)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/WarningsBaselineInitCommand.cs)
