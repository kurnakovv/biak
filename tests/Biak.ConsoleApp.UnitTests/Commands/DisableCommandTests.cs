// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class DisableCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(DisableCommand.IsRunnable([]), "You can't run disable command without `disable` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(DisableCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(DisableCommand.IsRunnable([CommandArgumentConstant.DISABLE, "invalid"]), "`disable` + `invalid` is invalid");
        Assert.False(DisableCommand.IsRunnable(["invalid", CommandArgumentConstant.DISABLE]), "`invalid` + `disable` is invalid");
        Assert.False(DisableCommand.IsRunnable([CommandArgumentConstant.SETUP]), "`setup` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForSingleDisableParam()
    {
        Assert.True(DisableCommand.IsRunnable([CommandArgumentConstant.DISABLE]));
    }
}
