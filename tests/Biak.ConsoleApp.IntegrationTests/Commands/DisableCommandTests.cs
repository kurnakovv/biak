// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class DisableCommandTests
{
    [Fact]
    public async Task RunWithoutBiakFolderAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await DisableCommand.RunAsync();

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
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await DisableCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunWithEditorconfigAsync(bool withConfig)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithEditorconfigAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await DisableCommand.RunAsync();

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
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunWithIncludeExcludeFiltersAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(DisableCommandTests)}_{nameof(RunWithIncludeExcludeFiltersAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await DisableCommand.RunAsync();

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
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
