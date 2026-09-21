using System.Reflection;

namespace Genesis.Core.Tests.TestKit;

/// <summary>
/// Reflection-based test discovery and execution: finds every public class
/// with a public parameterless constructor that has [Fact]-tagged public
/// instance methods, runs each in a fresh instance of its class (matching
/// xUnit's per-test-method instantiation semantics), and reports a
/// pass/fail summary. Returns a process exit code (0 = all green) so this
/// stands in for `dotnet test` in this sandbox -- see the NuGet-blocked note
/// in Genesis.Core.Tests.csproj.
/// </summary>
public static class TestRunner
{
    public static int RunAssembly(Assembly assembly)
    {
        var testClasses = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
            .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => m.GetCustomAttribute<FactAttribute>() != null))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        int passed = 0, failed = 0;
        var failures = new List<(string Test, string Message)>();

        foreach (var testClass in testClasses)
        {
            var methods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .ToList();

            foreach (var method in methods)
            {
                string name = $"{testClass.Name}.{method.Name}";
                try
                {
                    object instance = Activator.CreateInstance(testClass)!;
                    var result = method.Invoke(instance, null);
                    if (result is Task task) task.GetAwaiter().GetResult();
                    Console.WriteLine($"  PASS  {name}");
                    passed++;
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    failed++;
                    string msg = tie.InnerException.Message;
                    Console.WriteLine($"  FAIL  {name}");
                    Console.WriteLine($"        {tie.InnerException.GetType().Name}: {msg}");
                    failures.Add((name, msg));
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"  FAIL  {name}");
                    Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
                    failures.Add((name, ex.Message));
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Total: {passed + failed}, Passed: {passed}, Failed: {failed}");
        if (failed > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failures:");
            foreach (var (test, msg) in failures) Console.WriteLine($"  - {test}: {msg}");
        }

        return failed == 0 ? 0 : 1;
    }
}
