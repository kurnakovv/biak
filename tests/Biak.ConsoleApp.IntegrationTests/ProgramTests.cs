// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Tools;
using Xunit.Abstractions;

namespace Biak.ConsoleApp.IntegrationTests;

public class ProgramTests
{
    private readonly IProcessRunner _processRunner;

    public ProgramTests(
        ITestOutputHelper output
    )
    {
        _processRunner = new ProcessRunner(output);
    }

    [Fact]
    public async Task NoArgumentsGreetingAsync()
    {
        ProcessResult result = await _processRunner.RunAsync(string.Empty);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains(DocsConstant.GREETING, result.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidCommandNoCommandMessageAsync()
    {
        ProcessResult result = await _processRunner.RunAsync("invalidCommand");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains(DocsConstant.NO_COMMAND, result.Output, StringComparison.OrdinalIgnoreCase);
    }
}
