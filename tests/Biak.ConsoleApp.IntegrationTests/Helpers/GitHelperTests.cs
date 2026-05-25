// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biak.ConsoleApp.Exceptions;
using Biak.ConsoleApp.Helpers;

namespace Biak.ConsoleApp.IntegrationTests.Helpers;

public class GitHelperTests
{
    [Fact]
    public async Task RunTestInvalidCommandAsync()
    {
        await Assert.ThrowsAsync<BiakApplicationException>(async () => await GitHelper.RunAsync("invalid-command"));
    }
}
