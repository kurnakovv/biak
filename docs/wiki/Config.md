# 🔧 Config

You can configure **biak** behavior via the `.biak/config.json` file.

```json
{
  "severityWhenDisabled": "none"
}
```

| Name                   | Available                                                     | Default | Description                                                                                            |
| ---------------------- | ------------------------------------------------------------- | ------- | ------------------------------------------------------------------------------------------------------ |
| `severityWhenDisabled` | `none`, `error`, `warning`, `suggestion`, `silent`, `default` | `none`  | Defines the severity level applied to rules disabled using the `dotnet biak disable` command.          |

## 🗒️ Notes
* You can use biak without a configuration file - default values will be used.
* Some settings can be configured, but biak works without any configuration.

## 🔗 Links

* Issues: [#43](https://github.com/kurnakovv/biak/issues/43)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Models/BiakConfig.cs)
