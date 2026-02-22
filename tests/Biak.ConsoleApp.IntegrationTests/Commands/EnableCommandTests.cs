// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
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

        string templateEditorconfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "Disabled",
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
            string expectedContent = await File.ReadAllTextAsync(templateEditorconfigMain);

            Assert.Equal(expectedContent, contentAfterEnable);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
