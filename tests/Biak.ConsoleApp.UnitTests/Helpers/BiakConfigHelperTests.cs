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
    public static TheoryData<string?, string, IEnumerable<SeverityLevelType>> SeveritiesToDisableData => new()
    {
        {
            null,
            "{}",
            BiakConfig.s_defaultSeveritiesToDisable
        },
        {
            null,
            /*lang=json,strict*/ "{\"severitiesToDisable\": [\"error\", \"warning\"]}",
            [SeverityLevelType.Error, SeverityLevelType.Warning]
        },
        {
            null,
            /*lang=json,strict*/ "{\"severitiesToDisable\": [\"error\", \"warning\", \"suggestion\", \"silent\", \"default\"]}",
            [
                SeverityLevelType.Error,
                SeverityLevelType.Warning,
                SeverityLevelType.Suggestion,
                SeverityLevelType.Silent,
                SeverityLevelType.Default,
            ]
        },
        {
            null,
            /*lang=json,strict*/ "{\"severitiesToDisable\": [\"none\", \"error\", \"warning\", \"suggestion\", \"silent\", \"default\"]}",
            Enum.GetValues<SeverityLevelType>()
        },
        {
            BiakConfigConstant.SEVERETIES_TO_DISABLE_DUPLICATES,
            /*lang=json,strict*/ "{\"severitiesToDisable\": [\"error\", \"warning\", \"warning\", \"suggestion\", \"silent\"]}",
            [
                SeverityLevelType.Error,
                SeverityLevelType.Warning,
                SeverityLevelType.Suggestion,
                SeverityLevelType.Silent,
            ]
        },
        {
            BiakConfigConstant.SEVERETIES_TO_DISABLE_IS_EMPTY,
            /*lang=json,strict*/ "{\"severitiesToDisable\": []}",
            BiakConfig.s_defaultSeveritiesToDisable
        },
    };

    [Theory]
    [InlineData(null, BiakConfigConstant.FILE_NOT_FOUND)]
    [InlineData("null", BiakConfigConstant.IS_NULL)]
    [InlineData("", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData("fdasfsdafasf", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData("{ fsdasfcfasdfasdsfad dfdfdfas ", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData(/*lang=json,strict*/ "{\"severityWhenDisabled\": \"someValue\"}", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData(/*lang=json,strict*/ "{\"severitiesToDisable\": \"someValue\"}", BiakConfigConstant.INVALID_FORMAT)]
    [InlineData(/*lang=json,strict*/ "{\"severitiesToDisable\": [\"someValue\", \"someValue2\"]}", BiakConfigConstant.INVALID_FORMAT)]
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
    public async Task GetValidStringsForSeverityWhenDisabledAsync(string json, SeverityLevelType expectedSeverity)
    {
        (string? resultMessage, BiakConfig resultConfig) = await BiakConfigHelper.GetAsync(json);

        Assert.Null(resultMessage);
        Assert.NotNull(resultConfig);
        Assert.Equal(expectedSeverity, resultConfig.SeverityWhenDisabled);
    }

    [Theory]
    [MemberData(nameof(SeveritiesToDisableData))]
    public async Task GetValidStringsForSeveritiesToDisableAsync(string? expectedMessage, string json, IEnumerable<SeverityLevelType> expectedSeveritiesToDisable)
    {
        (string? resultMessage, BiakConfig resultConfig) = await BiakConfigHelper.GetAsync(json);

        Assert.Equal(expectedMessage, resultMessage);
        Assert.NotNull(resultConfig);
        Assert.Equal(expectedSeveritiesToDisable, resultConfig.SeveritiesToDisable);
    }

    [Fact]
    public async Task GetAllPropertiesAsync()
    {
        string json = /*lang=json,strict*/ "{\"severityWhenDisabled\": \"suggestion\", \"severitiesToDisable\": [\"error\", \"warning\"]}";

        (string? resultMessage, BiakConfig resultConfig) = await BiakConfigHelper.GetAsync(json);

        Assert.Null(resultMessage);
        Assert.NotNull(resultConfig);
        Assert.Equal(SeverityLevelType.Suggestion, resultConfig.SeverityWhenDisabled);
        Assert.Equal([SeverityLevelType.Error, SeverityLevelType.Warning], resultConfig.SeveritiesToDisable);
    }
}
