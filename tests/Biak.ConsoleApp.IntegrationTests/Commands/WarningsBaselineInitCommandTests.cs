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
        + WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE
        + Environment.NewLine
        + """
        [{VisualBasicProject/Module1.vb}]
        dotnet_diagnostic.BC40000.severity = suggestion # ^biak^ baseline

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

        [{FSharpProject/Library.fs}]
        dotnet_diagnostic.FS0025.severity = suggestion # ^biak^ baseline
        """;

    private static readonly TimeSpan s_commandTimeout = TimeSpan.FromMinutes(3);

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

            string csprojPath = Path.Join(testDir.Value, "MySimpleProjectTemplate.csproj");
            string csprojContent = await File.ReadAllTextAsync(csprojPath);
            csprojContent = csprojContent.Replace(
                "</Project>",
                """
                  <ItemGroup>
                    <PackageReference Include="Microsoft.Bcl.Async" Version="1.0.168" />
                  </ItemGroup>
                </Project>
                """,
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(csprojPath, csprojContent);

            string editorconfigContent = await ExecuteWithTimeoutAsync(WarningsBaselineInitCommand.RunAsync);

            string result = output.ToString();

            Assert.NotEmpty(result);
            Assert.Equal(TEST_OUTPUT, result.Trim());
            Assert.DoesNotContain("dotnet_diagnostic.NU1701.severity", result, StringComparison.Ordinal);
            Assert.False(File.Exists(WarningsBaselineInitCommandConstant.BUILD_BINLOG_PATH));

            string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
            await File.WriteAllTextAsync(editorconfigPath, "root = true" + Environment.NewLine + Environment.NewLine + editorconfigContent);

            string secondEditorconfigContent = await ExecuteWithTimeoutAsync(WarningsBaselineInitCommand.RunAsync);

            string[] suppressedCsCodes = ["CS0108", "CS0168", "CS0169", "CS0219", "CS0612", "CS0649", "CS8618"];
            foreach (string code in suppressedCsCodes)
            {
                Assert.DoesNotContain(code, secondEditorconfigContent, StringComparison.Ordinal);
            }
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

            Exception? exception = await ExecuteWithTimeoutAsync(() => Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync));

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

            Exception? exception = await ExecuteWithTimeoutAsync(() => Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync));

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
    public async Task RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new(
            $"{nameof(WarningsBaselineInitCommandTests)}_{nameof(RunShouldRefuseToGenerateFiltersWhenBuildContainsErrorsAsync)}"
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

            string brokenFile = Path.Join(testDir.Value, "ProgramCS0168Warning.cs");
            await File.WriteAllTextAsync(
                brokenFile,
                """
                public class ProgramCS0168Warning
                {
                    public static void M1()
                    {
                        int a = ;
                    }
                }
                """
            );

            Exception? exception = await ExecuteWithTimeoutAsync(() => Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync));

            Assert.NotNull(exception);
            Assert.IsType<BiakApplicationException>(exception);
            Assert.True(
                exception.Message.StartsWith(WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED, StringComparison.Ordinal)
                    || exception.Message.Equals(WarningsBaselineInitCommandConstant.BUILD_CONTAINS_ERRORS, StringComparison.Ordinal),
                $"Expected message to start with '{WarningsBaselineInitCommandConstant.DOTNET_BUILD_FAILED}' or be '{WarningsBaselineInitCommandConstant.BUILD_CONTAINS_ERRORS}', but got '{exception.Message}'."
            );
            Assert.DoesNotContain(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE, output.ToString(), StringComparison.Ordinal);
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

            Task<Exception?> exceptionTask = ExecuteWithTimeoutAsync(() => Record.ExceptionAsync(WarningsBaselineInitCommand.RunAsync));
            await Task.Delay(100);
            Directory.SetCurrentDirectory(anotherDirectory);

            Exception? exception = await exceptionTask;

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

    private static async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> action)
    {
        Task<T> task = action();
        Task completedTask = await Task.WhenAny(task, Task.Delay(s_commandTimeout));

        if (completedTask != task)
        {
            throw new TimeoutException($"WarningsBaselineInitCommand test timeout after {s_commandTimeout}.");
        }

        return await task;
    }
}
