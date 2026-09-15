namespace wrench.auto.repair.core.Security
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SensitiveDataAttribute(SensitiveDataType type) : Attribute
    {
        public SensitiveDataType Type { get; private set; } = type;
    }
}
