// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// ^biak^ import "..." helper.
/// </summary>
public static class ImportHelper
{
    private static readonly HttpClient s_httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    private static readonly Regex s_importRegex = new(
        @"^(?!\s*#)[ \t]*\^biak\^\s*import\s*(?:""([^""]+)""|(\S+))",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    /// <summary>
    /// Replace imports with file content.
    /// </summary>
    /// <param name="content">.editorconfig content.</param>
    /// <returns>Replaced content with imports.</returns>
    public static async Task<string> ReplaceAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        MatchCollection matches = s_importRegex.Matches(content);
        if (matches.Count == 0)
        {
            return content;
        }

        string biakDir = Path.Join(Directory.GetCurrentDirectory(), ".biak");
        string biakFullPath = Path.GetFullPath(biakDir);

        string newline = content.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";

        StringBuilder sb = new(content);

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];

            string value = match.Groups[1].Success
                ? match.Groups[1].Value
                : match.Groups[2].Value;

            string? replacement = await ResolveImportAsync(value, biakFullPath, newline);

            if (replacement != null)
            {
                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
            }
        }

        return sb.ToString();
    }

    private static async Task<string?> ResolveImportAsync(string value, string biakFullPath, string newline)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Scheme != Uri.UriSchemeHttps || !await IsSafeUriAsync(uri))
            {
                Console.WriteLine($"{ImportConstant.INVALID_URL_FORMAT} {value}");
                return null;
            }

            try
            {
                using HttpResponseMessage response = await s_httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (HTTP {(int)response.StatusCode} {response.ReasonPhrase})");
                    return null;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                return NormalizeLineEndings(responseContent, newline);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (Reason: {ex.Message})");
                return null;
            }
        }

        string fullPath = Path.GetFullPath(
            Path.Join(biakFullPath, value)
        );

        if (!fullPath.StartsWith(biakFullPath, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{ImportConstant.FORBIDDEN_OUTSIDE} {value}");
            return null;
        }

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"{ImportConstant.FILE_NOT_FOUND} {value}");
            return null;
        }

        return await File.ReadAllTextAsync(fullPath);
    }

    private static string NormalizeLineEndings(string content, string newline)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", newline, StringComparison.Ordinal);
    }

    private static async Task<bool> IsSafeUriAsync(Uri uri)
    {
        try
        {
            string host = uri.Host;

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (IPAddress.TryParse(host, out IPAddress? ip))
            {
                return !IsPrivateIp(ip);
            }

            IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);

            if (addresses.Any(IsPrivateIp))
            {
                return true;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPrivateIp(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();

        // IPv4
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return bytes[0] switch
            {
                10 => true,
                127 => true,
                169 when bytes[1] == 254 => true,
                172 when bytes[1] is >= 16 and <= 31 => true,
                192 when bytes[1] == 168 => true,
                _ => false,
            };
        }

        // IPv6
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return IPAddress.IsLoopback(ip) ||
                   ip.IsIPv6LinkLocal ||
                   ip.IsIPv6SiteLocal;
        }

        return false;
    }
}
