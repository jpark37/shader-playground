using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShaderPlayground.Core.Util
{
    internal static class Validate
    {
        private static readonly Regex IdentifierRegex = new Regex("[_a-zA-Z0-9][a-zA-Z0-9]*", RegexOptions.Compiled);

        public static string Identifier(Dictionary<string, string> arguments, string name)
        {
            var value = arguments[name];
            if (!IdentifierRegex.IsMatch(value))
            {
                throw new ArgumentOutOfRangeException($"Invalid identifier for {name}: '{value}'");
            }
            return value;
        }

        public static string Option(Dictionary<string, string> arguments, string name, string[] validOptions)
        {
            var value = arguments[name];
            if (!validOptions.Contains(value))
            {
                throw new ArgumentOutOfRangeException($"Invalid option for {name}: '{value}'");
            }
            return value;
        }

        public static bool Boolean(Dictionary<string, string> arguments, string name)
        {
            var value = arguments[name];
            if (!System.Boolean.TryParse(value, out var result))
            {
                throw new ArgumentOutOfRangeException($"Invalid value for {name}: '{value}'");
            }
            return result;
        }
    }
}
