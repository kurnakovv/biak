// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class SetupCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(SetupCommand.IsRunnable([]), "You can't run setup project without `setup` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(SetupCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(SetupCommand.IsRunnable([CommandArgumentConstant.SETUP, "invalid"]), "`setup` + `invalid` is invalid");
        Assert.False(SetupCommand.IsRunnable(["invalid", CommandArgumentConstant.SETUP]), "`invalid` + `setup` is invalid");
        Assert.False(SetupCommand.IsRunnable([CommandArgumentConstant.DISABLE]), "`disable` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForSingleSetupParam()
    {
        Assert.True(SetupCommand.IsRunnable([CommandArgumentConstant.SETUP]));
    }
}
