using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HyperSerializer.Dynamic.Delegates;

/// <summary>
/// Generator class for constructing delegates of type <typeparamref name="TDelegate"/>.
/// </summary>
public class DynamicDelegateBuilder<TDelegate>
    where TDelegate : System.Delegate
{
    private DynamicMethod _method;
    private ILGenerator _generator;
    public DynamicDelegateBuilder(Action<ILGenerator> generator) : this()
    {
        this.Add_IL(generator);
    }
    public DynamicDelegateBuilder(Module m = null)
    {
        MethodInfo invoke = typeof(TDelegate).GetMethod("Invoke");
        var parameterTypes = invoke.GetParameters().Select(p => p.ParameterType).ToArray();

        this._method = new DynamicMethod(
            string.Join('_', parameterTypes.Select(h => h.Name)),
            MethodAttributes.Static | MethodAttributes.Public,
            CallingConventions.Standard,
            invoke.ReturnType,
            parameterTypes, m, true);

        this._generator = this._method.GetILGenerator();
    }
    public DynamicDelegateBuilder<TDelegate> Add_IL(Action<ILGenerator> generator)
    {
        generator(this._generator);
        return this;
    }
    public TDelegate Build()
    {
        return (TDelegate)this._method.CreateDelegate(typeof(TDelegate));
    }
}