using MemoryPack;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Benchmarks.Experiments;

[MessagePackObject(), ProtoContract()]
[MemoryPackable]
public partial class TestWithStrings
{
    [Key(1), ProtoMember(1)]
    public virtual int A { get; set; }
    [Key(2), ProtoMember(2)]
    public virtual long B { get; set; }
    [Key(3), ProtoMember(3)]
    public virtual DateTime C { get; set; }
    [Key(4), ProtoMember(4)]
    public virtual uint D { get; set; }
    [Key(5), ProtoMember(5)]
    public virtual decimal E { get; set; }
    [Key(6), ProtoMember(6)]
    public virtual TimeSpan F { get; set; }
    [Key(7), ProtoMember(7)]
    public virtual Guid G { get; set; }
    [Key(8), ProtoMember(8)]
    public virtual TestEnum H { get; set; }
    [Key(9), ProtoMember(9)]
    public virtual string I { get; set; }
    [Key(10), ProtoMember(11)]
    public virtual int? An { get; set; }
    [Key(12), ProtoMember(12)]
    public virtual long? Bn { get; set; }
    [Key(13), ProtoMember(13)]
    public virtual DateTime Cn { get; set; }
    [Key(14), ProtoMember(14)]
    public virtual uint? Dn { get; set; }
    [Key(15), ProtoMember(15)]
    public virtual decimal? En { get; set; }
    [Key(16), ProtoMember(16)]
    public virtual TimeSpan? Fn { get; set; }
    [Key(17), ProtoMember(17)]
    public virtual Guid? Gn { get; set; }
    [Key(18), ProtoMember(18)]
    public virtual TestEnum? Hn { get; set; }
    [Key(19), ProtoMember(19)]
    public virtual string In { get; set; }

}

[MessagePackObject(), ProtoContract()]
[MemoryPackable]
public partial class TestObjectWithStringsAndArray
{
    [Key(1), ProtoMember(1)]
    public virtual int A { get; set; }
    [Key(2), ProtoMember(2)]
    public virtual long B { get; set; }
    [Key(3), ProtoMember(3)]
    public virtual DateTime C { get; set; }
    [Key(4), ProtoMember(4)]
    public virtual uint D { get; set; }
    [Key(5), ProtoMember(5)]
    public virtual decimal E { get; set; }
    [Key(6), ProtoMember(6)]
    public virtual TimeSpan F { get; set; }
    [Key(7), ProtoMember(7)]
    public virtual Guid G { get; set; }
    [Key(8), ProtoMember(8)]
    public virtual TestEnum H { get; set; }
    [Key(9), ProtoMember(9)]
    public virtual string I { get; set; }
    [Key(10), ProtoMember(11)]
    public virtual int? An { get; set; }
    [Key(12), ProtoMember(12)]
    public virtual long? Bn { get; set; }
    [Key(13), ProtoMember(13)]
    public virtual DateTime Cn { get; set; }
    [Key(14), ProtoMember(14)]
    public virtual uint? Dn { get; set; }
    [Key(15), ProtoMember(15)]
    public virtual decimal? En { get; set; }
    [Key(16), ProtoMember(16)]
    public virtual TimeSpan? Fn { get; set; }
    [Key(17), ProtoMember(17)]
    public virtual Guid? Gn { get; set; }
    [Key(18), ProtoMember(18)]
    public virtual TestEnum? Hn { get; set; }
    [Key(19), ProtoMember(19)]
    public virtual string In { get; set; }
    [Key(20), ProtoMember(20)]
    public virtual int[] ArrayTest { get; set; }
    [Key(21), ProtoMember(21)]
    public virtual int[] ArrayTestNull { get; set; }
    [Key(22), ProtoMember(22)]
    public virtual List<int> ListTest { get; set; }
    [Key(23), ProtoMember(23)]
    public virtual List<int> ListTestNull { get; set; }

}

public enum TestEnum
{
    one, two, three
}
[MessagePackObject(), ProtoContract()]
[Serializable]
[MemoryPackable]
public partial class Test
{
    [Key(1), ProtoMember(1)]
    public virtual Guid? Gn { get; set; }
    [Key(2), ProtoMember(2)]
    public virtual int A { get; set; }
    [Key(3), ProtoMember(3)]
    public virtual long B { get; set; }
    [Key(4), ProtoMember(4)]
    public virtual DateTime C { get; set; }
    [Key(5), ProtoMember(5)]
    public virtual uint D { get; set; }
    [Key(6), ProtoMember(6)]
    public virtual decimal E { get; set; }
    [Key(7), ProtoMember(7)]
    public virtual TimeSpan F { get; set; }
    [Key(8), ProtoMember(8)]
    public virtual Guid G { get; set; }
    [Key(9), ProtoMember(9)]
    public virtual TestEnum H { get; set; }
}
[MessagePackObject(), ProtoContract()]
[MemoryPackable]
public partial class SymbolTick
{
    public DateTime Timestamp { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public int SymbolId { get; set; }
    [MemoryPackConstructor]
    public SymbolTick()
    {
    }

    public SymbolTick(DateTime timestamp, double bid, double ask, int symbolId)
    {
        this.Timestamp = timestamp;
        this.Bid = bid;
        this.Ask = ask;
        this.SymbolId = symbolId;
    }
}