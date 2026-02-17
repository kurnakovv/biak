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

        TestDirectory testDir = new(nameof(RunWithEditorconfigAsync));

        string template = Path.Combine(
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

            string editorconfigMainFile = Path.Combine(testDir.Value, ".biak", ".editorconfig-main");
            bool editorconfigFileExists = File.Exists(editorconfigMainFile);

            Assert.True(editorconfigFileExists);
            using StreamReader readerTemplate = new(template);
            using StreamReader readerEditorconfigMain = new(editorconfigMainFile);
            string templateContent = await readerTemplate.ReadToEndAsync();
            string editorconfigMainContent = await readerEditorconfigMain.ReadToEndAsync();

            Assert.Equal(templateContent, editorconfigMainContent);
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
            Assert.False(File.Exists(Path.Combine(biakDir, ".editorconfig-main")));
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

            Assert.False(File.Exists(oldFile));
            Assert.True(File.Exists(Path.Combine(biakDir, ".editorconfig-main")));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
