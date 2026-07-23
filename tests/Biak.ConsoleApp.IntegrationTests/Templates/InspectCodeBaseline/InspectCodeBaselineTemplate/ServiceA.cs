// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace InspectCodeBaselineTemplate;

// Repeated violations of Rule 1: ConvertToConstant.Local (x3)
// All three private fields are never mutated and can be converted to constants.
public class ServiceA
{
    private int _timeout = 30;
    private string _prefix = "LOG";
    private bool _enabled = true;

    public string Format(string message) =>
        _enabled ? $"[{_prefix}:{_timeout}] {message}" : string.Empty;
}
