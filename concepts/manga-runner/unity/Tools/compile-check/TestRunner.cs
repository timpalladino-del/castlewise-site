using System;
using System.Reflection;
class TestRunner
{
    static int Main(string[] args)
    {
        int pass = 0, fail = 0;
        var asm = Assembly.LoadFrom(args[0]);
        foreach (var t in asm.GetTypes())
            foreach (var m in t.GetMethods())
            {
                if (m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Length == 0) continue;
                try { m.Invoke(Activator.CreateInstance(t), null); pass++; Console.WriteLine("ok   " + t.Name + "." + m.Name); }
                catch (TargetInvocationException e) { fail++; Console.WriteLine("FAIL " + t.Name + "." + m.Name + ": " + e.InnerException.Message); }
            }
        Console.WriteLine(pass + " passed, " + fail + " failed");
        return fail == 0 ? 0 : 1;
    }
}
