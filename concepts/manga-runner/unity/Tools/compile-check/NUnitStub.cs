// Just enough of NUnit for the Ink Run logic tests to compile and run outside Unity.
using System;
namespace NUnit.Framework
{
    public class TestAttribute : Attribute { }
    public class TestFixtureAttribute : Attribute { }
    public class AssertionException : Exception { public AssertionException(string m) : base(m) { } }
    public static class Assert
    {
        static void Fail(string m) { throw new AssertionException(m); }
        public static void IsTrue(bool c) { if (!c) Fail("expected true"); }
        public static void IsFalse(bool c) { if (c) Fail("expected false"); }
        public static void AreEqual(int a, int b) { if (a != b) Fail($"expected {a} but was {b}"); }
        public static void AreEqual(float a, float b, float tol) { if (Math.Abs(a - b) > tol) Fail($"expected {a} but was {b}"); }
        public static void AreNotEqual(object a, object b) { if (Equals(a, b)) Fail($"expected not {a}"); }
        public static void Greater(float a, float b) { if (!(a > b)) Fail($"expected {a} > {b}"); }
        public static void Greater(int a, int b) { if (!(a > b)) Fail($"expected {a} > {b}"); }
        public static void GreaterOrEqual(float a, float b) { if (!(a >= b)) Fail($"expected {a} >= {b}"); }
        public static void Less(float a, float b) { if (!(a < b)) Fail($"expected {a} < {b}"); }
    }
}
