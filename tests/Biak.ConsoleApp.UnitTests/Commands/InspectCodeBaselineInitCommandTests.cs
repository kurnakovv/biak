// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class InspectCodeBaselineInitCommandTests
{
    [Fact]
    public void IsRunnableReturnsFalseForEmptyArgs()
    {
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable([]),
            "inspectcode-baseline init requires at least two arguments"
        );
    }

    [Fact]
    public void IsRunnableReturnsFalseForInvalidArgs()
    {
        Assert.False(InspectCodeBaselineInitCommand.IsRunnable(["blabla"]), "`blabla` is invalid");
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE]),
            "`inspectcode-baseline` alone is invalid"
        );
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable([CommandArgumentConstant.INIT]),
            "`init` alone is invalid"
        );
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE, "invalid"]),
            "`inspectcode-baseline invalid` is invalid"
        );
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable(["invalid", CommandArgumentConstant.INIT]),
            "`invalid init` is invalid"
        );
        Assert.False(
            InspectCodeBaselineInitCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.INIT]),
            "`warnings-baseline init` must not match inspectcode-baseline init"
        );
    }

    [Fact]
    public void IsRunnableReturnsTrueForValidArgs()
    {
        Assert.True(
            InspectCodeBaselineInitCommand.IsRunnable([CommandArgumentConstant.INSPECTCODE_BASELINE, CommandArgumentConstant.INIT])
        );
    }
}
