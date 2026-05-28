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
        string message = "Test error message";
        BiakApplicationException exception = new(message);
        Assert.Equal(message, exception.ToString());
    }
}
