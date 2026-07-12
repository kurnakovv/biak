## 📙 Description

The `dotnet biak status` command returns the current biak mode for the root `.editorconfig`.

## 💻 Usage

```bash
dotnet biak status [--debug-info]
```

After executing this command, you may see one of the following outputs:

* **enabled** - means the current `.editorconfig` matches the enabled biak configuration.
* **disabled** - means the current `.editorconfig` matches the disabled biak configuration.
* **broken** - Something is incorrectly configured for the biak configuration, for example, the `.editorconfig` file was not found or other reasons that can be found using the `--debug-info` flag.

## ❔ Why
* Provide the necessary functionality for an IDE editor UI button that displays the current biak status and enables users to toggle it directly from the editor. This removes the need to use the CLI and simplifies the development workflow.

* Automatically detect the current biak status, eliminating the need for manual checks to determine whether biak is enabled.

## 🔗 Links

* Issues: [#117](https://github.com/kurnakovv/biak/issues/117)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/StatusCommand.cs)
