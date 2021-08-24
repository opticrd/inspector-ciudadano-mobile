using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Helpers
{
    public static class LoggerExtension
    {
        public static IDictionary<string, string> InitDictionary(params string[] properties)
        {
            var dictionary = new Dictionary<string, string>();

            for (int i = 0; i < properties.Length; i++)
            {
                dictionary.Add($"Property {i}", properties[i]);
            }

            return dictionary;
        }

        public static IDictionary<string, string> InitDictionary(string location, string method, params string[] properties)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Location", location);
            dictionary.Add("Method", method);

            for (int i = 0; i < properties.Length; i++)
            {
                dictionary.Add($"Property {i}", properties[i]);
            }

            return dictionary;
        }

        public static IDictionary<string, string> InitDictionary(string location, string method, Dictionary<string, string> properties = null)
        {
            if (properties == null) 
                properties = new Dictionary<string, string>();

            properties.Add("Location", location);
            properties.Add("Method", method);

            return properties;
        }
    }
}
