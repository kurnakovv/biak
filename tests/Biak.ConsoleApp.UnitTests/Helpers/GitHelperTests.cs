// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class GitHelperTests
{
    [Fact]
    public async Task RunTestInvalidCommandAsync()
    {
        Exception? exception = await Record.ExceptionAsync(async () => await GitHelper.RunAsync("invalid-command"));
        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(GitHelperConstant.GIT_ERROR + "git: 'invalid-command' is not a git command. See 'git --help'.", exception.Message.Trim());
    }
}
