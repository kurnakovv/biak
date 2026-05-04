# 🔧 Config

You can configure **biak** behavior via the `.biak/config.json` file.

```json
{
  "severityWhenDisabled": "none",
  "severitiesToDisable": [
    "error",
    "warning",
    "suggestion"
  ],
  "onImportFailure": "warning",
  "findActivity": {
    "defaultBranch": "main",
     "expirationPeriod": "30",
     "fileTypes": "MDR",
     "fileExtensions": ".cs,.vb",
     "excludeBranches": "test *-experimental my-test-branch",
     "includedFilePaths": "TestService1.cs,TestService2.cs,TestService3.cs",
  }
}
```

| Name                             | Available                                                                                                     | Default                               | Description                                                                                                                                                    |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------- | ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `severityWhenDisabled`           | `none`, `error`, `warning`, `suggestion`, `silent`, `default`                                                 | `none`                                | Defines the severity level applied to rules disabled using the `dotnet biak disable` command.                                                                  |
| `severitiesToDisable`            | `["none", "error", "warning", "suggestion", "silent", "default"]`                                             | `["error", "warning", "suggestion"]`  | Specifies which analyzer severity levels are replaced when running the `dotnet biak disable` command.                                                          |
| `onImportFailure`                | `nothing`, `warning`, `error`                                                                                 | `warning`                             | Controls what happens when an import cannot be resolved, for example because a file is missing or a URL is blocked or unreachable. ([docs](Import))            |
| `findActivity:defaultBranch`     | Any                                                                                                           | ---                                   | The branch into which all feature branches are eventually merged.                                                                                              |
| `findActivity:expirationPeriod`  | Positive number or '*' for no limit                                                                           | ---                                   | Defines how long a branch is considered active based on its last commit date.                                                                                  |
| `findActivity:fileTypes`         | Any file type's ([docs](https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB)) | ---                                   | What we consider activity, specifically which files.                                                                                                           |
| `findActivity:fileExtensions`    | Comma-separated file extensions strings                                                                       | ---                                   | Specifies which file types should be analyzed.                                                                                                                 |
| `findActivity:excludeBranches`   | Space-separated exclude branches strings                                                                      | ---                                   | Exclude specific branches separated by space (e.g., f-1 f-2). You can use '*' to select multiple similar branches (e.g., f-*). Use empty string for no exclude |
| `findActivity:includedFilePaths` | Comma-separated include file paths strings                                                                    | ---                                   | Find activity only for this files. Used when formatting has already been partially applied. Or use empty string for all files                                  |

## 🗒️ Notes
* You can use biak without a configuration file - default values will be used.
* Some settings can be configured, but biak works without any configuration.
* All `findActivity:...` settings are needed so that you don't have to enter them every time you run the `dotnet biak find-activity` command ([docs](FindActivity)).

## 🔗 Links

* Issues: [#43](https://github.com/kurnakovv/biak/issues/43) | [#66](https://github.com/kurnakovv/biak/issues/66) | [#81](https://github.com/kurnakovv/biak/issues/81)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Models/BiakConfig.cs)
