// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class DisableCommandTests
{
    [Fact]
    public async Task RunWithoutBiakFolderAsync()
    {
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithoutBiakFolderAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await DisableCommand.RunAsync(context);

            string result = output.ToString();
            Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task RunWithoutEditorconfigFileAsync()
    {
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

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

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await DisableCommand.RunAsync(context);

            string result = output.ToString();
            Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunWithEditorconfigAsync(bool withConfig)
    {
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithEditorconfigAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        string biakCategoriesDir = Path.Join(testDir.Value, ".biak", "Categories");
        Directory.CreateDirectory(biakCategoriesDir);

        string templateEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(templateEditorconfig);

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

        if (withConfig)
        {
            string templateConfig = Path.Join(
                AppContext.BaseDirectory,
                "Templates",
                "custom-config.json"
            );

            File.Copy(
                sourceFileName: templateConfig,
                destFileName: Path.Join(biakDir, "config.json"),
                overwrite: true
            );
        }

        Directory.SetCurrentDirectory(testDir.Value);

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await DisableCommand.RunAsync(context);

            string result = output.ToString();
            Assert.Contains(UIConstant.START_DISABLE, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.END_DISABLE, result, StringComparison.OrdinalIgnoreCase);
            if (withConfig)
            {
                Assert.DoesNotContain(BiakConfigConstant.FILE_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.Contains(BiakConfigConstant.FILE_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
            }
            Assert.DoesNotContain(BiakConfigConstant.SEVERITIES_TO_DISABLE_NULL_OR_EMPTY, result, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(BiakConfigConstant.SEVERITIES_TO_DISABLE_DUPLICATES, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        Assert.True(File.Exists(editorconfigFile));

        string contentAfterDisable = await File.ReadAllTextAsync(editorconfigFile);
        string templateDisabledEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
            withConfig ? ".editorconfig-with-suggestion" : ".editorconfig"
        );
        string expectedContent = await File.ReadAllTextAsync(templateDisabledEditorconfig);

        Assert.Equal(expectedContent, contentAfterDisable);
    }

    [Fact]
    public async Task RunWithIncludeExcludeFiltersAsync()
    {
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithIncludeExcludeFiltersAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        string templateEnabledEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LegacyExample",
            "Enabled",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(templateEnabledEditorconfig);

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

        Directory.SetCurrentDirectory(testDir.Value);

        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await DisableCommand.RunAsync(context);

            string result = output.ToString();
            Assert.Contains(UIConstant.START_DISABLE, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.END_DISABLE, result, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(BiakConfigConstant.SEVERITIES_TO_DISABLE_NULL_OR_EMPTY, result, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(BiakConfigConstant.SEVERITIES_TO_DISABLE_DUPLICATES, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        Assert.True(File.Exists(editorconfigFile));

        string contentAfterDisable = await File.ReadAllTextAsync(editorconfigFile);
        string templateDisabledEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LegacyExample",
            "Disabled",
            ".editorconfig"
        );
        string expectedContent = await File.ReadAllTextAsync(templateDisabledEditorconfig);

        Assert.Equal(expectedContent, contentAfterDisable);
    }

    [Fact]
    public async Task RunWithEmptyEditorconfigMainAsync()
    {
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithEmptyEditorconfigMainAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

        await File.WriteAllTextAsync(Path.Join(biakDir, ".editorconfig-main"), string.Empty);
        await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), string.Empty);

        await DisableCommand.RunAsync(context);

        string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
        string contentAfterDisable = await File.ReadAllTextAsync(editorconfigFile);

        Assert.Equal(string.Empty, contentAfterDisable);
    }
}
