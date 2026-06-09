// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class WarningsBaselineSyncCommandTests
{
    [Fact]
    public void IsRunnable_ReturnsFalseForEmptyArgs()
    {
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([]),
            "warnings-baseline sync requires three arguments"
        );
    }

    [Fact]
    public void IsRunnable_ReturnsFalseForInvalidArgs()
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
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, "invalid", ".editorconfig"]),
            "`warnings-baseline invalid .editorconfig` is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable(["invalid", CommandArgumentConstant.SYNC, ".editorconfig"]),
            "`invalid sync .editorconfig` is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]),
            "`warnings-baseline sync` without a path is invalid"
        );
        Assert.False(
            WarningsBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.WARNINGS_BASELINE,
                CommandArgumentConstant.SYNC,
                ".editorconfig",
                "extra",
            ]),
            "extra argument makes it invalid"
        );
    }

    [Fact]
    public void IsRunnable_ReturnsTrueForValidArgs()
    {
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, ".editorconfig"])
        );
    }

    [Fact]
    public void IsRunnable_ReturnsTrueForAnyEditorConfigPath()
    {
        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, "path/to/.editorconfig"])
        );

        Assert.True(WarningsBaselineSyncCommand.IsRunnable(
            [CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC, "some-file.txt"])
        );
    }
}
