namespace BookStoreApi.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SensitiveAttribute : Attribute, ISensitiveAttribute
{
    
}
