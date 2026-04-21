## 📙 Description
The ability to search for active branches and files within them where development is ongoing via `dotnet biak find-activity` command. This feature is needed to gradually implement formatting and analyzers without git conflicts by excluding active files from `.editorconfig` file.

## ❔ Why
Applying formatting to the entire project in a large team will lead to a ton of conflicts, especially in legacy projects. The idea is to implement `dotnet format` for most of the project (e.g., ~90% of files + new files), while leaving the remaining active files (the same ~10%) unchanged. Then, run this command again every N amount of time (e.g., once a month) for the remaining files (the same ~10%), and thus gradually implement formatting throughout the entire project (100% of files).

## 💻 Usage
Let's imagine we have a project containing 10 C# files (e.g., `TestService[1..10].cs`) and 3 feature branches (e.g., `f-[1..3]`), where development is taking place (e.g., in the `f-[X]` branch, the file `TestService[X].cs` is being changed, where `X` is the corresponding number). For clarity, let's also add the `change-testservice1` branch, in which we will change `TestService1.cs`.
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
```

Now, if we want to apply formatting, we need to apply it to all files except those currently being developed. Of course, this is easy in this case, since there aren't that many files, but in real projects there can be many more branches and files, and manually adding a filter can be simply impossible.

For such cases, we'll use the command
```
dotnet biak find-activity
```

Next you need to enter
```
Default branch ('main' by default): 
```
The name of the default branch into which all features of the branch will eventually be merged

```
Expiration period in days (default: 30, '*' for unlimited):
```
How many days must pass before we consider a branch active? The branch date is determined by the date of the last commit. I chose 1 month by default, because in my experience, if a branch hasn't been committed for more than a month, it will never be committed again. But you can increase or completely remove this number to be on the safe side.

```
About file types https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB
File types (MDR by default, '*' all files):
```
What we consider activity, specifically which files, is MDR by default:
* Modified - If we've modified a file, conflicts may arise when applying formatting. For example, if you added new code to a method, the method's indentation is incorrect, causing a conflict out of the blue. To avoid such situations, this setting is included in the filter; even if you added a single space to a file, it will still be filtered for reliability.

* Deleted - If a file was deleted in a feature branch, there's no point in formatting it, as it will be deleted and create a conflict because the file doesn't exist, and you're trying to make changes to it (due to formatting).

* Renamed - Some renamed files may contain changes, as Git allows this.

Why not add other types, like `Added`? If a file was added to a feature branch, it won't be modified anywhere else, meaning it doesn't need to be excluded from the filter.

If some filters aren't sufficient for your situation, you can change the filter.

```
File extensions separated by commas ('.cs' by default, '*' all files): 
```
File endings. The dotnet format only works with '.cs' files (it also works with '.vb', but that's a very rare case), so there's no point in using other files. However, if you want to look at the activity of other files, you can specify specific endings, not even file extensions (e.g., ReadMe.md).

```
Exclude specific branches separated by space (e.g., f-1 f-2).
You can use '*' to select multiple similar branches (e.g., f-*).
By default, no additional branches are excluded.
Exclude branches:
```
Specify specific branches you want to exclude from the activity list for various reasons. For example, a branch is experimental and won't be merged into the default branch. You can also specify a set of branches that shouldn't be merged, such as `test-*`, and all branches starting with `test-` won't be filtered.

```
Enter file paths (comma-separated) to process only these files, others will be skipped (default: all files):
```
This option is needed for situations where you've partially implemented `dotnet format` for most files and want to search for activity on the remaining files. Roughly speaking, we've implemented formatting for `TestService[4..10].cs` and want to work with the remaining `TestService[1..3].cs` files. If you're running this for the first time, skip this option. If this isn't your first time, where can I find these files? The easiest way is to go to `.editorconfig`, take the paths from the exceptions like `TestService1,TestService2,TestService3`, and paste them in.

After this, the search will begin, which takes about ~15 seconds, depending on the project, and the final output will be something like this.

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

All active files via variable
^biak^ var activeFiles = "TestService1.cs"
    + ",TestService2.cs"
    + ",TestService3.cs";
```

Where
```
Activity [Current UTC start date]
FullFilePath
[Branches that relate to this file]
```
We need the date so we can remember when we ran this command in the future.
We need branches so we know where this file is changing and which branches need to be merged before we run the command again.

```
Inactive branches
...
```
Branches that are not merged or filtered by the user, but that do not meet the criteria for an active branch, such as a branch that is too old, or a new branch that contains only new files, or does not contain any C# files

```
All active files in single line
...
```
All active files in one line, so that in the future they can be used in the input filter

```
All active files via variable
^biak^ var activeFiles = ...;
```
biak variable ([Docs](Variables)) which can be used together with 🔎 Include / Exclude filter ([Docs](IncludeExcludeFilter))

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
> I created the variable using concatenation. I did this so I could manually remove files from it, eliminating the need to run the command each time. However, you should understand that you do this at your own risk, and it's still better to do it using the command. Also, remember that all active files that are output on a single line will also need to be modified if you plan to run the command.

You can now run `dotnet format` and pull changes into all active branches.

## ⚙️ Logic
* Request the user to enter the required fields
* Get all branches (including remote) that haven't been merged into the default branch
* Filter the retrieved branches if the user specified "exclude branches"
* For each branch, we take the date of the last commit and determine whether it is out of date
* Get the difference in file paths between the feature branch and the default branch, similar to PR
* Filter files by [MDR](https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB)
* Filter files by C# extension or custom extension
* Filter specific files if the user specified "included file paths"
* Print activity in output

## 🔗 Links
* Issues: [#68](https://github.com/kurnakovv/biak/issues/68)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/FindActivityCommand.cs)
* dotnet format: [click](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
