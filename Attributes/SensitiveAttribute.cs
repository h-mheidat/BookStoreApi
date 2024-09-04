namespace BookStoreApi.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class SensitiveAttribute : Attribute, ISensitiveAttribute{}

