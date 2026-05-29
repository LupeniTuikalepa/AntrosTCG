using System;
using Unity.Properties;

namespace ATCG
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MultiColumnPropertyAttribute : CreatePropertyAttribute
    {
        public readonly string columnName;

        public MultiColumnPropertyAttribute() : this(string.Empty)
        {

        }

        public MultiColumnPropertyAttribute(string columnName)
        {
            this.columnName = columnName;
        }
    }
}