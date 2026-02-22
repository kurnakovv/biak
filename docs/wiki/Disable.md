## 📙 Description

The `dotnet biak disable` command takes the contents of `editorconfig-main`, disables all rules (error|warning|suggestion -> none) and inserts them into `.editorconfig`

## 📌 Overview

When executed inside a properly initialized project directory, the
command:

1.  Verifies that the project has been initialized with
    `dotnet biak setup`.
2.  Ensures that both:
    -   `.biak/.editorconfig-main`
    -   `.editorconfig` exist.
3. Takes the content from `.biak/.editorconfig-main`.
4. Disables all rule severities (error | warning | suggestion -> none), for example:
    ```.editorconfig
    [*.cs]
    # Before
    dotnet_diagnostic.CA2000.severity = error
    dotnet_diagnostic.CA1001.severity = warning
    dotnet_diagnostic.CA1707.severity = suggestion

    # After
    dotnet_diagnostic.CA2000.severity = none
    dotnet_diagnostic.CA1001.severity = none
    dotnet_diagnostic.CA1707.severity = none
    ```
5. Inserts the modified content into `.editorconfig`.

If the project is not initialized or required files are missing, the
command safely exits and prints guidance messages.

## 💻 Usage

``` bash
dotnet biak disable
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

* Issues: [#13](https://github.com/kurnakovv/biak/issues/13)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/DisableCommand.cs)
