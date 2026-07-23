// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Commands;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.IntegrationTests.Mock;

namespace Biak.ConsoleApp.IntegrationTests.Commands;

public class StatusCommandTests
{
    [Fact]
    public async Task RunWithoutBiakFolderAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await StatusCommand.RunAsync([CommandArgumentConstant.STATUS]);

            string result = output.ToString().Trim();
            Assert.Equal(UIConstant.STATUS_BROKEN, result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task RunWithoutBiakFolderAndDebugInfoAsync()
    {
        TextWriter originalOut = Console.Out;
        await using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            await StatusCommand.RunAsync([CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO]);

            string result = output.ToString().Trim();
            Assert.Equal(
                UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.BIAK_NOT_INITIALIZED + " " + UIConstant.RUN_BIAK_SETUP,
                result
            );
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
        TestDirectory testDir = new($"{nameof(StatusCommandTests)}_{nameof(RunWithoutEditorconfigFileAsync)}");

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
                await StatusCommand.RunAsync([CommandArgumentConstant.STATUS]);

                string result = output.ToString().Trim();
                Assert.Equal(UIConstant.STATUS_BROKEN, result);
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
    public async Task RunWithoutEditorconfigFileAndDebugInfoAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(StatusCommandTests)}_{nameof(RunWithoutEditorconfigFileAndDebugInfoAsync)}");

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
                await StatusCommand.RunAsync([CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO]);

                string result = output.ToString().Trim();
                Assert.Equal(
                    UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + UIConstant.EDITORCONFIG_NOT_FOUND + Path.Join(testDir.Value, ".editorconfig"),
                    result
                );
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
    [InlineData(UIConstant.STATUS_ENABLED)]
    [InlineData(UIConstant.STATUS_DISABLED)]
    [InlineData(UIConstant.STATUS_UNSYNCHRONISED)]
    public async Task RunWithEditorconfigAsync(string expectedStatus)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(StatusCommandTests)}_{nameof(RunWithEditorconfigAsync)}_{expectedStatus}");

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

        string templateConfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            "default-config.json"
        );

        File.Copy(
            sourceFileName: templateConfig,
            destFileName: Path.Join(biakDir, "config.json"),
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
                if (expectedStatus == UIConstant.STATUS_ENABLED)
                {
                    await EnableCommand.RunAsync();
                }
                else if (expectedStatus == UIConstant.STATUS_DISABLED)
                {
                    await DisableCommand.RunAsync();
                }
                else if (expectedStatus == UIConstant.STATUS_UNSYNCHRONISED)
                {
                    await File.WriteAllTextAsync(Path.Join(testDir.Value, ".editorconfig"), "root = true\n");
                }
                else
                {
                    throw new NotImplementedException("Not implemented expected status: " + expectedStatus);
                }

                output.GetStringBuilder().Clear();

                await StatusCommand.RunAsync([CommandArgumentConstant.STATUS]);

                string result = output.ToString().Trim();
                Assert.Equal(expectedStatus, result);
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
    public async Task RunWithDebugInfoAndInvalidConfigAsync()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(StatusCommandTests)}_{nameof(RunWithDebugInfoAndInvalidConfigAsync)}");

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);

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

        await File.WriteAllTextAsync(Path.Join(biakDir, "config.json"), "{");

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            TextWriter originalOut = Console.Out;
            await using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                await StatusCommand.RunAsync([CommandArgumentConstant.STATUS, CommandArgumentConstant.DEBUG_INFO]);

                string result = output.ToString().Trim();
                Assert.Equal(UIConstant.STATUS_BROKEN_WITH_CONFIG_MESSAGE + BiakConfigConstant.INVALID_FORMAT, result);
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
}
