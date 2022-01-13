namespace HyperSerialize
{
    internal interface ISnippets
    {
        string PropertyTemplateSerialize { get; }
        string PropertyTemplateDeserialize { get; }
        string PropertyTemplateDeserializeLocal { get; }
        string PropertyTemplateSerializeNullable { get; }
        string PropertyTemplateDeserializeNullable { get; }
        string PropertyTemplateSerializeVarLenStr { get; }
        string PropertyTemplateDeserializeVarLenStr { get; }
        string StringLength { get; }
        string StringLengthSpan { get; }
        string ClassTemplate { get; }
    }
}