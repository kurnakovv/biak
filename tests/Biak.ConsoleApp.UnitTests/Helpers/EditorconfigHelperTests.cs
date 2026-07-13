// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class EditorconfigHelperTests
{
    [Fact]
    public void AddAttentionBannersShouldUseLfWhenContentUsesLf()
    {
        string content = "root = true\n[*]\nindent_style = space\n";

        string result = EditorconfigHelper.AddAttentionBanners(content);

        Assert.StartsWith(
            EditorconfigConstant.UP_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal),
            result,
            StringComparison.Ordinal
        );

        Assert.EndsWith(
            EditorconfigConstant.BOTTOM_TEXT.Replace("\r\n", "\n", StringComparison.Ordinal),
            result,
            StringComparison.Ordinal
        );

        Assert.Contains("\n[*]\n", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void AddAttentionBannersShouldUseCrLfWhenContentUsesCrLf()
    {
        string content = "root = true\r\n[*]\r\nindent_style = space\r\n";

        string result = EditorconfigHelper.AddAttentionBanners(content);

        Assert.StartsWith(EditorconfigConstant.UP_TEXT, result, StringComparison.Ordinal);
        Assert.EndsWith(EditorconfigConstant.BOTTOM_TEXT, result, StringComparison.Ordinal);

        Assert.Contains("\r\n[*]\r\n", result, StringComparison.Ordinal);
    }

    [Fact]
    public void AddAttentionBannersShouldNotModifyOriginalContent()
    {
        string content = "root = true\ncustom=value\ncustom=value2\r\n";

        string result = EditorconfigHelper.AddAttentionBanners(content);

        Assert.Contains(content, result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEnabledContentAsyncWhenContentIsNullOrWhiteSpaceReturnsContentUnchangedAsync(string? content)
    {
        string? result = await EditorconfigHelper.GetEnabledContentAsync(content!, new BiakConfig());

        Assert.Equal(content, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetDisabledContentAsyncWhenContentIsNullOrWhiteSpaceReturnsContentUnchangedAsync(string? content)
    {
        string? result = await EditorconfigHelper.GetDisabledContentAsync(content!, new BiakConfig());

        Assert.Equal(content, result);
    }
}
