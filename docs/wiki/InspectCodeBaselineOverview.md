## 📙 Description
🚧 Inspect Code baseline | New violations fail fast, while legacy violations are isolated in a baseline and fixed incrementally over time.

## ❔ Why

### Problem
In mature projects, Inspect Code can report hundreds or thousands of issues. Fixing everything in one iteration is usually unrealistic.

Without a baseline, teams often postpone cleanup, and technical debt keeps growing.

### Proposed Solution
Use an **inspect code baseline**:

* Existing issues are recorded as `.editorconfig` baseline filters.
* New or changed code is checked against stricter expectations.
* During synchronization, fixed issues are automatically removed from the baseline.
* Teams can reduce debt gradually without blocking delivery.

## 💻 Usage
After running [Init](InspectCodeBaselineInit), insert generated filters into your `.editorconfig` (or baseline file):

```.editorconfig
# Convert property into auto-property [ConvertToAutoProperty] | https://www.jetbrains.com/help/resharper/ConvertToAutoProperty.html
[{ServiceE.cs}]
resharper_convert_to_auto_property_highlighting = suggestion # ^biak^ inspectcode-baseline

# Use 'String.IsNullOrEmpty' [ReplaceWithStringIsNullOrEmpty] | https://www.jetbrains.com/help/resharper/ReplaceWithStringIsNullOrEmpty.html
[{ServiceD.cs}]
resharper_replace_with_string_is_null_or_empty_highlighting = suggestion # ^biak^ inspectcode-baseline

...
```

You can configure behavior via `.biak/config.json`:

```json
{
  "inspectCodeBaseline": {
    "target": "./biak.slnx",
    "path": ".biak/.editorconfig-main",
    "snapshotSeverity": "suggestion",
    "additionalArgs": ["--severity=WARNING"],
    "ruleIdOverrides": {
      "RuleId1": "resharper_your_rule1_editorconfig_key_highlighting"
    }
  }
}
```

> [!WARNING]
> In rare cases, file-specific `.editorconfig` overrides are not detected reliably by the Inspect Code `.editorconfig` parser.
> Unfortunately this cannot be fully fixed from the biak side.
> The practical workaround is to manually fix that small subset of residual rules.

## 🎯 Expected Benefits

* Prevents silent growth of Inspect Code debt.
* Supports gradual cleanup with low operational risk.
* Keeps baseline aligned with the current code state via sync.

## 🔗 Links
* Init ([docs](InspectCodeBaselineInit))
* Sync ([docs](InspectCodeBaselineSync))
