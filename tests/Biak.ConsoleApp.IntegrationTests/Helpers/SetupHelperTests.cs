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
    public async Task GetEditorconfigPathsWithoutFilesAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupHelperTests)}_{nameof(GetEditorconfigPathsWithoutFilesAsync)}");
        await using StringWriter output = new();
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value, Out = output };

        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(executionContext: context);

        string result = output.ToString();
        Assert.Contains(UIConstant.BIAK_NOT_INITIALIZED, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UIConstant.RUN_BIAK_SETUP, result, StringComparison.OrdinalIgnoreCase);
        Assert.Null(editorconfigPaths.Value);
        Assert.Null(editorconfigPaths.MainValue);
    }

    [Fact]
    public async Task GetEditorconfigPathsWithoutEditorconfigFileAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupHelperTests)}_{nameof(GetEditorconfigPathsWithoutEditorconfigFileAsync)}");
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

        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(executionContext: context);

        string result = output.ToString();
        Assert.Contains(UIConstant.EDITORCONFIG_NOT_FOUND, result, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(editorconfigPaths.MainValue);
        Assert.Null(editorconfigPaths.Value);
    }

    [Fact]
    public async Task GetEditorconfigPathsWithAllFilesAsync()
    {
        TestDirectory testDir = new($"{nameof(SetupHelperTests)}_{nameof(GetEditorconfigPathsWithAllFilesAsync)}");
        AppExecutionContext context = new() { WorkingDirectory = testDir.Value };

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

        EditorconfigPaths editorconfigPaths = await SetupHelper.GetEditorconfigPathsAsync(executionContext: context);

        Assert.NotNull(editorconfigPaths.Value);
        Assert.NotNull(editorconfigPaths.MainValue);
    }
}
