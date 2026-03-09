// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Models;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// .biak/config.json helper.
/// </summary>
public static class BiakConfigHelper
{
    private static readonly JsonSerializerOptions s_jso = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    /// <summary>
    /// Get .biak/config model.
    /// </summary>
    /// <param name="json">json content if you want to extract it not from '.biak/config.json' file.</param>
    /// <returns>Warning message and .biak/config model.</returns>
    public static async Task<(string? Message, BiakConfig Config)> GetAsync(string? json = null)
    {
        if (json == null)
        {
            string configPath = Path.Join(".biak", "config.json");
            if (!File.Exists(configPath))
            {
                return (BiakConfigConstant.FILE_NOT_FOUND, new BiakConfig());
            }
            json = await File.ReadAllTextAsync(configPath);
        }
        try
        {
            BiakConfig? config = JsonSerializer.Deserialize<BiakConfig>(json, s_jso);
            if (config == null)
            {
                return (BiakConfigConstant.IS_NULL, new BiakConfig());
            }
            return (null, config);
        }
        catch (JsonException)
        {
            return (BiakConfigConstant.INVALID_FORMAT, new BiakConfig());
        }
    }
}
