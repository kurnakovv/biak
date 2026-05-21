// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.IntegrationTests;

/// <summary>
/// This class contains intentional errors to test PVS-Studio functionality
/// </summary>
public class PvsStudioTestCode
{
    // V3001: Variable is compared with itself
    public bool CheckSelfComparison(int value)
    {
        if (value == value)
        {
            return true;
        }
        return false;
    }

    // V3003: Using identical conditions in if-else
    public string CheckDuplicateConditions(int x)
    {
        if (x > 10)
        {
            return "greater";
        }
        else if (x > 10) // Duplicate condition
        {
            return "also greater";
        }
        return "other";
    }

    // V3021: Identical actions in different branches
    public int CheckIdenticalBranches(bool condition)
    {
        if (condition)
        {
            return 42;
        }
        else
        {
            return 42; // Same constant in both branches
        }
    }

    // V3022: Potential division by zero
    public double CheckDivisionByZero(double numerator)
    {
        double denominator = 0;
        return numerator / denominator;
    }

    // V3063: Condition is always true
    public void CheckAlwaysTrueCondition()
    {
        int x = 5;
        if (x == 5)
        {
            // This condition is always true
            Console.WriteLine("Always true");
        }
    }

    // V3004: Suspicious sequence of operations
    public void CheckSuspiciousSequence(int x)
    {
        int y = x;
        y = x; // Repeated assignment of the same value
    }

    // V3008: Variable is assigned to itself
    public void CheckSelfAssignment(int value)
    {
        value = value;
    }

    // V3095: Object is created but not used
    public void CheckUnusedObject()
    {
        new List<int>();
    }

    // V3010: Method call from constructor
    public class BaseClass
    {
        public virtual void VirtualMethod() { }
    }

    public class DerivedClass : BaseClass
    {
        private int _value;

        public DerivedClass()
        {
            VirtualMethod(); // Dangerous call of virtual method from constructor
        }

        public override void VirtualMethod()
        {
            _value = 42;
        }
    }

    // V3032: Identical expressions in logical operator
    public bool CheckIdenticalExpressions(int a, int b)
    {
        return a > 10 || a > 10; // Duplicate condition
    }

    // V3042: Possible copy-paste error
    public void CheckCopyPasteError(int x, int y)
    {
        if (x < 0)
        {
            x = 0;
        }
        if (x < 0) // Probably should be y < 0
        {
            x = 0; // Probably should be y = 0
        }
    }

    // V3080: Possible loss of precision during implicit conversion
    public void CheckPossibleLossOfPrecision()
    {
        long longValue = 1234567890123456789L;
        int intValue = (int)longValue; // Loss of precision during conversion
    }
}
