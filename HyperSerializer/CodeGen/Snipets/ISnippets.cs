namespace HyperSerializer.CodeGen.Snipets
{
    internal interface ISnippets
    {
        public string PropertyTemplateSerialize { get; }
        public string PropertyTemplateDeserialize { get; }
        public string PropertyTemplateDeserializeLocal { get; }
        public string PropertyTemplateSerializeNullable { get; }
        public string PropertyTemplateDeserializeNullable { get; }
        public string PropertyTemplateSerializeVarLenStr { get; }
        public string PropertyTemplateDeserializeVarLenStr { get; }
        public string StringLength { get; }
        public string StringLengthSpan { get; }
        public string ClassTemplate { get; }
    }
}