using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Hyperlnq.Dynamic.Delegates;

/// <summary>
/// Generator class for constructing delegates of type <typeparamref name="TDelegate"/>.
/// </summary>
public class DynamicDelegateBuilder<TDelegate>
    where TDelegate : Delegate
{
    private DynamicMethod _method;
    private ILGenerator _generator;
    public DynamicDelegateBuilder(Action<ILGenerator> generator) : this()
    {
        Add_IL(generator);
    }
    public DynamicDelegateBuilder(Module m = null)
    {
        MethodInfo invoke = typeof(TDelegate).GetMethod("Invoke");
        var parameterTypes = invoke.GetParameters().Select(p => p.ParameterType).ToArray();

        _method = new DynamicMethod(
            string.Join('_', parameterTypes.Select(h => h.Name)),
            MethodAttributes.Static | MethodAttributes.Public,
            CallingConventions.Standard,
            invoke.ReturnType,
            parameterTypes, m, true);

        _generator = _method.GetILGenerator();
    }
    public DynamicDelegateBuilder<TDelegate> Add_IL(Action<ILGenerator> generator)
    {
        generator(_generator);
        return this;
    }
    public TDelegate Build()
    {
        return (TDelegate)_method.CreateDelegate(typeof(TDelegate));
    }
}