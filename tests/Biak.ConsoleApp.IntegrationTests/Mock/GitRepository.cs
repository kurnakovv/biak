// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.IntegrationTests.Mock;

public static class GitRepository
{
    public static async Task MockAsync()
    {
        await GitHelper.RunAsync("init");

        await GitHelper.RunAsync("config --local user.email \"test@example.com\"");
        await GitHelper.RunAsync("config --local user.name \"Test User\"");

        await GitHelper.RunAsync("branch -m master main");

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

            string testServicePath = $"TestService{i}.cs";

            string serviceContent = await File.ReadAllTextAsync(testServicePath);
            serviceContent = serviceContent.Replace(
                "Console.WriteLine(\"2\");",
                "Console.WriteLine(\"2\");Console.WriteLine(\"test\");",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(testServicePath, serviceContent);

            await GitHelper.RunAsync("add .");

            await GitHelper.RunAsync($"commit -m \"Update TestService{i}\"");

            await GitHelper.RunAsync($"checkout {defaultBranch}");
        }

        await GitHelper.RunAsync("checkout -b empty-branch");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b no-cs-file-changes");
        string gitattributesPath = Path.Join(".gitattributes");
        string gitattributesContent = await File.ReadAllTextAsync(gitattributesPath);
        gitattributesContent += " ";
        await File.WriteAllTextAsync(gitattributesPath, gitattributesContent);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update .gitattributes\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b old-branch");
        string testService9Path = Path.Join("TestService9.cs");
        string testService9Content = await File.ReadAllTextAsync(testService9Path);
        testService9Content += " ";
        await File.WriteAllTextAsync(testService9Path, testService9Content);
        await GitHelper.RunAsync("add .");
        string? originCommitterDate = Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
        try
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", "2021-01-01 12:12:00");
            await GitHelper.RunAsync("commit --date=\"2021-01-01 12:12:00\" -m \"Update TestService9\"");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", originCommitterDate);
        }
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b change-testservice1");
        string testService1Path = Path.Join("TestService1.cs");
        string testService1Content = await File.ReadAllTextAsync(testService1Path);
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update TestService1\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");

        await GitHelper.RunAsync("checkout -b f-new-cs-file");
        string newTestFilePath = Path.Join("NewTestFile.cs");
        await File.WriteAllTextAsync(newTestFilePath, "// Test");
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Add NewTestFile.cs\"");

        await GitHelper.RunAsync("checkout -b branch-for-exclude");
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .");
        await GitHelper.RunAsync("commit -m \"Update TestService1\"");
        await GitHelper.RunAsync($"checkout {defaultBranch}");
    }
}
