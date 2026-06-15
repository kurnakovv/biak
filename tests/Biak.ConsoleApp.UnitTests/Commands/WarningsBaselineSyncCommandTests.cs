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
            "third argument must be --path"
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForDefaultArguments()
    {
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC])
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForPathOption()
    {
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, CommandArgumentConstant.PATH, "path/to/.editorconfig"])
        );

        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, CommandArgumentConstant.PATH, "some-file.txt"])
        );
    }
}
