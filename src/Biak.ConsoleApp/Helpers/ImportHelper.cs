// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Biak.ConsoleApp.Constants;
using Biak.ConsoleApp.Enums;
using Biak.ConsoleApp.Exceptions;

namespace Biak.ConsoleApp.Helpers;

/// <summary>
/// ^biak^ import "..." helper.
/// </summary>
public static class ImportHelper
{
    private const long MAX_SIZE = 5_000_000; // 5 MB

    private static readonly HttpClient s_httpClient = new(
        new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            CheckCertificateRevocationList = true,
        }
    )
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
    /// <param name="onImportFailure">What to do on import failure.</param>
    /// <returns>Replaced content with imports.</returns>
    public static async Task<string> ReplaceAsync(string content, FailureBehaviorType onImportFailure)
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

            string? replacement = await ResolveImportAsync(value, biakFullPath, newline, onImportFailure);

            if (replacement != null)
            {
                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, replacement);
            }
        }

        return sb.ToString();
    }

    private static async Task<string?> ResolveImportAsync(
        string value,
        string biakFullPath,
        string newline,
        FailureBehaviorType onImportFailure
    )
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Scheme != Uri.UriSchemeHttps || !await IsSafeUriAsync(uri))
            {
                HandleFailureBehavior(onImportFailure, $"{ImportConstant.URL_NOT_ALLOWED} {value}");
                return null;
            }

            try
            {
                using HttpResponseMessage response = await s_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    HandleFailureBehavior(onImportFailure, $"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (HTTP {(int)response.StatusCode} {response.ReasonPhrase})");
                    return null;
                }

                if (response.Content.Headers.ContentLength is > MAX_SIZE)
                {
                    HandleFailureBehavior(onImportFailure, $"{ImportConstant.RESPONSE_TOO_LARGE} {value}");
                    return null;
                }

                if (!response.Content.Headers.ContentType?.MediaType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    HandleFailureBehavior(onImportFailure, $"{ImportConstant.INVALID_CONTENT_TYPE} {value}");
                    return null;
                }

                string? content = await ReadWithLimitAsync(response.Content, MAX_SIZE, newline);

                if (content == null)
                {
                    HandleFailureBehavior(onImportFailure, $"{ImportConstant.RESPONSE_TOO_LARGE} {value}");
                    return null;
                }

                return content;
            }
            catch (Exception ex) when (ex is not BiakApplicationException and NotImplementedException)
            {
                HandleFailureBehavior(onImportFailure, $"{ImportConstant.UNABLE_TO_RETRIEVE_CONTENT_FROM_LINK} {value} (Reason: {ex.Message})");
                return null;
            }
        }

        string fullPath = Path.GetFullPath(
            Path.Join(biakFullPath, value)
        );

        if (!fullPath.StartsWith(biakFullPath, StringComparison.OrdinalIgnoreCase))
        {
            HandleFailureBehavior(onImportFailure, $"{ImportConstant.FORBIDDEN_OUTSIDE} {value}");
            return null;
        }

        if (!File.Exists(fullPath))
        {
            HandleFailureBehavior(onImportFailure, $"{ImportConstant.FILE_NOT_FOUND} {value}");
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

            return !addresses.Any(IsPrivateIp);
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

    private static void HandleFailureBehavior(FailureBehaviorType onImportFailure, string message)
    {
        switch (onImportFailure)
        {
            case FailureBehaviorType.Nothing:
            {
                return;
            }
            case FailureBehaviorType.Warning:
            {
                Console.WriteLine(message);
                return;
            }
            case FailureBehaviorType.Error:
            {
                throw new BiakApplicationException(message);
            }
            default:
            {
                throw new NotImplementedException(ImportConstant.FAILURE_BEHAVIOR_TYPE_NOT_IMPLEMENTED);
            }
        }
    }

    private static async Task<string?> ReadWithLimitAsync(
        HttpContent content,
        long maxBytes,
        string newline
    )
    {
        await using Stream stream = await content.ReadAsStreamAsync();

        await using MemoryStream ms = new();
        byte[] buffer = new byte[81920]; // 80 KB

        long totalBytes = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            totalBytes += read;

            if (totalBytes > maxBytes)
            {
                return null;
            }

            await ms.WriteAsync(buffer.AsMemory(0, read));
        }

        ms.Position = 0;

        Encoding encoding = Encoding.UTF8;
        string? charset = content.Headers.ContentType?.CharSet?.Trim('"');
        if (!string.IsNullOrWhiteSpace(charset))
        {
            try
            {
                encoding = Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                encoding = Encoding.UTF8;
            }
        }
        using StreamReader reader = new(ms, encoding, detectEncodingFromByteOrderMarks: true);
        string result = await reader.ReadToEndAsync();

        return NormalizeLineEndings(result, newline);
    }
}
