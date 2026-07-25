// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Models;

/// <summary>
/// Execution context for commands and helpers.
/// </summary>
public class AppExecutionContext
{
    /// <summary>
    /// Working directory used for path resolution and process execution.
    /// </summary>
    public string WorkingDirectory { get; init; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// Creates the default context for the current process.
    /// </summary>
    /// <returns>Default execution context.</returns>
    public static AppExecutionContext CreateDefault()
    {
        return new AppExecutionContext();
    }
}
