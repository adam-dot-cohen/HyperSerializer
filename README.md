# HyperSerializer
Blazing fast binary serialization up to 16 times faster than MessagePack with roughly equivelant memory allocation.  The benchmarks below are depict 100K iterations serializing and deserializing the class included below the table.

|                    Method |         Mean |  Ratio |       Gen 0 |     Gen 1 | Allocated |
|-------------------------- |-------------:|-------:|------------:|----------:|----------:|
|           HyperSerializer |     5.461 ms |   1.00 |   1000.0000 |         - |     21 MB |
|     MessagePackSerializer |    89.076 ms |  16.36 |   1000.0000 |         - |     20 MB |
|        ProtobufSerializer |    97.729 ms |  17.97 |   3000.0000 |         - |     43 MB |
|    DataContractSerializer | 1,030.394 ms | 189.49 | 129000.0000 | 3000.0000 |  1,602 MB |
|           BinaryFormatter | 1,791.142 ms | 329.02 | 152000.0000 | 4000.0000 |  1,881 MB |

```csharp
public class Test
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
```
## Example Code
