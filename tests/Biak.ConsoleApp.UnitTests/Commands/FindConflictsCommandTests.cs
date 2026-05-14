// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Commands;

public class FindConflictsCommandTests
{
    [Fact]
    public void IsRunnableFalseForEmptyParams()
    {
        Assert.False(FindConflictsCommand.IsRunnable([]), "You can't run find-conflicts command without `find-conflicts` argument");
    }

    [Fact]
    public void IsRunnableFalseForInvalidParams()
    {
        Assert.False(FindConflictsCommand.IsRunnable(["blabla"]), "`blabla` is invalid param");
        Assert.False(FindConflictsCommand.IsRunnable([CommandArgumentConstant.FIND_CONFLICTS, "invalid"]), "`find-conflicts` + `invalid` is invalid");
        Assert.False(FindConflictsCommand.IsRunnable(["invalid", CommandArgumentConstant.FIND_CONFLICTS]), "`invalid` + `conflicts` is invalid");
        Assert.False(FindConflictsCommand.IsRunnable([CommandArgumentConstant.SETUP]), "`setup` is invalid");
    }

    [Fact]
    public void IsRunnableTrueForSingleFindCoflictsParam()
    {
        Assert.True(FindConflictsCommand.IsRunnable([CommandArgumentConstant.FIND_CONFLICTS]));
    }
}
