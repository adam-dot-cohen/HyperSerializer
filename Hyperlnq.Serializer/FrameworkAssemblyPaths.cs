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
        public static string clrcompression => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "clrcompression.dll");
        public static string clretwrc => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "clretwrc.dll");
        public static string clrjit => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "clrjit.dll");
        public static string coreclr => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "coreclr.dll");
        public static string coreclr_delegates => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "coreclr_delegates.h");
        public static string createdump => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "createdump.exe");
        public static string dbgshim => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "dbgshim.dll");
        public static string hostfxr => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "hostfxr.h");
        public static string hostpolicy => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "hostpolicy.dll");
        public static string Microsoft_CSharp => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.CSharp.dll");
        public static string Microsoft_DiaSymReader_Native_amd64 => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.DiaSymReader.Native.amd64.dll");
        public static string Microsoft_NETCore_App_deps => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.NETCore.App.deps.json");
        public static string Microsoft_VisualBasic_Core => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.VisualBasic.Core.dll");
        public static string Microsoft_VisualBasic => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.VisualBasic.dll");
        public static string Microsoft_Win32_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.Win32.Primitives.dll");
        public static string Microsoft_Win32_Registry => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "Microsoft.Win32.Registry.dll");
        public static string mscordaccore => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "mscordaccore.dll");
        public static string mscordaccore_amd64_amd64_5_0_821_31504 => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "mscordaccore_amd64_amd64_5.0.821.31504.dll");
        public static string mscordbi => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "mscordbi.dll");
        public static string mscorlib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "mscorlib.dll");
        public static string mscorrc => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "mscorrc.dll");
        public static string netstandard => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "netstandard.dll");
        public static string System_AppContext => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.AppContext.dll");
        public static string System_Buffers => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Buffers.dll");
        public static string System_Collections_Concurrent => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.Concurrent.dll");
        public static string System_Collections => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.dll");
        public static string System_Collections_Immutable => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.Immutable.dll");
        public static string System_Collections_NonGeneric => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.NonGeneric.dll");
        public static string System_Collections_Specialized => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.Specialized.dll");
        public static string System_ComponentModel_Annotations => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.Annotations.dll");
        public static string System_ComponentModel_DataAnnotations => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.DataAnnotations.dll");
        public static string System_ComponentModel => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.dll");
        public static string System_ComponentModel_EventBasedAsync => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.EventBasedAsync.dll");
        public static string System_ComponentModel_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.Primitives.dll");
        public static string System_ComponentModel_TypeConverter => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ComponentModel.TypeConverter.dll");
        public static string System_Configuration => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Configuration.dll");
        public static string System_Console => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Console.dll");
        public static string System_Core => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Core.dll");
        public static string System_Data_Common => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Data.Common.dll");
        public static string System_Data_DataSetExtensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Data.DataSetExtensions.dll");
        public static string System_Data => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Data.dll");
        public static string System_Diagnostics_Contracts => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.Contracts.dll");
        public static string System_Diagnostics_Debug => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.Debug.dll");
        public static string System_Diagnostics_DiagnosticSource => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.DiagnosticSource.dll");
        public static string System_Diagnostics_FileVersionInfo => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.FileVersionInfo.dll");
        public static string System_Diagnostics_Process => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.Process.dll");
        public static string System_Diagnostics_StackTrace => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.StackTrace.dll");
        public static string System_Diagnostics_TextWriterTraceListener => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.TextWriterTraceListener.dll");
        public static string System_Diagnostics_Tools => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.Tools.dll");
        public static string System_Diagnostics_TraceSource => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.TraceSource.dll");
        public static string System_Diagnostics_Tracing => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Diagnostics.Tracing.dll");
        public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
        public static string System_Drawing => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Drawing.dll");
        public static string System_Drawing_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Drawing.Primitives.dll");
        public static string System_Dynamic_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Dynamic.Runtime.dll");
        public static string System_Formats_Asn1 => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Formats.Asn1.dll");
        public static string System_Globalization_Calendars => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Globalization.Calendars.dll");
        public static string System_Globalization => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Globalization.dll");
        public static string System_Globalization_Extensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Globalization.Extensions.dll");
        public static string System_IO_Compression_Brotli => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Compression.Brotli.dll");
        public static string System_IO_Compression => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Compression.dll");
        public static string System_IO_Compression_FileSystem => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Compression.FileSystem.dll");
        public static string System_IO_Compression_ZipFile => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Compression.ZipFile.dll");
        public static string System_IO => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.dll");
        public static string System_IO_FileSystem_AccessControl => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.FileSystem.AccessControl.dll");
        public static string System_IO_FileSystem => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.FileSystem.dll");
        public static string System_IO_FileSystem_DriveInfo => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.FileSystem.DriveInfo.dll");
        public static string System_IO_FileSystem_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.FileSystem.Primitives.dll");
        public static string System_IO_FileSystem_Watcher => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.FileSystem.Watcher.dll");
        public static string System_IO_IsolatedStorage => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.IsolatedStorage.dll");
        public static string System_IO_MemoryMappedFiles => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.MemoryMappedFiles.dll");
        public static string System_IO_Pipes_AccessControl => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Pipes.AccessControl.dll");
        public static string System_IO_Pipes => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.Pipes.dll");
        public static string System_IO_UnmanagedMemoryStream => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.IO.UnmanagedMemoryStream.dll");
        public static string System_Linq => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Linq.dll");
        public static string System_Linq_Expressions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Linq.Expressions.dll");
        public static string System_Linq_Parallel => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Linq.Parallel.dll");
        public static string System_Linq_Queryable => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Linq.Queryable.dll");
        public static string System_Memory => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Memory.dll");
        public static string System_Net => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.dll");
        public static string System_Net_Http => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Http.dll");
        public static string System_Net_Http_Json => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Http.Json.dll");
        public static string System_Net_HttpListener => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.HttpListener.dll");
        public static string System_Net_Mail => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Mail.dll");
        public static string System_Net_NameResolution => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.NameResolution.dll");
        public static string System_Net_NetworkInformation => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.NetworkInformation.dll");
        public static string System_Net_Ping => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Ping.dll");
        public static string System_Net_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Primitives.dll");
        public static string System_Net_Requests => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Requests.dll");
        public static string System_Net_Security => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Security.dll");
        public static string System_Net_ServicePoint => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.ServicePoint.dll");
        public static string System_Net_Sockets => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.Sockets.dll");
        public static string System_Net_WebClient => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.WebClient.dll");
        public static string System_Net_WebHeaderCollection => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.WebHeaderCollection.dll");
        public static string System_Net_WebProxy => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.WebProxy.dll");
        public static string System_Net_WebSockets_Client => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.WebSockets.Client.dll");
        public static string System_Net_WebSockets => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Net.WebSockets.dll");
        public static string System_Numerics => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Numerics.dll");
        public static string System_Numerics_Vectors => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Numerics.Vectors.dll");
        public static string System_ObjectModel => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ObjectModel.dll");
        public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
        public static string System_Private_DataContractSerialization => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.DataContractSerialization.dll");
        public static string System_Private_Uri => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.Uri.dll");
        public static string System_Private_Xml => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.Xml.dll");
        public static string System_Private_Xml_Linq => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.Xml.Linq.dll");
        public static string System_Reflection_DispatchProxy => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.DispatchProxy.dll");
        public static string System_Reflection => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.dll");
        public static string System_Reflection_Emit => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Emit.dll");
        public static string System_Reflection_Emit_ILGeneration => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Emit.ILGeneration.dll");
        public static string System_Reflection_Emit_Lightweight => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Emit.Lightweight.dll");
        public static string System_Reflection_Extensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Extensions.dll");
        public static string System_Reflection_Metadata => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Metadata.dll");
        public static string System_Reflection_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.Primitives.dll");
        public static string System_Reflection_TypeExtensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Reflection.TypeExtensions.dll");
        public static string System_Resources_Reader => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Resources.Reader.dll");
        public static string System_Resources_ResourceManager => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Resources.ResourceManager.dll");
        public static string System_Resources_Writer => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Resources.Writer.dll");
        public static string System_Runtime_CompilerServices_Unsafe => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.CompilerServices.Unsafe.dll");
        public static string System_Runtime_CompilerServices_VisualC => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.CompilerServices.VisualC.dll");
        public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
        public static string System_Runtime_Extensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Extensions.dll");
        public static string System_Runtime_Handles => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Handles.dll");
        public static string System_Runtime_InteropServices => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.InteropServices.dll");
        public static string System_Runtime_InteropServices_RuntimeInformation => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.InteropServices.RuntimeInformation.dll");
        public static string System_Runtime_Intrinsics => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Intrinsics.dll");
        public static string System_Runtime_Loader => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Loader.dll");
        public static string System_Runtime_Numerics => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Numerics.dll");
        public static string System_Runtime_Serialization => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Serialization.dll");
        public static string System_Runtime_Serialization_Formatters => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Serialization.Formatters.dll");
        public static string System_Runtime_Serialization_Json => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Serialization.Json.dll");
        public static string System_Runtime_Serialization_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Serialization.Primitives.dll");
        public static string System_Runtime_Serialization_Xml => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.Serialization.Xml.dll");
        public static string System_Security_AccessControl => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.AccessControl.dll");
        public static string System_Security_Claims => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Claims.dll");
        public static string System_Security_Cryptography_Algorithms => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.Algorithms.dll");
        public static string System_Security_Cryptography_Cng => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.Cng.dll");
        public static string System_Security_Cryptography_Csp => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.Csp.dll");
        public static string System_Security_Cryptography_Encoding => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.Encoding.dll");
        public static string System_Security_Cryptography_OpenSsl => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.OpenSsl.dll");
        public static string System_Security_Cryptography_Primitives => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.Primitives.dll");
        public static string System_Security_Cryptography_X509Certificates => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Cryptography.X509Certificates.dll");
        public static string System_Security => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.dll");
        public static string System_Security_Principal => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Principal.dll");
        public static string System_Security_Principal_Windows => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.Principal.Windows.dll");
        public static string System_Security_SecureString => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Security.SecureString.dll");
        public static string System_ServiceModel_Web => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ServiceModel.Web.dll");
        public static string System_ServiceProcess => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ServiceProcess.dll");
        public static string System_Text_Encoding_CodePages => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.Encoding.CodePages.dll");
        public static string System_Text_Encoding => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.Encoding.dll");
        public static string System_Text_Encoding_Extensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.Encoding.Extensions.dll");
        public static string System_Text_Encodings_Web => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.Encodings.Web.dll");
        public static string System_Text_Json => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.Json.dll");
        public static string System_Text_RegularExpressions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Text.RegularExpressions.dll");
        public static string System_Threading_Channels => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Channels.dll");
        public static string System_Threading => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.dll");
        public static string System_Threading_Overlapped => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Overlapped.dll");
        public static string System_Threading_Tasks_Dataflow => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Tasks.Dataflow.dll");
        public static string System_Threading_Tasks => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Tasks.dll");
        public static string System_Threading_Tasks_Extensions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Tasks.Extensions.dll");
        public static string System_Threading_Tasks_Parallel => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Tasks.Parallel.dll");
        public static string System_Threading_Thread => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Thread.dll");
        public static string System_Threading_ThreadPool => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.ThreadPool.dll");
        public static string System_Threading_Timer => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Threading.Timer.dll");
        public static string System_Transactions => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Transactions.dll");
        public static string System_Transactions_Local => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Transactions.Local.dll");
        public static string System_ValueTuple => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.ValueTuple.dll");
        public static string System_Web => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Web.dll");
        public static string System_Web_HttpUtility => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Web.HttpUtility.dll");
        public static string System_Windows => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Windows.dll");
        public static string System_Xml => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.dll");
        public static string System_Xml_Linq => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.Linq.dll");
        public static string System_Xml_ReaderWriter => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.ReaderWriter.dll");
        public static string System_Xml_Serialization => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.Serialization.dll");
        public static string System_Xml_XDocument => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.XDocument.dll");
        public static string System_Xml_XmlDocument => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.XmlDocument.dll");
        public static string System_Xml_XmlSerializer => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.XmlSerializer.dll");
        public static string System_Xml_XPath => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.XPath.dll");
        public static string System_Xml_XPath_XDocument => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Xml.XPath.XDocument.dll");
        public static string ucrtbase => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "ucrtbase.dll");
        public static string WindowsBase => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "WindowsBase.dll");
    }
}
