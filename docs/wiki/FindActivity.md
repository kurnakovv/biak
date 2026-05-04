## 📙 Description
Provides the ability to find active branches and files being modified within them using the `dotnet biak find-activity` command. This feature helps gradually introduce formatting and analyzers without causing Git conflicts by excluding actively modified files from the `.editorconfig` file.

## ❔ Why
Applying formatting across an entire project in a large team often leads to many merge conflicts, especially in legacy projects. The idea is to apply `dotnet format` to most of the project (e.g., ~90% of files + new files), while leaving the remaining files (the same ~10%) unchanged. Then, rerun this command periodically (e.g., once a month) for the remaining files (the same ~10%), and thus gradually apply formatting across the entire project (100% of files).

## 💻 Usage
Let's imagine a project containing 10 C# files (e.g., `TestService[1..10].cs`) and 3 feature branches (e.g., `f-[1..3]`), and active feature branches (e.g., in the `f-[X]` branch, the file `TestService[X].cs` is being modified). For clarity, let's also add the `change-testservice1` branch, where `TestService1.cs` is modified.
```
> ls
* TestService1.cs
* TestService2.cs
* TestService3.cs
* TestService4.cs
* TestService5.cs
* TestService6.cs
* TestService7.cs
* TestService8.cs
* TestService9.cs
* TestService10.cs
...
```

```
> git branch
* f-1 # (Changed file TestService1)
* f-2 # (Changed file TestService2)
* f-3 # (Changed file TestService3)
* change-testservice1 # (Changed file TestService1)
...
```

Now, if we want to apply formatting, we need to apply it to all files except those currently being developed. In small projects this is manageable manually, since there aren't that many files, but in real projects there can be many more branches and files, and manually adding a filter can be simply impossible.

For such cases, we'll use the command
```
dotnet biak find-activity
```

Next, you need to enter
```
Default branch ('main' by default): 
```
The branch into which all feature branches are eventually merged

---

```
Expiration period in days (default: 30, '*' for unlimited):
```
Defines how long a branch is considered active based on its last commit date.

By default, 30 days is used. In practice, branches that have been inactive for more than a month are often abandoned, but you can increase this value or disable it entirely (`*`) if needed.

---

```
About file types https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB
File types (MDR by default, '*' all files):
```
What we consider activity, specifically which files, is MDR by default:
* Modified (M) - even minor changes (e.g., whitespace) can lead to formatting conflicts, so these files are excluded.

* Deleted (D) - formatting deleted files is unnecessary and may lead to conflicts.

* Renamed (R) - renamed files may still contain changes.

Why not include **Added (A)**?

New files typically don't cause formatting conflicts, since they are not modified elsewhere.

You can customize this filter if needed.

---

```
File extensions separated by commas ('.cs' by default, '*' all files): 
```
Specifies which file types should be analyzed.

`dotnet format` primarily targets `.cs` files (and optionally `.vb`), so other file types are usually unnecessary. However, you can include any file types if needed (e.g., `README.md`).

---

```
Exclude specific branches separated by space (e.g., f-1 f-2).
You can use '*' to select multiple similar branches (e.g., f-*).
By default, no additional branches are excluded.
Exclude branches:
```
Specify branches you want to exclude from the activity list for various reasons. For example, if a branch is experimental and won't be merged into the default branch. You can also specify a set of branches that shouldn't be merged, such as `test-*`, and all branches starting with `test-` won't be filtered.

---

```
Enter file paths (comma-separated) to process only these files, others will be skipped (default: all files):
```
Used when formatting has already been partially applied.

Example:
If formatting has already been applied to `TestService[4..10].cs`, you can limit the scan to `TestService[1..3].cs`.

If this is your first run, leave this empty.

If not, you can copy excluded file paths from `.editorconfig` and reuse them here.

---

After this, the search will begin, which takes ~15 seconds, depending on the project, and the final output will be something like this.

```
Start find activity...
Activity [4/20/2026 12:38:25 PM]
TestService1.cs
[change-testservice1 test-f-1]

TestService2.cs
[test-f-2]

TestService3.cs
[test-f-3]


Inactive branches
f-new-cs-file no-cs-file-changes old-branch

All active files in single line
TestService1.cs,TestService2.cs,TestService3.cs

All active files in single line for `dotnet format --exclude ...` command
TestService1.cs TestService2.cs TestService3.cs

All active files via variable
^biak^ var activeFiles = "TestService1.cs"
    + ",TestService2.cs"
    + ",TestService3.cs";
```

Where:
```
Activity [Current UTC start date]
FullFilePath
[Branches that relate to this file]
```
The date is needed so we can remember when the command was run.
Branches are listed so we know where the file is being changed and which branches need to be merged before we run the command again.

---

```
Inactive branches
...
```
Branches that are not merged or filtered by the user, but do not meet the criteria for being considered active, such as a branch that is too old, or a new branch that contains only new files, or does not contain any C# files

---

```
All active files in single line
...
```
All active files in one line, so they can be reused in the input filter later

---

```
All active files in single line for `dotnet format --exclude ...` command
...
```
Filter for the `dotnet format`, for example, `dotnet format --exclude TestService1.cs TestService2.cs TestService3.cs` If you simply run the dotnet format, it will apply to all files, even if you specify a limitation in `.editorconfig`, although everything will work correctly for the build and in IDE.

---

```
All active files via variable
^biak^ var activeFiles = ...;
```
A biak variable ([Docs](Variables)) which can be used together with 🔎 Include / Exclude filter ([Docs](IncludeExcludeFilter))

.editorconfig-main
```.editorconfig
^biak^ var activeFiles = "TestService1.cs"
    + ",TestService2.cs"
    + ",TestService3.cs";

^biak^ include [*.cs]
^biak^ exclude [{$activeFiles}]

dotnet_diagnostic.SA1025.severity = error
dotnet_diagnostic.SA1026.severity = error
dotnet_diagnostic.SA1028.severity = error

^biak^ END include/exclude
```

After running the [Enable](Enable) / [Disable](Disable) commands, the `.editorconfig` file is converted to:
```.editorconfig
[*.cs]
dotnet_diagnostic.SA1025.severity = error
dotnet_diagnostic.SA1026.severity = error
dotnet_diagnostic.SA1028.severity = error

[TestService1.cs,TestService2.cs,TestService3.cs]
dotnet_diagnostic.SA1025.severity = none
dotnet_diagnostic.SA1026.severity = none
dotnet_diagnostic.SA1028.severity = none
```

> [!NOTE]
> The variable was created using concatenation. I did this so I could manually remove files from it, eliminating the need to run the command each time. However, this should be done at your own risk, and it's still better to use the command. Also, remember that all active files that are output on a single line will also need to be modified if you plan to run the command.

You can now run `dotnet format --exclude ...` and pull the changes into all active branches.

> [!NOTE]
> You can automate the input parameters through the config ([Docs](Config)).

## ⚙️ Logic
* Prompt the user to enter the required fields
* Get all branches (including remote) that haven't been merged into the default branch
* Apply user-defined branch exclusions
* Determine branch activity based on the last commit date
* Compare each branch with the default branch (similar to a PR diff)
* Filter files by [MDR](https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB)
* Filter by file extensions
* Optionally filter by user-provided file paths
* Output the detected activity

## 🔗 Links
* Issues: [#68](https://github.com/kurnakovv/biak/issues/68)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/FindActivityCommand.cs)
* dotnet format: [click](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
