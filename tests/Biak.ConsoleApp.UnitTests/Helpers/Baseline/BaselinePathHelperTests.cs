// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers.Baseline;

namespace Biak.ConsoleApp.UnitTests.Helpers.Baseline;

public class BaselinePathHelperTests
{
    [Theory]
    [InlineData(".editorconfig", true)]
    [InlineData(".biak/.editorconfig-main", true)]
    [InlineData(".editorconfig-legacy", true)]
    [InlineData(".editorconfig-custom", true)]
    [InlineData("src/nested/.editorconfig", true)]
    [InlineData(".editorconfig/test.txt", false)]
    [InlineData("src/.editorconfig/.editorconfig-main", false)]
    [InlineData("test.editorconfig", false)]
    [InlineData("src/test.editorconfig", false)]
    [InlineData("../.editorconfig", false)]
    [InlineData("../../.editorconfig", false)]
    [InlineData("../other-project/.editorconfig", false)]
    [InlineData("appsettings.json", false)]
    public void IsSafeReturnsExpectedResult(string relativePath, bool expected)
    {
        string baseDir = Path.Join(Path.GetTempPath(), "biak-test-safe");

        bool result = BaselinePathHelper.IsSafe(relativePath, baseDir);

        Assert.Equal(expected, result);
    }
}
