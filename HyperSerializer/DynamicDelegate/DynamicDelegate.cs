using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hyperlnq.Dynamic.Delegates
{
    
    public class DynamicDelegate<TDelegate>
        where TDelegate : Delegate
    {
        private TDelegate _delegate;
        public DynamicDelegate(Action<ILGenerator> generator)
        {
            _delegate = new DynamicDelegateBuilder<TDelegate>(this.GetType().Module)
                .Add_IL(generator)
                .Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDelegate Invoke()
        {
            return _delegate;
        }
    }
}
