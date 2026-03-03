// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// .editorconfig rule severity helper.
/// </summary>
public static class SeverityHelper
{
    /// <summary>
    /// Disable severity.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <returns>Disabled content.</returns>
    public static string Disable(string content)
    {
#pragma warning disable SYSLIB1045
        return Regex.Replace(content, @"=\s*(error|warning|suggestion)", "= none");
#pragma warning restore SYSLIB1045
    }
}
