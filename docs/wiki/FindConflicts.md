## 📙 Description
⚔️ Find files with merge conflicts between the default branch and selected branches.

## ❔ Why
This command is very useful when combined with 🧑‍💻 Find activity ([docs](FindActivity)) to ensure that applying filters to the default branch doesn't cause any conflicts with active branches. This command can also be used to automatically find conflicts with other branches.

## 💻 Usage
Let's imagine that we have the same conditions as described [here](FindActivity#-usage). Now let's run `dotnet format` for the entire project and commit it.

### Input

> [!NOTE]
> Before running this command, make sure your team has pushed all recent changes to the remote repository, and run `git fetch origin` to ensure you have the latest state of all active branches.

To find conflicts in the branches we need, let's run this command
```
dotnet biak find-conflicts
```

Next, you need to enter
```
Default branch ('main' by default): 
```
The branch into which the specified branches will be merged

---

```
Branches separated by spaces that will be merged with no commit into the default branch (e.g., f-1 f-2): 
```
Branches you want to compare with the default branch to detect conflicts. I recommend taking these from the Active branches section of the [docs](FindActivity#output). In our case `f-1 f-2 f-3`

### Output
```
Start find conflicts command...
Conflicting files [5/15/2026 8:06:51 AM]
TestService1.cs
[f-1]

TestService2.cs
[f-2]

TestService3.cs
[f-3]
```
Now you can look at the conflicting files and change the filter or simply revert the changes to the default branch.

## ⚙️ Logic
* Prompt the user to enter the required fields
* Checkout the default branch
* Check for not found selected branches
* Merge every branch into the default branch without commit
* Get conflicting files
* Abort merge
* Checkout the original branch

## 🔗 Links
* Issues: [#93](https://github.com/kurnakovv/biak/issues/93)
* Source code: [click](https://github.com/kurnakovv/biak/blob/dev/src/Biak.ConsoleApp/Commands/FindConflictsCommand.cs)
* dotnet format: [click](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
