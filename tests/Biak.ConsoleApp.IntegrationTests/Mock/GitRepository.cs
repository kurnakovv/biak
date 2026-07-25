// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Mock;

public static class GitRepository
{
    public static async Task MockAsync(AppExecutionContext? executionContext = null)
    {
        AppExecutionContext context = executionContext ?? AppExecutionContext.CreateDefault();

        await GitHelper.RunAsync("init", context);

        await GitHelper.RunAsync("config --local user.email \"test@example.com\"", context);
        await GitHelper.RunAsync("config --local user.name \"Test User\"", context);

        await GitHelper.RunAsync("branch -m master main", context);

        await GitHelper.RunAsync("add .", context);

        await GitHelper.RunAsync("commit -m \"Initial commit\"", context);

        string defaultBranch = (await GitHelper.RunAsync("branch --show-current", context)).Trim();
        if (string.IsNullOrEmpty(defaultBranch))
        {
            defaultBranch = "master";
        }

        for (int i = 1; i <= 3; i++)
        {
            await GitHelper.RunAsync($"checkout -b test-f-{i}", context);

            string testServicePath = Path.Join(context.WorkingDirectory, $"TestService{i}.cs");

            string serviceContent = await File.ReadAllTextAsync(testServicePath);
            serviceContent = serviceContent.Replace(
                "Console.WriteLine(\"2\");",
                "Console.WriteLine(\"2\");Console.WriteLine(\"test\");",
                StringComparison.Ordinal
            );
            await File.WriteAllTextAsync(testServicePath, serviceContent);

            await GitHelper.RunAsync("add .", context);

            await GitHelper.RunAsync($"commit -m \"Update TestService{i}\"", context);

            await GitHelper.RunAsync($"checkout {defaultBranch}", context);
        }

        await GitHelper.RunAsync("checkout -b empty-branch", context);
        await GitHelper.RunAsync($"checkout {defaultBranch}", context);

        await GitHelper.RunAsync("checkout -b no-cs-file-changes", context);
        string gitattributesPath = Path.Join(context.WorkingDirectory, ".gitattributes");
        string gitattributesContent = await File.ReadAllTextAsync(gitattributesPath);
        gitattributesContent += " ";
        await File.WriteAllTextAsync(gitattributesPath, gitattributesContent);
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Update .gitattributes\"", context);
        await GitHelper.RunAsync($"checkout {defaultBranch}", context);

        await GitHelper.RunAsync("checkout -b old-branch", context);
        string testService9Path = Path.Join(context.WorkingDirectory, "TestService9.cs");
        string testService9Content = await File.ReadAllTextAsync(testService9Path);
        testService9Content += " ";
        await File.WriteAllTextAsync(testService9Path, testService9Content);
        await GitHelper.RunAsync("add .", context);
        string? originCommitterDate = Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
        try
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", "2021-01-01 12:12:00");
            await GitHelper.RunAsync("commit --date=\"2021-01-01 12:12:00\" -m \"Update TestService9\"", context);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", originCommitterDate);
        }
        await GitHelper.RunAsync($"checkout {defaultBranch}", context);

        await GitHelper.RunAsync("checkout -b change-testservice1", context);
        string testService1Path = Path.Join(context.WorkingDirectory, "TestService1.cs");
        string testService1Content = await File.ReadAllTextAsync(testService1Path);
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Update TestService1\"", context);
        await GitHelper.RunAsync($"checkout {defaultBranch}", context);

        await GitHelper.RunAsync("checkout -b f-new-cs-file", context);
        string newTestFilePath = Path.Join(context.WorkingDirectory, "NewTestFile.cs");
        await File.WriteAllTextAsync(newTestFilePath, "// Test");
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Add NewTestFile.cs\"", context);

        await GitHelper.RunAsync("checkout -b branch-for-exclude", context);
        testService1Content += " ";
        await File.WriteAllTextAsync(testService1Path, testService1Content);
        await GitHelper.RunAsync("add .", context);
        await GitHelper.RunAsync("commit -m \"Update TestService1\"", context);
        await GitHelper.RunAsync($"checkout {defaultBranch}", context);
    }
}
