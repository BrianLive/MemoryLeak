using System.Collections.Generic;

namespace MemoryLeak.Utility
{
    public class Property
    {
        public class Container
        {
            private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();

            public void Add(string name, object value)
            {
                _properties.Add(name, new Property(value));
            }

            public Property Has(string name)
            {
                return _properties.ContainsKey(name) ? _properties[name] : Empty;
            }
        }

        public static readonly Property Empty = new Property(null);

        public int Int { get; private set; }
        public float Float { get; private set; }
        public bool Boolean { get; private set; }
        public string String { get; private set; }

        public Property(object value)
        {
            if (value == null) return; //Required otherwise we can't make empty properties

            int i;
            float f;
            bool b;

            int.TryParse(value.ToString(), out i);
            float.TryParse(value.ToString(), out f);
            bool.TryParse(value.ToString(), out b);
            String = value.ToString();

            Int = i;
            Float = f;
            Boolean = b;
        }
    }
}