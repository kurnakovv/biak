// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class SetupHelperTests
{
    [Fact]
    public void GetEditorconfigPathsWithoutFiles()
    {
        TextWriter originalOut = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);

        try
        {
            EditorconfigPaths editorconfigPaths = SetupHelper.GetEditorconfigPaths();

            string result = output.ToString();
            Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
            Assert.Null(editorconfigPaths.Value);
            Assert.Null(editorconfigPaths.MainValue);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void GetEditorconfigPathsWithoutEditorconfigFile()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(SetupHelperTests)}_{nameof(GetEditorconfigPathsWithoutEditorconfigFile)}");

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
            using StringWriter output = new();
            Console.SetOut(output);

            try
            {
                EditorconfigPaths editorconfigPaths = SetupHelper.GetEditorconfigPaths();

                string result = output.ToString();
                Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
                Assert.NotNull(editorconfigPaths.MainValue);
                Assert.Null(editorconfigPaths.Value);
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
    public void GetEditorconfigPathsWithAllFiles()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(SetupHelperTests)}_{nameof(GetEditorconfigPathsWithAllFiles)}");

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

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);
            EditorconfigPaths editorconfigPaths = SetupHelper.GetEditorconfigPaths();

            Assert.NotNull(editorconfigPaths.Value);
            Assert.NotNull(editorconfigPaths.MainValue);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
