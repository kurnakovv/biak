// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class EnableCommandTests
{
    [Fact]
    public async Task RunWithoutBiakFolderAsync()
    {
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithoutBiakFolderAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        await EnableCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunWithoutEditorconfigFileAsync()
    {
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string templateEditorconfigMain = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
            ".biak",
            ".editorconfig-main"
        );

        File.Copy(
            sourceFileName: templateEditorconfigMain,
            destFileName: Path.Join(biakDir, ".editorconfig-main"),
            overwrite: true
        );

        await EnableCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunWithEditorconfigAsync()
    {
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithEditorconfigAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        string biakCategoriesDir = Path.Join(testDir.Value, ".biak", "Categories");
        Directory.CreateDirectory(biakCategoriesDir);

        string templateDisabledEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(templateDisabledEditorconfig);

        string templateEditorconfigMain = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
            ".biak",
            ".editorconfig-main"
        );

        File.Copy(
            sourceFileName: templateEditorconfigMain,
            destFileName: Path.Join(biakDir, ".editorconfig-main"),
            overwrite: true
        );

        string templateEditorconfigRoslynator = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Import",
            ".biak",
            "Categories",
            ".editorconfig-Roslynator"
        );

        File.Copy(
            sourceFileName: templateEditorconfigRoslynator,
            destFileName: Path.Join(biakCategoriesDir, ".editorconfig-Roslynator"),
            overwrite: true
        );

        string templateEditorconfigStyleCop = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Import",
            ".biak",
            "Categories",
            ".editorconfig-StyleCop"
        );

        File.Copy(
            sourceFileName: templateEditorconfigStyleCop,
            destFileName: Path.Join(biakCategoriesDir, ".editorconfig-StyleCop"),
            overwrite: true
        );

        await EnableCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.START_ENABLE, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.END_ENABLE, result, StringComparison.OrdinalIgnoreCase);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        Assert.True(File.Exists(editorconfigFile));

        string contentAfterEnable = await File.ReadAllTextAsync(editorconfigFile);
        string templateEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        string expectedContent = await File.ReadAllTextAsync(templateEditorconfig);
        expectedContent = EditorconfigHelper.AddAttentionBanners(expectedContent);

        Assert.Equal(expectedContent, contentAfterEnable);
    }

    [Fact]
    public async Task RunWithIncludeExcludeFiltersAsync()
    {
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithIncludeExcludeFiltersAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        string templateDisabledEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LegacyExample",
            "Disabled",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(templateDisabledEditorconfig);

        string templateEditorconfigMain = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LegacyExample",
            ".biak",
            ".editorconfig-main"
        );

        File.Copy(
            sourceFileName: templateEditorconfigMain,
            destFileName: Path.Join(biakDir, ".editorconfig-main"),
            overwrite: true
        );

        await EnableCommand.RunAsync(context);

        string result = output.ToString();
        Assert.Contains(UIConstant.START_ENABLE, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.END_ENABLE, result, StringComparison.OrdinalIgnoreCase);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        Assert.True(File.Exists(editorconfigFile));

        string contentAfterEnable = await File.ReadAllTextAsync(editorconfigFile);
        string templateEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LegacyExample",
            "Enabled",
            ".editorconfig"
        );
        string expectedContent = await File.ReadAllTextAsync(templateEditorconfig);

        Assert.Equal(expectedContent, contentAfterEnable);
    }

    [Fact]
    public async Task RunWithEmptyEditorconfigMainAsync()
    {
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithEmptyEditorconfigMainAsync)}");

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        await File.WriteAllTextAsync(Path.Join(biakDir, ".editorconfig-main"), string.Empty);
        await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), string.Empty);

        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };
        await EnableCommand.RunAsync(context);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        string contentAfterEnable = await File.ReadAllTextAsync(editorconfigFile);

        Assert.Equal(string.Empty, contentAfterEnable);
    }
}
