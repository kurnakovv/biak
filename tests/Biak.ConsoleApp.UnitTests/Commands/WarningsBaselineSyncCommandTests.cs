// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class WarningsBaselineSyncCommandTests
{
    [Fact]
    public void IsRunnableReturnsFalseForEmptyArgs()
    {
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([]),
            "warnings-baseline sync requires at least two arguments"
        );
    }

    [Fact]
    public void IsRunnableReturnsFalseForInvalidArgs()
    {
        Assert.False(WarningsBaselineSyncCommand.IsRunnable(["blabla"]), "`blabla` is invalid");
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE]),
            "`warnings-baseline` alone is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.SYNC]),
            "`sync` alone is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, "invalid"]),
            "`warnings-baseline invalid` is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable(["invalid", CommandArgumentConstant.SYNC]),
            "`invalid sync` is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                ".editorconfig",
            ]),
            "path without --path option is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
            ]),
            "`--path` without value is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                ".editorconfig",
                "extra",
            ]),
            "third argument must be an option"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.TARGET,
            ]),
            "`--target` without value is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                "a.editorconfig",
                CommandArgumentConstant.PATH,
                "b.editorconfig",
            ]),
            "duplicate options are invalid"
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForDefaultArguments()
    {
        Assert.True(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC])
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForSupportedOptions()
    {
        Assert.True(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, CommandArgumentConstant.PATH, "path/to/.editorconfig"])
        );

        Assert.True(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, CommandArgumentConstant.TARGET, "path/to/app.slnx"])
        );

        string[] pathThenTarget =
        [
            CommandArgumentConstant.WARNINGS_BASELINE,
            CommandArgumentConstant.SYNC,
            CommandArgumentConstant.PATH,
            ".editorconfig",
            CommandArgumentConstant.TARGET,
            "path/to/app.csproj",
        ];
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(pathThenTarget));

        string[] targetThenPath =
        [
            CommandArgumentConstant.WARNINGS_BASELINE,
            CommandArgumentConstant.SYNC,
            CommandArgumentConstant.TARGET,
            "path/to/app.csproj",
            CommandArgumentConstant.PATH,
            ".editorconfig",
        ];
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(targetThenPath));
    }
}
