// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class EnableCommandTests
{
    [Fact]
    public async Task RunWithoutBiakFolderAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await EnableCommand.RunAsync();

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
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");

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
                await EnableCommand.RunAsync();

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

    [Fact]
    public async Task RunWithEditorconfigAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithEditorconfigAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await EnableCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.START_ENABLE, result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(UIConstant.END_ENABLE, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

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
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunWithIncludeExcludeFiltersAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(EnableCommandTests)}_{nameof(RunWithIncludeExcludeFiltersAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await EnableCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.START_ENABLE, result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(UIConstant.END_ENABLE, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

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
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
