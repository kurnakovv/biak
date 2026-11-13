// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class FindBranchDefaultTests
{
    [Theory]
    [InlineData(true, "", false)]
    [InlineData(true, " \t", false)]
    [InlineData(true, "dev", false)]
    [InlineData(true, "main", true)]
    [InlineData(false, "", true)]
    [InlineData(false, " \t", true)]
    [InlineData(false, "dev", true)]
    [InlineData(false, "  main  ", false)]
    public async Task UsesCurrentBranchOrExplicitOverrideAsync(bool activity, string branchInput, bool hasEntry)
    {
        TestDirectory testDir = new(nameof(UsesCurrentBranchOrExplicitOverrideAsync));
        using StringReader input = new(branchInput + (activity ? "\n\n\n\n\n\n\n" : "\nfeature\n"));
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };
        await CreateRepositoryAsync(context);

        await RunAsync(activity, context);

        Assert.Contains("Target branch ('dev' by default): ", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(hasEntry, output.ToString().Contains(activity ? "dev-copy" : "Shared.cs", StringComparison.Ordinal));
        await AssertRestoredAsync(context, "dev");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" \t", false)]
    [InlineData("main", true)]
    [InlineData("dev", false)]
    public async Task ActivityConfigurationTakesPrecedenceAsync(string? configuredBranch, bool hasEntry)
    {
        TestDirectory testDir = new(nameof(ActivityConfigurationTakesPrecedenceAsync));
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };
        await CreateRepositoryAsync(context);
        string configDirectory = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(configDirectory);
        string config = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                findActivity = new
                {
                    targetBranch = configuredBranch,
                    expirationPeriod = "*",
                    fileTypes = "MDR",
                    fileExtensions = ".cs",
                    excludeBranches = string.Empty,
                    includedFilePaths = string.Empty,
                    saveOutput = false,
                },
            }
        );
        await File.WriteAllTextAsync(Path.Join(configDirectory, "config.json"), config);

        await FindActivityCommand.RunAsync(context);

        Assert.Equal(hasEntry, output.ToString().Contains("dev-copy", StringComparison.Ordinal));
        Assert.Equal(string.IsNullOrWhiteSpace(configuredBranch), output.ToString().Contains("Target branch (", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(true, "", false)]
    [InlineData(false, "", false)]
    [InlineData(true, "main", true)]
    [InlineData(false, "main", true)]
    public async Task DetachedHeadRequiresExplicitBranchAsync(bool activity, string branchInput, bool succeeds)
    {
        TestDirectory testDir = new(nameof(DetachedHeadRequiresExplicitBranchAsync));
        using StringReader input = new(branchInput + (activity ? "\n\n\n\n\n\n\n" : "\nfeature\n"));
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };
        await CreateRepositoryAsync(context);
        await GitHelper.RunAsync("checkout --detach", context);
        string originalCommit = await GitHelper.RunAsync("rev-parse HEAD", context);

        if (succeeds)
        {
            await RunAsync(activity, context);
        }
        else
        {
            BiakApplicationException exception = await Assert.ThrowsAsync<BiakApplicationException>(() => RunAsync(activity, context));
            Assert.Contains("An explicit branch is required", exception.Message, StringComparison.Ordinal);
        }

        Assert.Contains("Current branch could not be determined", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("'main' by default", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(originalCommit, await GitHelper.RunAsync("rev-parse HEAD", context));
        await AssertRestoredAsync(context, string.Empty);
    }

    [Fact]
    public async Task ActivityConfigurationWorksWithDetachedHeadAsync()
    {
        TestDirectory testDir = new(nameof(ActivityConfigurationWorksWithDetachedHeadAsync));
        using StringReader input = new("\n\n\n\n\n\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };
        await CreateRepositoryAsync(context);
        await GitHelper.RunAsync("checkout --detach", context);
        string configDirectory = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(configDirectory);
        await File.WriteAllTextAsync(Path.Join(configDirectory, "config.json"), """{"findActivity":{"targetBranch":"main"}}""");

        await FindActivityCommand.RunAsync(context);

        Assert.Contains("dev-copy", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Enter an explicit branch", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ActivityConfigurationSupportsLegacyDefaultBranchAsync()
    {
        TestDirectory testDir = new(nameof(ActivityConfigurationSupportsLegacyDefaultBranchAsync));
        using StringReader input = new("\n");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };
        await CreateRepositoryAsync(context);
        string configDirectory = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(configDirectory);
        await File.WriteAllTextAsync(Path.Join(configDirectory, "config.json"), """{"findActivity":{"defaultBranch":"main"}}""");

        await FindActivityCommand.RunAsync(context);

        Assert.Contains("dev-copy", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Target branch (", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(FindActivityCommandConstant.LEGACY_BRANCH_CONFIG_WARNING, output.ToString(), StringComparison.Ordinal);
    }

    private static Task RunAsync(bool activity, AppExecutionContext context)
    {
        return activity ? FindActivityCommand.RunAsync(context) : FindConflictsCommand.RunAsync(context);
    }

    private static async Task CreateRepositoryAsync(AppExecutionContext context)
    {
        await GitHelper.RunAsync("init -b main", context);
        await GitHelper.RunAsync("config user.email test@example.com", context);
        await GitHelper.RunAsync("config user.name Test", context);
        string file = Path.Join(context.WorkingDirectory, "Shared.cs");
        await File.WriteAllTextAsync(file, "base\n");
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m base", context);
        await GitHelper.RunAsync("checkout -b feature", context);
        await File.WriteAllTextAsync(file, "feature\n");
        await GitHelper.RunAsync("commit -am feature", context);
        await GitHelper.RunAsync("checkout -b dev main", context);
        await File.WriteAllTextAsync(file, "dev\n");
        await GitHelper.RunAsync("commit -am dev", context);
        await GitHelper.RunAsync("branch dev-copy", context);
    }

    private static async Task AssertRestoredAsync(AppExecutionContext context, string branch)
    {
        Assert.Equal(branch, (await GitHelper.RunAsync("branch --show-current", context)).Trim());
        Assert.Empty(await GitHelper.RunAsync("status --porcelain", context));
        GitResult mergeHead = await GitHelper.RunWithModelAsync("rev-parse -q --verify MERGE_HEAD", context);
        Assert.NotEqual(0, mergeHead.ExitCode);
    }
}
