// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class FindActivityCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(FindActivityCommand.IsRunnable([]), "You can't run enable command without `enable` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(FindActivityCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(FindActivityCommand.IsRunnable([CommandArgumentConstant.FIND_ACTIVITY, "invalid"]), "`find-activity` + `invalid` is invalid");
        Assert.False(FindActivityCommand.IsRunnable(["invalid", CommandArgumentConstant.FIND_ACTIVITY]), "`invalid` + `activity` is invalid");
        Assert.False(FindActivityCommand.IsRunnable([CommandArgumentConstant.SETUP]), "`setup` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForSingleEnableParam()
    {
        Assert.True(FindActivityCommand.IsRunnable([CommandArgumentConstant.FIND_ACTIVITY]));
    }
}
