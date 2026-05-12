// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests;

public class ProgramTests
{
    [Fact]
    public async Task NoArgumentsGreetingAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([]);

            string result = output.ToString().Trim();
            Assert.Equal(DocsConstant.GREETING.Trim(), result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task HelpArgumentAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([CommandArgumentConstant.HELP]);

            string result = output.ToString().Trim();
            Assert.Equal(DocsConstant.HELP.Trim(), result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task SetupCommandNoEditorconfigMessageAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([CommandArgumentConstant.SETUP]);

            string result = output.ToString().Trim();
            Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisableCommandNoConfigurableMessageAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([CommandArgumentConstant.DISABLE]);

            string result = output.ToString().Trim();
            Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task EnableCommandNoConfigurableMessageAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([CommandArgumentConstant.ENABLE]);

            string result = output.ToString().Trim();
            Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task FindActivityCommandForCurrentRepoAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(FindActivityCommandForCurrentRepoAsync)}");

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n\n\n\n\n");
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            await GitHelper.RunAsync("init");
            await GitHelper.RunAsync("branch -m master main");
            await GitHelper.RunAsync("config --local user.email \"test@example.com\"");
            await GitHelper.RunAsync("config --local user.name \"Test User\"");
            await GitHelper.RunAsync("add .");
            await GitHelper.RunAsync("commit -m \"Initial commit\"");

            await Program.Main([CommandArgumentConstant.FIND_ACTIVITY]);

            string result = output.ToString().Trim();
            Assert.Contains(FindActivityCommandConstant.START, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(FindActivityCommandConstant.ACTIVITY, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(FindActivityCommandConstant.INACTIVE_BRANCHES, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task SetupWithInvalidCommandNoCommandMessageAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main([CommandArgumentConstant.SETUP, "invalidCommand"]);

            string result = output.ToString().Trim();
            Assert.Contains(UIConstant.NO_COMMAND, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task InvalidCommandNoCommandMessageAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await Program.Main(["invalidCommand"]);

            string result = output.ToString().Trim();
            Assert.Equal(UIConstant.NO_COMMAND, result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
