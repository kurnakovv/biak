// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biak.ConsoleApp.IntegrationTests.Templates.SimpleProjectWithWarnings.MySimpleProjectTemplate;

internal class ProgramCS0612
{
    [Obsolete]
    static void OldMethod()
    {
    }

    static void Main()
    {
        OldMethod();
    }
}
