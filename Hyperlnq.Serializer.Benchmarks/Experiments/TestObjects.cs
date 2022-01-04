using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace HyperSerializer.Benchmarks.Experiments
{
    [MessagePackObject(), ProtoContract(), ZeroFormattable()]
    public class TestWithStrings
    {
        [Key(1), ProtoMember(1), Index(2)]
        public virtual int A { get; set; }
        [Key(2), ProtoMember(2), Index(2)]
        public virtual long B { get; set; }
        [Key(3), ProtoMember(3), Index(3)]
        public virtual DateTime C { get; set; }
        [Key(4), ProtoMember(4), Index(4)]
        public virtual uint D { get; set; }
        [Key(5), ProtoMember(5), Index(5)]
        public virtual decimal E { get; set; }
        [Key(6), ProtoMember(6), Index(6)]
        public virtual TimeSpan F { get; set; }
        [Key(7), ProtoMember(7), Index(7)]
        public virtual Guid G { get; set; }
        [Key(8), ProtoMember(8), Index(8)]
        public virtual TestEnum H { get; set; }
        [Key(9), ProtoMember(9), Index(9)]
        public virtual string I { get; set; }
        [Key(10), ProtoMember(11), Index(11)]
        public virtual int? An { get; set; }
        [Key(12), ProtoMember(12), Index(12)]
        public virtual long? Bn { get; set; }
        [Key(13), ProtoMember(13), Index(13)]
        public virtual DateTime Cn { get; set; }
        [Key(14), ProtoMember(14), Index(14)]
        public virtual uint? Dn { get; set; }
        [Key(15), ProtoMember(15), Index(15)]
        public virtual decimal? En { get; set; }
        [Key(16), ProtoMember(16), Index(16)]
        public virtual TimeSpan? Fn { get; set; }
        [Key(17), ProtoMember(17), Index(17)]
        public virtual Guid? Gn { get; set; }
        [Key(18), ProtoMember(18), Index(18)]
        public virtual TestEnum? Hn { get; set; }
        [Key(19), ProtoMember(19), Index(19)]
        public virtual string In { get; set; }

    }

    public enum TestEnum
    {
        one, two, three
    }
    [MessagePackObject(), ProtoContract()]
    [Serializable]
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
        //[Key(10), ProtoMember(10), Index(9)]
        //public virtual string I { get; set; }
        //[Key(11), ProtoMember(11), Index(9)]
        //public virtual int? An { get; set; }
        //[Key(12), ProtoMember(12), Index(10)]
        //public virtual long? Bn { get; set; }
        //[Key(13), ProtoMember(13), Index(11)]
        //public virtual DateTime Cn { get; set; }
        //[Key(14), ProtoMember(14), Index(12)]
        //public virtual uint? Dn { get; set; }
        //[Key(15), ProtoMember(15), Index(13)]
        //public virtual decimal? En { get; set; }
        //[Key(16), ProtoMember(16), Index(14)]
        //public virtual TimeSpan? Fn { get; set; }
        //[Key(17), ProtoMember(17), Index(15)]
        //public virtual TestEnum? Hn { get; set; }
        //[Key(18), ProtoMember(18), Index(19)]
        //public virtual string In { get; set; }
    }

}
