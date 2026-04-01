// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class SetupCommandTests
{
    [Fact]
    public async Task RunWithoutEditorconfigFileAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await SetupCommand.RunAsync();

            string result = output.ToString();
            Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task RunWithEditorconfigAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();

        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWithEditorconfigAsync)}");

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await SetupCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.START_SETUP, result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(UIConstant.END_SETUP, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

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
            string configContent = await File.ReadAllTextAsync(configPath);
            string templateConfigContent = await File.ReadAllTextAsync(templateConfigPath);
            Assert.Equal(templateConfigContent, configContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();

        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWhenBiakFolderExistsAndUserPressesEnterShouldNotRecreateAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            TextReader originalIn = Console.In;

            await using StringWriter output = new();
            using StringReader input = new(string.Empty);

            Console.SetOut(output);
            Console.SetIn(input);

            try
            {
                await SetupCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.BIAK_FOLDER_ALREADY_EXISTS, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetIn(originalIn);
            }

            Assert.True(File.Exists(oldFile));
            Assert.False(File.Exists(Path.Join(biakDir, ".editorconfig-main")));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();

        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWhenBiakFolderExistsAndUserTypesYShouldRecreateAsync)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            TextReader originalIn = Console.In;

            await using StringWriter output = new();
            using StringReader input = new(UIConstant.CONFIRM);

            Console.SetOut(output);
            Console.SetIn(input);

            try
            {
                await SetupCommand.RunAsync();

                string result = output.ToString();
                Assert.Contains(UIConstant.BIAK_FOLDER_ALREADY_EXISTS, result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(UIConstant.START_SETUP, result, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(UIConstant.END_SETUP, result, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetIn(originalIn);
            }

            string editorconfigMainPath = Path.Join(biakDir, ".editorconfig-main");
            Assert.False(File.Exists(oldFile));
            Assert.True(File.Exists(editorconfigMainPath));
            string editorconfigMainContent = await File.ReadAllTextAsync(editorconfigMainPath);
            Assert.DoesNotContain(EditorconfigConstant.UP_TEXT, editorconfigMainContent, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(EditorconfigConstant.BOTTOM_TEXT, editorconfigMainContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task RunWithLFEditorconfigAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();

        TestDirectory testDir = new($"{nameof(SetupCommandTests)}_{nameof(RunWithLFEditorconfigAsync)}");

        string template = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "LF",
            ".editorconfig"
        );
        testDir.CopyTemplateEditorconfig(template);

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            await SetupCommand.RunAsync();

            string editorconfigFile = Path.Join(testDir.Value, ".editorconfig");
            string editorconfigContent = await File.ReadAllTextAsync(editorconfigFile);

            Assert.DoesNotContain("\r\n", editorconfigContent, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(EditorconfigConstant.UP_TEXT, editorconfigContent, StringComparison.Ordinal);
            Assert.DoesNotContain(EditorconfigConstant.BOTTOM_TEXT, editorconfigContent, StringComparison.Ordinal);
            Assert.Contains(EditorconfigConstant.UP_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal), editorconfigContent, StringComparison.Ordinal);
            Assert.Contains(EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal), editorconfigContent, StringComparison.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
