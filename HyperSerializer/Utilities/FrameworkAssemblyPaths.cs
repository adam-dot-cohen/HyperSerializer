using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HyperSerializer.Utilities;

internal class FrameworkAssemblyPaths
{
    public static string System_Console => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Console.dll");
    public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
    public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
    public static string System_Runtime_CompilerServices_Unsafe => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.CompilerServices.Unsafe.dll");
    public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
}