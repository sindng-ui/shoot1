using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace TestRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var name = new AssemblyName(eventArgs.Name).Name;
                if (name == "nunit.framework")
                {
                    return Assembly.LoadFrom(@"Library\PackageCache\com.unity.ext.nunit@d8c07649098d\net40\unity-custom\nunit.framework.dll");
                }
                if (name == "HappyShoot.Domain")
                {
                    return Assembly.LoadFrom(@"Temp\bin\Debug\HappyShoot.Domain.dll");
                }
                if (name == "netstandard")
                {
                    string netstandardPath = @"C:\Program Files\Unity\Hub\Editor\6000.3.22f1-x86_64\Editor\Data\NetStandard\compat\2.1.0\shims\netstandard\netstandard.dll";
                    if (File.Exists(netstandardPath)) return Assembly.LoadFrom(netstandardPath);
                    string netstandardRef = @"C:\Program Files\Unity\Hub\Editor\6000.3.22f1-x86_64\Editor\Data\NetStandard\ref\2.1.0\netstandard.dll";
                    if (File.Exists(netstandardRef)) return Assembly.LoadFrom(netstandardRef);
                }
                if (name.StartsWith("UnityEngine") || name.StartsWith("System."))
                {
                    string p1 = Path.Combine(@"C:\Program Files\Unity\Hub\Editor\6000.3.22f1-x86_64\Editor\Data\Managed\UnityEngine", name + ".dll");
                    if (File.Exists(p1)) return Assembly.LoadFrom(p1);
                    string p2 = Path.Combine(@"C:\Program Files\Unity\Hub\Editor\6000.3.22f1-x86_64\Editor\Data\Managed", name + ".dll");
                    if (File.Exists(p2)) return Assembly.LoadFrom(p2);
                    string p3 = Path.Combine(@"C:\Program Files\Unity\Hub\Editor\6000.3.22f1-x86_64\Editor\Data\NetStandard\compat\2.1.0\shims\netstandard", name + ".dll");
                    if (File.Exists(p3)) return Assembly.LoadFrom(p3);
                }
                return null;
            };

            return RunTests();
        }

        static int RunTests()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=================================================");
            sb.AppendLine("  HappyShoot Domain Unit Test Suite Runner");
            sb.AppendLine("  Critical Strike System & Combat Sandbox Tuning");
            sb.AppendLine("=================================================");
            sb.AppendLine();

            string testDllPath = @"Temp\bin\Debug\HappyShoot.Domain.Tests.dll";
            if (!File.Exists(testDllPath))
            {
                Console.WriteLine("Cannot find test DLL: " + testDllPath);
                return 1;
            }

            var asm = Assembly.LoadFrom(testDllPath);
            int total = 0;
            int passed = 0;
            int failed = 0;

            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            foreach (var type in types)
            {
                if (type == null) continue;
                if (type.GetCustomAttribute<TestFixtureAttribute>() == null && !type.Name.EndsWith("Tests"))
                    continue;

                sb.AppendLine(string.Format("[Fixture] {0}", type.FullName));

                var setupMethod = type.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                var tearDownMethod = type.GetMethod("TearDown", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    if (m.GetCustomAttribute<TestAttribute>() == null)
                        continue;

                    total++;
                    object instance = null;
                    try
                    {
                        instance = Activator.CreateInstance(type);
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        sb.AppendLine(string.Format("   [FAIL] {0} (Instantiation error: {1})", m.Name, ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        continue;
                    }

                    try
                    {
                        if (setupMethod != null) setupMethod.Invoke(instance, null);
                        m.Invoke(instance, null);
                        if (tearDownMethod != null) tearDownMethod.Invoke(instance, null);
                        passed++;
                        sb.AppendLine(string.Format("   [PASS] {0}", m.Name));
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        var inner = ex.InnerException != null ? ex.InnerException : ex;
                        sb.AppendLine(string.Format("   [FAIL] {0} ({1})", m.Name, inner.Message));
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("=================================================");
            sb.AppendLine(string.Format(" Total Tests: {0} | Passed: {1} | Failed: {2}", total, passed, failed));
            sb.AppendLine("=================================================");

            string output = sb.ToString();
            Console.WriteLine(output);
            File.WriteAllText(@"docs\TEST_RESULTS_CRIT.txt", output);

            return failed == 0 ? 0 : 1;
        }
    }
}
