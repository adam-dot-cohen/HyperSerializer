using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HyperSerializer.Dynamic.Delegates;

public class DynamicDelegate<TDelegate>
    where TDelegate : System.Delegate
{
    private TDelegate _delegate;
    public DynamicDelegate(Action<ILGenerator> generator)
    {
        this._delegate = new DynamicDelegateBuilder<TDelegate>(this.GetType().Module)
            .Add_IL(generator)
            .Build();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TDelegate Invoke()
    {
        return this._delegate;
    }
}