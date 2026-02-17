// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.IntegrationTests.Tools;
using Xunit.Abstractions;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class SetupCommandTests
{
    private readonly IProcessRunner _processRunner;

    public SetupCommandTests(
        ITestOutputHelper output
    )
    {
        _processRunner = new ProcessRunner(output);
    }

    [Fact]
    public async Task RunWithoutEditorconfigFileAsync()
    {
        ProcessResult result = await _processRunner.RunAsync(CommandArgumentConstant.SETUP);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains(".editorconfig not found:", result.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunWithEditorconfigAsync()
    {
        TestDirectory testDir = new(nameof(RunWithEditorconfigAsync));

        string template = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        ProcessResult result = await _processRunner.RunAsync(
            command: CommandArgumentConstant.SETUP,
            workingDirectory: testDir.Value
        );

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Setup .biak folder...", result.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Folder .biak was created successfully.", result.Output, StringComparison.OrdinalIgnoreCase);

        string editorconfigMainFile = Path.Combine(testDir.Value, ".biak", ".editorconfig-main");
        bool editorconfigFileExists = File.Exists(editorconfigMainFile);

        Assert.True(editorconfigFileExists);
        using StreamReader readerTemplate = new(template);
        using StreamReader readerEditorconfigMain = new(editorconfigMainFile);
        string templateContent = await readerTemplate.ReadToEndAsync();
        string editorconfigMainContent = await readerEditorconfigMain.ReadToEndAsync();

        Assert.Equal(templateContent, editorconfigMainContent);
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync()
    {
        TestDirectory testDir = new(nameof(RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync));

        string template = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        string biakDir = Path.Combine(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string oldFile = Path.Combine(biakDir, "old.txt");
        await File.WriteAllTextAsync(oldFile, "old");

        ProcessResult result = await _processRunner.RunAsync(
            command: CommandArgumentConstant.SETUP,
            workingDirectory: testDir.Value,
            standardInput: string.Empty
        );

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Folder .biak already exists", result.Output, StringComparison.OrdinalIgnoreCase);

        Assert.True(File.Exists(oldFile));
        Assert.False(File.Exists(Path.Combine(biakDir, ".editorconfig-main")));
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync()
    {
        TestDirectory testDir = new(nameof(RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync));

        string template = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        string biakDir = Path.Combine(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string oldFile = Path.Combine(biakDir, "old.txt");
        await File.WriteAllTextAsync(oldFile, "old");

        ProcessResult result = await _processRunner.RunAsync(
            command: CommandArgumentConstant.SETUP,
            workingDirectory: testDir.Value,
            standardInput: "y"
        );

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Setup .biak folder", result.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("was created successfully", result.Output, StringComparison.OrdinalIgnoreCase);

        Assert.False(File.Exists(oldFile));
        Assert.True(File.Exists(Path.Combine(biakDir, ".editorconfig-main")));
    }
}
