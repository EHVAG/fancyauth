using System;
using System.Diagnostics;

namespace Fancyauth
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<FancyContext, Migrations.Configuration>());

            #if !DEBUG
            System.Diagnostics.Debug.Listeners.Add(new ConsoleTraceListener(true));
            #endif

            new Fancyauth().ServerMain().Wait();
        }

        /*
        public static Assembly Compile(params string[] sources)
        {


            var assemblyFileName = "gen" + Guid.NewGuid().ToString().Replace("-", "") + ".dll";
            var compilation = CSharpCompilation.Create(assemblyFileName,
                options:     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: from source in sources
                select CSharpSyntaxTree.ParseText(source),
                references:  new[]
                {
                    MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(RuntimeBinderException).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly),
                });

            EmitResult emitResult;

            using (var ms = new MemoryStream())
            {
                emitResult = compilation.Emit(ms);

                if (emitResult.Success)
                {
                    var assembly = Assembly.Load(ms.GetBuffer());
                    return assembly;
                }
            }

            var message = string.Join("\r\n", emitResult.Diagnostics);
            throw new ApplicationException(message);
        }

        public static void Main__(string[] args)
        {
            object row = "swag";// this object contains the runtime data
            string code = @"using System;
public class CompiledExpression
{
    public static int Run(object o)
    {
        Func<int, int> asd = x => x * 2;
        Console.WriteLine(""Hello world! {0} {1} {2}"", o, asd, asd(21));
        return 1337;
    }
}
";// this is the generated C# code

            var assembly = Compile(code);
            var type = assembly.GetType("CompiledExpression");
            var method = type.GetMethod("Run");
            var result = method.Invoke(null, new object[] { row });

            Console.WriteLine("Hurray, the expression returned " + result);
        }*/
    }
}
