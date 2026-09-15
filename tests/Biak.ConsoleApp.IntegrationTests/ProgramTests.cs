// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Commands.Baseline.Warnings;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests;

public class ProgramTests
{
    [Fact]
    public async Task NoArgumentsGreetingAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(NoArgumentsGreetingAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([], context);

        string result = output.ToString().Trim();
        Assert.Equal(DocsConstant.GREETING.Trim(), result);
    }

    [Fact]
    public async Task HelpArgumentAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(HelpArgumentAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.HELP], context);

        string result = output.ToString().Trim();
        Assert.Equal(DocsConstant.HELP.Trim(), result);
    }

    [Fact]
    public async Task SetupCommandNoEditorconfigMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(SetupCommandNoEditorconfigMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.SETUP], context);

        string result = output.ToString().Trim();
        Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DisableCommandNoConfigurableMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(DisableCommandNoConfigurableMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.DISABLE], context);

        string result = output.ToString().Trim();
        Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnableCommandNoConfigurableMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(EnableCommandNoConfigurableMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.ENABLE], context);

        string result = output.ToString().Trim();
        Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StatusCommandNoConfigurableMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(StatusCommandNoConfigurableMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.STATUS], context);

        string result = output.ToString().Trim();
        Assert.Equal(UIConstant.STATUS_BROKEN, result);
    }

    [Fact]
    public async Task FindActivityCommandForCurrentRepoAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(FindActivityCommandForCurrentRepoAsync)}");
        using StringReader input = new("\n\n\n\n\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProject",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        await GitHelper.RunAsync("init", context);
        await GitHelper.RunAsync("branch -m master main", context);
        await GitHelper.RunAsync("config --local user.email \"test@example.com\"", context);
        await GitHelper.RunAsync("config --local user.name \"Test User\"", context);
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Initial commit\"", context);

        await Program.MainInnerAsync([CommandArgumentConstant.FIND_ACTIVITY], context);

        string result = output.ToString().Trim();
        Assert.Contains(FindActivityCommandConstant.START, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindActivityCommandConstant.ACTIVITY, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindActivityCommandConstant.INACTIVE_BRANCHES, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindActivityCommandConstant.ACTIVITY_VIA_SINGLE_LINE, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindActivityCommandConstant.ACTIVITY_VIA_VARIABLE, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FindConflictCommandForCurrentRepoAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(FindConflictCommandForCurrentRepoAsync)}");
        using StringReader input = new("\nfdasdfadfsa");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProject",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        await GitHelper.RunAsync("init", context);
        await GitHelper.RunAsync("branch -m master main", context);
        await GitHelper.RunAsync("config --local user.email \"test@example.com\"", context);
        await GitHelper.RunAsync("config --local user.name \"Test User\"", context);
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Initial commit\"", context);

        await Program.MainInnerAsync([CommandArgumentConstant.FIND_CONFLICTS], context);

        string result = output.ToString().Trim();
        Assert.Contains(FindConflictsCommandConstant.START, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindConflictsCommandConstant.CONFLICTING_FILES, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(SharedFindCommandConstant.NO_ENTRIES, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FindConflictsCommandConstant.NOT_FOUND_BRANCHES, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FindConflictCommandWithExceptionAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(FindConflictCommandWithExceptionAsync)}");
        using StringReader input = new("\nfdasdfadfsa");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProject",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        await GitHelper.RunAsync("init", context);
        await GitHelper.RunAsync("branch -m master main", context);
        await GitHelper.RunAsync("config --local user.email \"test@example.com\"", context);

        Exception? exception = await Record.ExceptionAsync(async () => await Program.MainInnerAsync([CommandArgumentConstant.FIND_CONFLICTS], context));
        Assert.NotNull(exception);
        Assert.IsType<BiakApplicationException>(exception);
        Assert.Equal(FindConflictsCommandConstant.LOCAL_CHANGES_DETECTED, exception.Message.Trim());
    }

    [Fact]
    public async Task WarningsBaselineInitCommandAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(WarningsBaselineInitCommandAsync)}");
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProjectWithWarnings",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        await Program.MainInnerAsync([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.INIT], context);

        string result = output.ToString().Trim();
        Assert.Contains(WarningsBaselineInitCommandConstant.TREAT_WARNINGS_AS_ERRORS_NOTE, result, StringComparison.Ordinal);
        Assert.Contains(WarningsBaselineInitCommandConstant.INSERT_FILTERS_TO_EDITORCONFIG_NOTE, result, StringComparison.Ordinal);
        Assert.Contains("dotnet_diagnostic.CS", result, StringComparison.Ordinal);
        Assert.Contains($"severity = suggestion {WarningsBaselineInitCommandConstant.BASELINE_DIAGNOSTIC_MARKER}", result, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WarningsBaselineSyncCommandAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(WarningsBaselineSyncCommandAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templateSimpleProject = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "SimpleProjectWithWarnings",
            "MySimpleProjectTemplate"
        );

        testDir.CopyDirectory(templateSimpleProject);

        string editorconfigPath = Path.Join(testDir.Value, ".editorconfig");
        await File.WriteAllTextAsync(editorconfigPath, WarningsBaselineCommandTestConstants.BASELINE_EDITORCONFIG);

        await Program.MainInnerAsync([CommandArgumentConstant.WARNINGS_BASELINE, CommandArgumentConstant.SYNC], context);

        string result = output.ToString().Trim();
        Assert.Contains(WarningsBaselineSyncCommandConstant.SYNC_STARTED, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InspectCodeBaselineInitCommandAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(InspectCodeBaselineInitCommandAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string templatePath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "InspectCodeBaseline",
            "InspectCodeBaselineTemplate"
        );

        testDir.CopyDirectory(templatePath);

        await Program.MainInnerAsync([CommandArgumentConstant.INSPECTCODE_BASELINE, CommandArgumentConstant.INIT], context);

        string result = output.ToString().Trim();
        Assert.Contains(InspectCodeBaselineInitCommandConstant.INIT_STARTED, result, StringComparison.Ordinal);
        Assert.Contains(InspectCodeBaselineInitCommandConstant.INSERT_FILTERS_NOTE, result, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SetupWithInvalidCommandNoCommandMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(SetupWithInvalidCommandNoCommandMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync([CommandArgumentConstant.SETUP, "invalidCommand"], context);

        string result = output.ToString().Trim();
        Assert.Contains(UIConstant.NO_COMMAND, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidCommandNoCommandMessageAsync()
    {
        TestDirectory testDir = new($"{nameof(ProgramTests)}_{nameof(InvalidCommandNoCommandMessageAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await Program.MainInnerAsync(["invalidCommand"], context);

        string result = output.ToString().Trim();
        Assert.Equal(UIConstant.NO_COMMAND, result);
    }
}
