# Status

The `dotnet biak status` command returns the current Biak mode for the root `.editorconfig`.

## Usage

```bash
dotnet biak status
```

## Output

```bash
on
```

`on` means the current `.editorconfig` matches the enabled Biak configuration.

```bash
off
```

`off` means the current `.editorconfig` does not match the enabled Biak configuration, for example after `dotnet biak disable`.
