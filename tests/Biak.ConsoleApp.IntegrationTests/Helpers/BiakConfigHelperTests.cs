// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Helpers;
using Biak.ConsoleApp.IntegrationTests.Mock;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class BiakConfigHelperTests
{
    [Theory]
    [InlineData("default-config", null, SeverityLevelType.None, FailureBehaviorType.Warning)]
    [InlineData("custom-config", null, SeverityLevelType.Suggestion, FailureBehaviorType.Error)]
    [InlineData("null-config", BiakConfigConstant.IS_NULL, SeverityLevelType.None, FailureBehaviorType.Warning)]
    [InlineData("invalid-config", BiakConfigConstant.INVALID_FORMAT, SeverityLevelType.None, FailureBehaviorType.Warning)]
    [InlineData("null-severities-to-disable", BiakConfigConstant.SEVERITIES_TO_DISABLE_NULL_OR_EMPTY, SeverityLevelType.None, FailureBehaviorType.Warning)]
    [InlineData("empty-severities-to-disable", BiakConfigConstant.SEVERITIES_TO_DISABLE_NULL_OR_EMPTY, SeverityLevelType.None, FailureBehaviorType.Warning)]
    [InlineData("duplicate-severities-to-disable", BiakConfigConstant.SEVERITIES_TO_DISABLE_DUPLICATES, SeverityLevelType.None, FailureBehaviorType.Warning)]
    public async Task GetConfigModelAsync(string templateName, string? expectedMessage, SeverityLevelType expectedSeverity, FailureBehaviorType expectedOnImportFailure)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        TestDirectory testDir = new($"{nameof(BiakConfigHelperTests)}_{nameof(GetConfigModelAsync)}");

        string biakDir = Path.Join(testDir.Value, ".biak");
        Directory.CreateDirectory(biakDir);
        string templateConfig = Path.Join(
            AppContext.BaseDirectory,
            "Templates",
            $"{templateName}.json"
        );

        File.Copy(
            sourceFileName: templateConfig,
            destFileName: Path.Join(biakDir, "config.json"),
            overwrite: true
        );

        try
        {
            Directory.SetCurrentDirectory(testDir.Value);

            (string? resultMessage, BiakConfig resultConfig) = await BiakConfigHelper.GetAsync();

            Assert.Equal(expectedMessage, resultMessage);
            Assert.Equal(expectedSeverity, resultConfig.SeverityWhenDisabled);
            Assert.NotNull(resultConfig.SeveritiesToDisable);
            Assert.NotEmpty(resultConfig.SeveritiesToDisable);
            Assert.Equal(expectedOnImportFailure, resultConfig.OnImportFailure);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
