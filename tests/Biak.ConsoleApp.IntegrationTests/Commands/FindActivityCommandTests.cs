// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class FindActivityCommandTests
{
    [Fact]
    public async Task RunTestAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(FindActivityCommandTests)}_{nameof(RunTestAsync)}");
        await FindActivityCommand.RunAsync();

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            string templateSimpleProject = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "SimpleProject"
            );

            testDir.CopyDirectory(templateSimpleProject);

            await MockGitRepositoryAsync();

            await FindActivityCommand.RunAsync();

            string result = output.ToString();
            Assert.NotEmpty(result);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static async Task MockGitRepositoryAsync()
    {
        await GitHelper.RunAsync("init");

        await GitHelper.RunAsync("config --local user.email \"test@example.com\"");
        await GitHelper.RunAsync("config --local user.name \"Test User\"");

        await GitHelper.RunAsync("add .");

        await GitHelper.RunAsync("commit -m \"Initial commit\"");

        string defaultBranch = (await GitHelper.RunAsync("branch --show-current")).Trim();
        if (string.IsNullOrEmpty(defaultBranch))
        {
            defaultBranch = "master";
        }

        for (int i = 1; i <= 3; i++)
        {
            string featureBranch = $"test-f-{i}";
            await GitHelper.RunAsync($"checkout -b {featureBranch}");

            string testServicePath = Path.Join(
                $"TestService{i}.cs"
            );

            string content = await File.ReadAllTextAsync(testServicePath);
            content = content.Replace(
                "Console.WriteLine(\"2\");",
                "Console.WriteLine(\"2\");Console.WriteLine(\"test\");",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(testServicePath, content);

            await GitHelper.RunAsync("add .");

            await GitHelper.RunAsync($"commit -m \"Update TestService{i}\"");

            await GitHelper.RunAsync($"checkout {defaultBranch}");
        }
    }
}
