## 📙 Description
* 🚧 Warnings baseline | Build a warning baseline from existing compiler and analyzer warnings, allowing [TreatWarningsAsErrors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#treatwarningsaserrors) to be enabled without fixing all legacy violations at once.

## ❔ Why

### Problem
In large codebases, it is common to accumulate thousands of compiler or analyzer warnings (e.g., 3,000+). Resolving all of them at once is often unrealistic.

As a result, developers may become desensitized to new warnings. When a new warning appears during development, the typical reaction is:

> "There are already thousands of warnings in the project - one more won't make a difference."

This mindset causes technical debt to grow continuously, making future cleanup efforts even more difficult.

### Proposed Solution

Introduce a **warnings baseline** mechanism that allows existing warnings to remain unchanged while enforcing stricter rules for all new code.

The idea is:

* Existing files that currently contain warnings remain in a baseline list.
* All other files are treated with **Warnings as Errors**.
* New warnings introduced outside of the baseline immediately fail the build.
* Teams can gradually reduce technical debt without requiring a massive one-time cleanup.

## 💻 Usage
After running [Init](WarningsBaselineInit) command, insert output filters to your `.editorconfig` file

```.editorconfig
[{path/to/file1.cs,path/to/file2.cs}]
code1 = suggestion # ^biak^ baseline

[{path/to/file3.cs}]
code2 = suggestion # ^biak^ baseline

...
```

Choose one: add this full snippet to `Directory.Build.props` ([what?](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=visualstudio) / [my favorite](https://gist.github.com/kurnakovv/160520da1054eb32f235f44c75a5c804)), or if you configure in a `.csproj` file(s), add only `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` inside an existing `<PropertyGroup>`.
```csproj
<Project>
	<PropertyGroup>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
	</PropertyGroup>
</Project>
```

## 🎯 Expected Benefits

* Prevents introduction of new warnings while preserving the current state of the codebase.
* Enables gradual and manageable warning cleanup.
* Reduces long-term technical debt.
* Encourages teams to maintain a warning-free standard for new code.
* Can be adopted incrementally without disrupting existing development workflows.

## 🔗 Links
* Init ([docs](WarningsBaselineInit))
