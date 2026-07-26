// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class SetupCommandTests
{
    [Fact]
    public async Task RunWithoutEditorconfigFileAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await SetupCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunWithEditorconfigAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWithEditorconfigAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        await SetupCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.START_SETUP, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.END_SETUP, result, StringComparison.OrdinalIgnoreCase);

        string editorconfigMainFile = Path.Join(testDir.Value, ".biak", ".editorconfig-main");

        Assert.True(File.Exists(editorconfigMainFile));
        string templateContent = await File.ReadAllTextAsync(template);
        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigMainFile);

        Assert.Equal(templateContent, editorconfigMainContent);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        string editorconfigContent = await File.ReadAllTextAsync(editorconfigFile);

        string newline = editorconfigContent.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";

        string up = EditorconfigConstant.UP_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);
        string bottom = EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);

        Assert.Contains(up, editorconfigContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(bottom, editorconfigContent, StringComparison.OrdinalIgnoreCase);

        string configPath = Path.Join(testDir.Value, ".biak", "config.json");
        string templateConfigPath = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "default-config.json"
        );
        Assert.True(File.Exists(configPath));
        string templateConfigContent = await File.ReadAllTextAsync(templateConfigPath);
        string configContent = await File.ReadAllTextAsync(configPath);
        Assert.Equal(templateConfigContent, configContent);
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync)}");
        using StringReader input = new(string.Empty);
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string oldFile = Path.Join(biakDir, "old.txt");
        await File.WriteAllTextAsync(oldFile, "old");

        await SetupCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.BIAK_FOLDER_ALREADY_EXISTS, result, StringComparison.OrdinalIgnoreCase);

        Assert.True(File.Exists(oldFile));
        Assert.False(File.Exists(Path.Join(biakDir, ".editorconfig-main")));
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync)}");
        using StringReader input = new(UIConstant.CONFIRM);
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, In = input, Out = output };

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string oldFile = Path.Join(biakDir, "old.txt");
        await File.WriteAllTextAsync(oldFile, "old");

        await SetupCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.BIAK_FOLDER_ALREADY_EXISTS, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.START_SETUP, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.END_SETUP, result, StringComparison.OrdinalIgnoreCase);

        string editorconfigMainPath = Path.Join(biakDir, ".editorconfig-main");
        Assert.False(File.Exists(oldFile));
        Assert.True(File.Exists(editorconfigMainPath));
        string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigMainPath);
        Assert.DoesNotContain(EditorconfigConstant.UP_TEXT, editorconfigMainContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EditorconfigConstant.BOTTOM_TEXT, editorconfigMainContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunWithLFEditorconfigAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWithLFEditorconfigAsync)}");

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LF",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };
        await SetupCommand.RunAsync(context);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        string editorconfigContent = await File.ReadAllTextAsync(editorconfigFile);

        Assert.DoesNotContain("\r\n", editorconfigContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EditorconfigConstant.UP_TEXT, editorconfigContent, StringComparison.Ordinal);
        Assert.DoesNotContain(EditorconfigConstant.BOTTOM_TEXT, editorconfigContent, StringComparison.Ordinal);
        Assert.Contains(EditorconfigConstant.UP_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal), editorconfigContent, StringComparison.Ordinal);
        Assert.Contains(EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal), editorconfigContent, StringComparison.Ordinal);
    }
}
