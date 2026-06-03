// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class WarningsBaselineInitCommandTests
{
#pragma warning disable IDE1006 // Naming Styles
    private static readonly string TEST_OUTPUT = WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE
#pragma warning restore IDE1006 // Naming Styles
        + Environment.NewLine
        + WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_CONFIGURATION
        + Environment.NewLine
        + Environment.NewLine
        + """
        .editorconfig
        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0108.severity = suggestion # ^biak^ baseline

        [{ProgramCS0168Warning.cs}]
        dotnet_diagnostic.CS0168.severity = suggestion # ^biak^ baseline

        [{MyClassCS0169.cs}]
        dotnet_diagnostic.CS0169.severity = suggestion # ^biak^ baseline

        [{ProgramCS0219Warning.cs}]
        dotnet_diagnostic.CS0219.severity = suggestion # ^biak^ baseline

        [{ProgramCS0612.cs}]
        dotnet_diagnostic.CS0612.severity = suggestion # ^biak^ baseline

        [{DerivedClassCS0649.cs}]
        dotnet_diagnostic.CS0649.severity = suggestion # ^biak^ baseline

        [{MyTestForlder/MyTestModel1.cs,MyTestModel.cs}]
        dotnet_diagnostic.CS8618.severity = suggestion # ^biak^ baseline
        """;

    [Fact]
    public async Task RunTestAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunTestAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProjectWithWarnings",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            await WarningsBaselineInitCommand.RunAsync();

            string result = output.ToString();

            Assert.NotEmpty(result);
            Assert.Equal(TEST_OUTPUT, result.Trim());
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildFailedAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildFailedAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            Exception? exception = await Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync);

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.StartsWith(WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenDotnetBuildProcessCannotStartAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenDotnetBuildProcessCannotStartAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        string? originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            Exception? exception = await Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync);

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.True(
                exception.Message.StartsWith(WarningsBaselineInitCommandConstant.INIT_FAILED, StringComparison.Ordinal)
                    || exception.Message.StartsWith(WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED, StringComparison.Ordinal),
                $"Expected message to start with '{WarningsBaselineInitCommandConstant.INIT_FAILED}' or '{WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED}', but got '{exception.Message}'."
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunShouldThrowWhenBiakBuildBinlogNotFoundAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenBiakBuildBinlogNotFoundAsync)}"
        );

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        TextReader originalIn = Console.In;
        using StringReader input = new("\n");
        Console.SetIn(input);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProjectWithWarnings",
                "MySimpleProjectTemplate"
            );

            testDir.CopyDirectory(templateSimpleProject);

            string anotherDirectory = Path.Join(testDir.Value, "another");
            Directory.CreateDirectory(anotherDirectory);

            Task<Exception> exceptionTask = Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync);
            await Task.Delay(100);
            Directory.SetCurrentDirectory(anotherDirectory);

            Exception exception = await exceptionTask;

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.Equal(WarningsBaselineInitCommandConstant.BUILD_BINLOG_NOT_FOUND, exception.Message);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
