// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class WarningsBaselineInitCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(WarningsBaselineInitCommand.IsRunnable([]), "You can't run warnings-baseline init command without `warnings-baseline` + `init` arguments");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(WarningsBaselineInitCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(WarningsBaselineInitCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE]), "`warnings-baseline` without `init` is invalid");
        Assert.False(WarningsBaselineInitCommand.IsRunnable([CommandArgumentConstant.INIT]), "`init` without `warnings-baseline` is invalid");
        Assert.False(WarningsBaselineInitCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, "invalid"]), "`warnings-baseline` + `invalid` is invalid");
        Assert.False(WarningsBaselineInitCommand.IsRunnable(["invalid", CommandArgumentConstant.INIT]), "`invalid` + `init` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForWarningsBaselineInitParams()
    {
        Assert.True(WarningsBaselineInitCommand.IsRunnable([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.INIT]));
    }
}
