using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace TestHost
{
    class Program
    {
        static int Main(string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=================================================");
            sb.AppendLine("  HappyShoot Domain Unit Test Suite Runner (.NET 8)");
            sb.AppendLine("  Critical Strike System & Combat Sandbox Tuning");
            sb.AppendLine("=================================================");
            sb.AppendLine();

            var asm = typeof(HappyShoot.Domain.Tests.Entities.CriticalStrikeTests).Assembly;
            int total = 0;
            int passed = 0;
            int failed = 0;

            foreach (var type in asm.GetTypes())
            {
                if (type.GetCustomAttribute<TestFixtureAttribute>() == null && !type.Name.EndsWith("Tests"))
                    continue;

                sb.AppendLine($"[Fixture] {type.FullName}");

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
                        sb.AppendLine($"   [FAIL] {m.Name} (Instantiation: {ex.InnerException?.Message ?? ex.Message})");
                        continue;
                    }

                    try
                    {
                        setupMethod?.Invoke(instance, null);
                        m.Invoke(instance, null);
                        tearDownMethod?.Invoke(instance, null);
                        passed++;
                        sb.AppendLine($"   [PASS] {m.Name}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        var inner = ex.InnerException ?? ex;
                        sb.AppendLine($"   [FAIL] {m.Name} ({inner.Message})");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("=================================================");
            sb.AppendLine($" Total Tests: {total} | Passed: {passed} | Failed: {failed}");
            sb.AppendLine("=================================================");

            string output = sb.ToString();
            Console.WriteLine(output);
            File.WriteAllText(@"docs\TEST_RESULTS_CRIT.txt", output);

            return failed == 0 ? 0 : 1;
        }
    }
}
