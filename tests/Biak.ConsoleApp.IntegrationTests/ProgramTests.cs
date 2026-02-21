// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

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
