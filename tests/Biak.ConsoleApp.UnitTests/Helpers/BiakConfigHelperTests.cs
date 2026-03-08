// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class BiakConfigHelperTests
{
    [Theory]
    [InlineData(null, BiakConfigConstant.FILE_NOT_FOUND)]
    [InlineData("null", BiakConfigConstant.IS_NULL)]
    [InlineData("", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData("fdasfsdafasf", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData("{ fsdasfcfasdfasdsfad dfdfdfas ", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"someValue\"}", BiakConfigConstant.INVALID_FORMAT)]
    public async Task GetInvalidStringsAsync(string? json, string expectedMessage)
    {
        (string? resultMessage, _) = await BiakConfigHelper.GetAsync(json);

        Assert.Equal(expectedMessage, resultMessage);
    }

    [Theory]
    [InlineData("{}", SeverityLevelType.None)]
    [InlineData(/*lang=json,strict*/ "{\"someField\": \"someValue\"}", SeverityLevelType.None)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"none\"}", SeverityLevelType.None)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"suggestion\"}", SeverityLevelType.Suggestion)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"silent\"}", SeverityLevelType.Silent)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"error\"}", SeverityLevelType.Error)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"warning\"}", SeverityLevelType.Warning)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"default\"}", SeverityLevelType.Default)]
    public async Task GetValidStringsAsync(string? json, SeverityLevelType expectedSeverity)
    {
        (string? resultMessage, BiakConfig resultConfig) = await BiakConfigHelper.GetAsync(json);

        Assert.Null(resultMessage);
        Assert.NotNull(resultConfig);
        Assert.Equal(resultConfig.SeverityWhenDisabled, expectedSeverity);
    }
}
