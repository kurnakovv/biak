## 📙 Description

The `dotnet biak setup` command initializes **Biak** environment in your current project directory by creating a dedicated configuration folder and copying your existing `.editorconfig` into it.

## 📌 Overview

When executed inside a project directory, the command:

1. Verifies that a `.editorconfig` file exists.
2. Creates a `.biak` folder.
3. Copies `.editorconfig` into `.biak/.editorconfig-main`.
4. Optionally recreates the `.biak` folder if it already exists.

## 💻 Usage

```bash
dotnet biak setup
```

### Requirements

- Must be run from a directory that contains a `.editorconfig` file.
- No additional arguments are supported.

## 📂 Resulting Directory Structure

After a successful run:

```
your-project/
│
├── .editorconfig
└── .biak/
    └── .editorconfig-main
```

## ⚠️ Warning

After running `dotnet biak setup`, the root `.editorconfig` should be treated as **read-only** for Biak configuration purposes.

Do not modify project structure or Biak-specific configuration inside `.editorconfig`. All structural or Biak-related changes should now be made in `.biak/.editorconfig-main`.

## 🔗 Links
* Issues: [#10](https://github.com/kurnakovv/biak/issues/10)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/SetupCommand.cs)
