// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.IntegrationTests.Mock;

public class TestDirectory
{
    public TestDirectory(string methodName)
    {
        Value = Path.Combine(
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
            destFileName: Path.Combine(Value, ".editorconfig"),
            overwrite: true
        );
    }
}
