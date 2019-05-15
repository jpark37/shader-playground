using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShaderPlayground.Core
{
    public sealed class ShaderCompilerParameter
    {
        public string Name { get; }
        public string DisplayName { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ShaderCompilerParameterType ParameterType { get; }

        public string[] Options { get; }
        public string DefaultValue { get; }

        public string Description { get; }

        public ParameterFilter Filter { get; }

        internal ShaderCompilerParameter(
            string name, 
            string displayName, 
            ShaderCompilerParameterType parameterType, 
            string[] options = null, 
            string defaultValue = null, 
            string description = null,
            ParameterFilter filter = null)
        {
            Name = name;
            DisplayName = displayName;
            ParameterType = parameterType;
            Options = options ?? Array.Empty<string>();
            DefaultValue = defaultValue;
            Description = description;
            Filter = filter;
        }

        public ShaderCompilerParameter WithFilter(string name, string value)
        {
            return WithFilter(new ParameterFilter(name, new[] { value }));
        }

        public ShaderCompilerParameter WithFilter(string name, string[] values)
        {
            return WithFilter(new ParameterFilter(name, values));
        }

        public ShaderCompilerParameter WithFilter(ParameterFilter filter)
        {
            return new ShaderCompilerParameter(
                Name,
                DisplayName,
                ParameterType,
                Options,
                DefaultValue,
                Description,
                filter);
        }
    }

    public sealed class ParameterFilter
    {
        public string Name { get; }
        public string[] Values { get; }

        public ParameterFilter(string name, string[] values)
        {
            Name = name;
            Values = values;
        }

        public ParameterFilter(string name, string value)
            : this(name, new[] {  value })
        {
            
        }
    }
}
