// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.UnitTests.Helpers;

public class AlwaysEnabledRulesHelperTests
{
    [Fact]
    public void ProtectSeveritiesWhenStartMarkerHasNoEndMarkerReturnsSeveritiesUnprotected()
    {
        string input = @"
^biak^ always-enabled start
dotnet_diagnostic.CA9999.severity = error
";

        (string content, Dictionary<string, string> placeholders) = AlwaysEnabledRulesHelper.ProtectSeverities(input);

        Assert.Equal(input, content);
        Assert.Empty(placeholders);
    }

    [Fact]
    public void ProtectSeveritiesWhenBlockIsEmptySkipsBlockAndContinues()
        string input =
            """

            ^biak^ always-enabled start
            ^biak^ always-enabled end
            dotnet_diagnostic.CA9999.severity = error

            """;

        (string content, Dictionary<string, string> placeholders) = AlwaysEnabledRulesHelper.ProtectSeverities(input);

        Assert.Equal(input, content);
        Assert.Empty(placeholders);
    }
}
