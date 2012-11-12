using System;

namespace MemoryLeak.Utility
{
    public class Property
    {
        public static readonly Property Empty = new Property(null);

        public int Int { get; private set; }
        public float Float { get; private set; }
        public bool Boolean { get; private set; }
        public string String { get; private set; }

        public Property(object value)
        {
            Int = Convert.ToInt32(value);
            Float = Convert.ToSingle(value);
            Boolean = Convert.ToBoolean(value);
            String = Convert.ToString(value);
        }
    }
}
