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
    private const string TEST_OUTPUT = WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE + """

        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

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
            Assert.StartsWith(WarningsBaselineInitCommandConstant.INIT_FAILED, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    ////[Fact]
    ////public async Task RunShouldThrowWhenBuildBinlogNotFoundAsync()
    ////{
    ////    string originalDirectory = Directory.GetCurrentDirectory();
    ////    TestDirectory testDir = new(
    ////        $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldThrowWhenBuildBinlogNotFoundAsync)}"
    ////    );

    ////    TextWriter originalOut = Console.Out;
    ////    await using StringWriter output = new();
    ////    Console.SetOut(output);

    ////    TextReader originalIn = Console.In;
    ////    using StringReader input = new("\n");
    ////    Console.SetIn(input);

    ////    try
    ////    {
    ////        Directory.SetCurrentDirectory(testDir.Value);

    ////        string templateSimpleProject = Path.Join(
    ////            AppContext.BaseDirectory,
    ////            "Templates",
    ////            "SimpleProjectWithWarnings",
    ////            "MySimpleProjectTemplate"
    ////        );

    ////        testDir.CopyDirectory(templateSimpleProject);

    ////        string anotherDirectory = Path.Join(testDir.Value, "another");
    ////        Directory.CreateDirectory(anotherDirectory);

    ////        Task<Exception?> exceptionTask = Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync);
    ////        await Task.Delay(100);
    ////        Directory.SetCurrentDirectory(anotherDirectory);

    ////        Exception? exception = await exceptionTask;

    ////        Assert.NotNull(exception);
    ////        Assert.IsType<BiakApplicationException>(exception);
    ////        Assert.Equal(WarningsBaselineInitCommandConstant.BUILD_BINLOG_NOT_FOUND, exception.Message);
    ////    }
    ////    finally
    ////    {
    ////        Console.SetOut(originalOut);
    ////        Console.SetIn(originalIn);
    ////        Directory.SetCurrentDirectory(originalDirectory);
    ////    }
    ////}
}
