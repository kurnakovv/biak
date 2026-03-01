// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.UnitTests.Constants;

public class EditorconfigConstantTests
{
    [Fact]
    public void UpAndBottomConstantsContainCrLf()
    {
        Assert.Contains("\r\n", EditorconfigConstant.UP_TEXT, StringComparison.Ordinal);
        Assert.Contains("\r\n", EditorconfigConstant.BOTTOM_TEXT, StringComparison.Ordinal);
    }
}
