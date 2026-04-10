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
  "onImportFailure": "warning"
}
```

| Name                   | Available                                                         | Default                               | Description                                                                                                                                              |
| ---------------------- | ----------------------------------------------------------------- | ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `severityWhenDisabled` | `none`, `error`, `warning`, `suggestion`, `silent`, `default`     | `none`                                | Defines the severity level applied to rules disabled using the `dotnet biak disable` command.                                                            |
| `severitiesToDisable`  | `["none", "error", "warning", "suggestion", "silent", "default"]` | `["error", "warning", "suggestion"]`  | Specifies which analyzer severity levels are replaced when running the `dotnet biak disable` command.                                                    |
| `onImportFailure`      | `nothing`, `warning`, `error`                                     | `warning`                             | A field that allows you to control the behavior of the biak when it is not possible to substitute the import value for various reasons. ([docs](Import)) |

## 🗒️ Notes
* You can use biak without a configuration file - default values will be used.
* Some settings can be configured, but biak works without any configuration.

## 🔗 Links

* Issues: [#43](https://github.com/kurnakovv/biak/issues/43) | [#66](https://github.com/kurnakovv/biak/issues/66)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Models/BiakConfig.cs)
