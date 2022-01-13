using System;

namespace HyperSerialize.Test
{
    public interface ITestWithStrings
    {
        int A { get; set; }
        long B { get; set; }
        DateTime C { get; set; }
        uint D { get; set; }
        decimal E { get; set; }
        TimeSpan F { get; set; }
        Guid G { get; set; }
        TestEnum H { get; set; }
        string? I { get; set; }
        int? An { get; set; }
        long? Bn { get; set; }
        DateTime Cn { get; set; }
        uint? Dn { get; set; }
        decimal? En { get; set; }
        TimeSpan? Fn { get; set; }
        Guid? Gn { get; set; }
        TestEnum? Hn { get; set; }
        string? In { get; set; }
    }

    public class TestWithStringsV2 : ITestWithStrings
    {
        public virtual int A { get; set; }
        public virtual long B { get; set; }
        public virtual DateTime C { get; set; }
        public virtual uint D { get; set; }
        public virtual decimal E { get; set; }
        public virtual TimeSpan F { get; set; }
        public virtual Guid G { get; set; }
        public virtual TestEnum H { get; set; }
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
        public virtual int A0 { get; set; }
    }
    public class TestWithStringsV3 : ITestWithStrings
    {
        public virtual int A0 { get; set; }
        public virtual int A { get; set; }
        public virtual long B { get; set; }
        public virtual DateTime C { get; set; }
        public virtual uint D { get; set; }
        public virtual decimal E { get; set; }
        public virtual TimeSpan F { get; set; }
        public virtual Guid G { get; set; }
        public virtual TestEnum H { get; set; }
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
    }
    public class TestWithStrings : ITestWithStrings
    {
        public virtual int A { get; set; }
        public virtual long B { get; set; }
        public virtual DateTime C { get; set; }
        public virtual uint D { get; set; }
        public virtual decimal E { get; set; }
        public virtual TimeSpan F { get; set; }
        public virtual Guid G { get; set; }
        public virtual TestEnum H { get; set; }
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
    }
}

namespace HyperSerialize.Test.Incompatible
{
    public class TestWithStrings
    {
        public virtual long A { get; set; }
        public virtual int B { get; set; }
        public virtual uint C { get; set; }
        public virtual DateTime D { get; set; }
        public virtual decimal E { get; set; }
        public virtual TimeSpan F { get; set; }
        public virtual Guid G { get; set; }
        public virtual TestEnum H { get; set; }

    }
}