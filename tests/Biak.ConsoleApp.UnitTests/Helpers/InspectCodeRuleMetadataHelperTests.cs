// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers.Baseline.InspectCode;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class InspectCodeRuleMetadataHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("UnknownRuleId")]
    public void GetReturnsNullWhenRuleIdIsInvalid(string? ruleId)
    {
        Assert.Null(InspectCodeRuleMetadataHelper.Get(ruleId));
    }

    [Theory]
    [InlineData("InvocationIsSkipped")]
    [InlineData("ArrangeTypeModifiers")]
    [InlineData("FunctionNeverReturns")]
    public void GetReturnsObjectWhenRuleIdIsValid(string ruleId)
    {
        Assert.NotNull(InspectCodeRuleMetadataHelper.Get(ruleId));
    }
}
