## 📙 Description

The `dotnet biak enable` command activates **Biak** configuration in
your project by copying the managed configuration file from
`.biak/.editorconfig-main` back to the root `.editorconfig` (enable all rules in `.editorconfig` file).

This ensures that your project uses the Biak-controlled configuration as
the active EditorConfig file.

## 📌 Overview

When executed inside a properly initialized project directory, the
command:

1.  Verifies that the project has been initialized with
    `dotnet biak setup`.
2.  Ensures that both:
    -   `.biak/.editorconfig-main`
    -   `.editorconfig` exist.
3.  Copies the contents of `.biak/.editorconfig-main` into the root
    `.editorconfig`.
4.  Displays start and completion messages in the console.

If the project is not initialized or required files are missing, the
command safely exits and prints guidance messages.

## 💻 Usage

``` bash
dotnet biak enable
```

### Requirements

-   Must be run from a directory that has already been initialized
    using:

    ``` bash
    dotnet biak setup
    ```

-   The following files must exist:

    -   `.biak/.editorconfig-main`
    -   `.editorconfig`

-   No additional arguments are supported.

## ⚠️ Important Notes

- This command **overwrites** the root `.editorconfig` file.
- Any manual changes previously made directly in `.editorconfig` will be replaced.
- If `.biak` is not initialized, the command will display an error and exit safely.

## 🔗 Links

* Issues: [#23](https://github.com/kurnakovv/biak/issues/23)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/EnableCommand.cs)
