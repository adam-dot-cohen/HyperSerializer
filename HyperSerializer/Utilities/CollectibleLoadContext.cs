using System.Reflection;
using System.Runtime.Loader;

namespace HyperSerializer.Utilities;

internal class CollectibleLoadContext : AssemblyLoadContext
{
    public CollectibleLoadContext() : base(false)
    {

    }
    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}