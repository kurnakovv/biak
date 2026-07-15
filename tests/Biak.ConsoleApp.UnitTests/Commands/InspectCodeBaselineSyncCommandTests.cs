// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

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
    }

    [Fact]
    public void IsRunnableReturnsTrueForValidArgs()
    {
        Assert.True(
            InspectCodeBaselineSyncCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE, CommandArgumentConstant.SYNC])
        );
    }
}
