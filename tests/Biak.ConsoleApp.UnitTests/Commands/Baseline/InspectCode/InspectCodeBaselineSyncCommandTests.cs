// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands.Baseline.InspectCode;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands.Baseline.InspectCode;

public class InspectCodeBaselineSyncCommandTests
{
    [Fact]
    public void IsRunnableReturnsFalseForEmptyArgs()
    {
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([]),
            "inspectcode-baseline sync requires at least two arguments"
        );
    }

    [Fact]
    public void IsRunnableReturnsFalseForInvalidArgs()
    {
        Assert.False(InspectCodeBaselineSyncCommand.IsRunnable(["blabla"]), "`blabla` is invalid");
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE]),
            "`inspectcode-baseline` alone is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.SYNC]),
            "`sync` alone is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE, "invalid"]),
            "`inspectcode-baseline invalid` is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable(["invalid", CommandArgumentConstant.SYNC]),
            "`invalid sync` is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC]),
            "`warnings-baseline sync` must not match inspectcode-baseline sync"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                ".editorconfig",
            ]),
            "path without --path option is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
            ]),
            "`--path` without value is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                "--unknown",
                ".editorconfig",
            ]),
            "unsupported option is invalid"
        );
        Assert.False(
            InspectCodeBaselineSyncCommand.IsRunnable([
                CommandArgumentConstant.INSPECTCODE_BASELINE,
                CommandArgumentConstant.SYNC,
                CommandArgumentConstant.PATH,
                "a.editorconfig",
                CommandArgumentConstant.PATH,
                "b.editorconfig",
            ]),
            "duplicate --path options are invalid"
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForValidArgs()
    {
        Assert.True(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE, CommandArgumentConstant.SYNC])
        );

        string[] argsWithPath =
        [
            CommandArgumentConstant.INSPECTCODE_BASELINE,
            CommandArgumentConstant.SYNC,
            CommandArgumentConstant.PATH,
            ".biak/.editorconfig-InspectCodeBaseline",
        ];

        Assert.True(InspectCodeBaselineSyncCommand.IsRunnable(argsWithPath));
    }
}
