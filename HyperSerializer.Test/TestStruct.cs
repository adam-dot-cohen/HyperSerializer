using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hyper.Test;

public struct TestStruct
{
    public int T1;
    public int T2;
}
public class TestObjectWithTestStruct
{
    public virtual int A { get; set; }
    public virtual long B { get; set; }
    public virtual DateTime C { get; set; }
    public virtual uint D { get; set; }
    public virtual decimal E { get; set; }
    public virtual TimeSpan F { get; set; }
    public virtual Guid G { get; set; }
    public virtual TestEnum H { get; set; }
    public virtual TestStruct Ts { get; set; }
    public virtual string? I { get; set; }
    public virtual int? An { get; set; }
    public virtual long? Bn { get; set; }
    public virtual DateTime Cn { get; set; }
    public virtual uint? Dn { get; set; }
    public virtual decimal? En { get; set; }
    public virtual TimeSpan? Fn { get; set; }
    public virtual Guid? Gn { get; set; }
    public virtual TestEnum? Hn { get; set; }
    public virtual string? In { get; set; }
    public virtual TestStruct? Ts_Nullable { get; set; }

}
public class TestObjectWithTestStructAndarray
{
    public virtual int A { get; set; }
    public virtual long B { get; set; }
    public virtual DateTime C { get; set; }
    public virtual uint D { get; set; }
    public virtual decimal E { get; set; }
    public virtual TimeSpan F { get; set; }
    public virtual Guid G { get; set; }
    public virtual TestEnum H { get; set; }
    public virtual TestStruct Ts { get; set; }
    public virtual string? I { get; set; }
    public virtual int? An { get; set; }
    public virtual long? Bn { get; set; }
    public virtual DateTime Cn { get; set; }
    public virtual uint? Dn { get; set; }
    public virtual decimal? En { get; set; }
    public virtual TimeSpan? Fn { get; set; }
    public virtual Guid? Gn { get; set; }
    public virtual TestEnum? Hn { get; set; }
    public virtual string? In { get; set; }
    public virtual TestStruct? Ts_Nullable { get; set; }
    public virtual int[] ArrayTest { get; set; } = null!;
    public virtual int[] ArrayTestNull { get; set; }
    public virtual List<int> ListTest { get; set; }
    public virtual List<int> ListTestNull { get; set; }

}