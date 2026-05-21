// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Exceptions;

/// <summary>
/// Exception for application errors that should terminate the application.
/// </summary>
public class BiakApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BiakApplicationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
#pragma warning disable SA1502 // Element should not be on a single line
    public BiakApplicationException(string message) : base(message) { }
#pragma warning restore SA1502 // Element should not be on a single line
}
