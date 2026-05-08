// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// Global json options helper.
/// </summary>
public static class GlobalJsonSerializerOptionsHelper
{
    /// <summary>
    /// Global json options value.
    /// </summary>
    public static JsonSerializerOptions Value { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
