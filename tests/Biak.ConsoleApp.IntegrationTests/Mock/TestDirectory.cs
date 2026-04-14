// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.IntegrationTests.Mock;

public class TestDirectory
{
    public TestDirectory(string methodName)
    {
        Value = Path.Join(
            AppContext.BaseDirectory,
            "_tests",
            methodName,
            Guid.NewGuid().ToString("N")
        );

        Directory.CreateDirectory(Value);
    }

    public string Value { get; }

    public void CopyTemplateEditorconfig(string filePath)
    {
        File.Copy(
            sourceFileName: filePath,
            destFileName: Path.Join(Value, ".editorconfig"),
            overwrite: true
        );
    }

    public void CopyDirectory(string sourceDir, bool overwrite = false)
    {
        DirectoryInfo dir = new(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(Value, file.Name);
            file.CopyTo(targetFilePath, overwrite);
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(Value, subDir.Name);
            CopyDirectory(subDir.FullName, overwrite);
        }
    }
}
