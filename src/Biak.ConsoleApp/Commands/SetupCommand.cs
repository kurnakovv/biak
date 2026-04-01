// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Commands;

/// <summary>
/// `dotnet biak setup` command.
/// </summary>
public static class SetupCommand
{
    /// <summary>
    /// Can `dotnet biak setup` command be run.
    /// </summary>
    /// <param name="args">User input arguments.</param>
    /// <returns>Can be run or not.</returns>
    public static bool IsRunnable(string[] args)
    {
        return args.Length == 1 && args[0] == CommandArgumentConstant.SETUP;
    }

    /// <summary>
    /// Run.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task RunAsync()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string editorConfigPath = Path.Join(currentDirectory, ".editorconfig");

        if (!File.Exists(editorConfigPath))
        {
            Console.WriteLine(UIConstant.EDITORCONFIG_NOT_FOUND + editorConfigPath);
            return;
        }

        string targetDir = ".biak";

        if (Directory.Exists(targetDir))
        {
            Console.WriteLine(UIConstant.BIAK_FOLDER_ALREADY_EXISTS);

            string? userInput = Console.ReadLine();
            bool recreateFolder = string.Equals(userInput, UIConstant.CONFIRM, StringComparison.OrdinalIgnoreCase);

            if (!recreateFolder)
            {
                return;
            }

            Directory.Delete(targetDir, recursive: true);
        }

        Console.WriteLine(UIConstant.START_SETUP);

        Directory.CreateDirectory(targetDir);

        string targetFile = Path.Join(targetDir, ".editorconfig-main");

        string editorconfigContent = await File.ReadAllTextAsync(editorConfigPath);

        string newline = editorconfigContent.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";

        string up = EditorconfigConstant.UP_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);
        string bottom = EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", newline, StringComparison.Ordinal);

        if (editorconfigContent.Contains(up, StringComparison.Ordinal))
        {
            editorconfigContent = editorconfigContent.Replace(up, string.Empty, StringComparison.Ordinal);
        }

        if (editorconfigContent.Contains(bottom, StringComparison.Ordinal))
        {
            editorconfigContent = editorconfigContent.Replace(bottom, string.Empty, StringComparison.Ordinal);
        }

        await File.WriteAllTextAsync(
            path: targetFile,
            contents: editorconfigContent
        );

        await File.WriteAllTextAsync(
            path: editorConfigPath,
            contents: EditorconfigHelper.AddAttentionBanners(editorconfigContent)
        );

        await File.WriteAllTextAsync(
            path: Path.Join(targetDir, "config.json"),
            contents: JsonSerializer.Serialize(
                value: new BiakConfig(),
                options: GlobalJsonSerializerOptionsHelper.Value
            ) + Environment.NewLine
        );

        Console.WriteLine(UIConstant.END_SETUP);
    }
}
