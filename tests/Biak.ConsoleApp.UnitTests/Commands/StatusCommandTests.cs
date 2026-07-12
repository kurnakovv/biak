// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class StatusCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(StatusCommand.IsRunnable([]), "You can't run status command without `status` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(StatusCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(StatusCommand.IsRunnable([CommandArgumentConstant.STATUS, "invalid"]), "`status` + `invalid` is invalid");
        Assert.False(StatusCommand.IsRunnable(["invalid", CommandArgumentConstant.STATUS]), "`invalid` + `status` is invalid");
        Assert.False(StatusCommand.IsRunnable([CommandArgumentConstant.SETUP]), "`setup` is invalid");
        Assert.False(StatusCommand.IsRunnable([CommandArgumentConstant.DEBUG_INFO, CommandArgumentConstant.STATUS]), "`--debug-info` + `status` is invalid");
        Assert.False(StatusCommand.IsRunnable([CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO, "extra"]), "`status --debug-info` with extra argument is invalid");
    }

    [Fact]
    public void IsRunnableTrueForValidParams()
    {
        Assert.True(StatusCommand.IsRunnable([CommandArgumentConstant.STATUS]));
        Assert.True(StatusCommand.IsRunnable([CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO]));
    }
}
