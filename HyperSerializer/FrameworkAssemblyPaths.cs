using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace HyperSerializer
{
    internal static class MyExtensions
    {
        public static async void Compile()
        {
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var workspace = new AdhocWorkspace(host);
            var scriptCode = GenCode();

            var compilationOptions = new CSharpCompilationOptions(
               OutputKind.DynamicallyLinkedLibrary,
               usings: new[] { "System" });

            var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
               .WithMetadataReferences(new[]
               {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
               })
               .WithCompilationOptions(compilationOptions);

            var scriptProject = workspace.AddProject(scriptProjectInfo);
            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(scriptProject.Id), "Script",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
            var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

            // cursor position is at the end
            var position = scriptCode.Length - 1;

            var completionService = CompletionService.GetService(scriptDocument);
            var results = await completionService.GetCompletionsAsync(scriptDocument, position);
        }

        public static string GenCode()
        {
            return @"
	            public class FrameworkAssemblyPaths
	            {
		            " + string.Join("\n\t\t", GetPaths().Select(r => r.Item2)) + @"
	            }
	            ";

        }
        public static string Join()
        {
            return string.Join(",\n", GetNames().Join(GetPaths(), a => a.Trim(), b => b.Item1.Replace(".", "_"), (a, b) => "{FrameworkAssemblyPaths." + a + "," + b.Item2 + "}"));
        }
        public static IEnumerable<string> GetNames()
        {
            foreach (var p in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location)))
                .Where(r => !Path.GetFileName(r).StartsWith(".") && !Path.GetFileName(r).StartsWith("api")).ToList())
                yield return Path.GetFileNameWithoutExtension(p).Replace(".", "_");
        }

        public static IEnumerable<(string, string)> GetPaths()
        {
            foreach (var p in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location)))
                .Where(r => !Path.GetFileName(r).StartsWith(".") && !Path.GetFileName(r).StartsWith("api")).ToList())
                yield return (Path.GetFileNameWithoutExtension(p), "public static string " + Path.GetFileNameWithoutExtension(p).Replace(".", "_") + " => Path.Combine(Path.GetDirectoryName(typeof(System.Object).Assembly.Location), \"" + Path.GetFileName(p) + "\");");
        }
    }
    public class FrameworkAssemblyPaths
    {
        public static string System_Console => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Console.dll");
        public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
        public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
        public static string System_Runtime_CompilerServices_Unsafe => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.CompilerServices.Unsafe.dll");
        public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
    }
}
