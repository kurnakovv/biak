// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class EnableCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(EnableCommand.IsRunnable([]), "You can't run enable command without `enable` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(EnableCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(EnableCommand.IsRunnable([CommandArgumentConstant.ENABLE, "invalid"]), "`enable` + `invalid` is invalid");
        Assert.False(EnableCommand.IsRunnable(["invalid", CommandArgumentConstant.ENABLE]), "`invalid` + `enable` is invalid");
        Assert.False(EnableCommand.IsRunnable([CommandArgumentConstant.SETUP]), "`setup` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForSingleEnableParam()
    {
        Assert.True(EnableCommand.IsRunnable([CommandArgumentConstant.ENABLE]));
    }
}
