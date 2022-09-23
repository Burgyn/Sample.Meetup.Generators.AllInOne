using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace HelloWorld
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var myClass = @"
using System;

namespace HelloWorld.Demo
{
    public class MyClass
    {
        public void HelloWorld()
        {
            Console.WriteLine(""Hello World from generator ..."");
        }
    }
}
";
            context.AddSource("MyClass", SourceText.From(myClass, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
