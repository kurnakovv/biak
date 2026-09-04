// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp.UnitTests.ExceptionsTests;

public class BiakApplicationExceptionTests
{
    [Fact]
    public void ToStringTest()
    {
        const string MESSAGE = "Test error message";
        BiakApplicationException exception = new(MESSAGE);
        Assert.Equal(MESSAGE, exception.ToString());
    }
}
