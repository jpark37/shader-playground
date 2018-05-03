using System;
using System.Collections.Generic;
using OnlineShaderCompiler.Framework.Languages;

namespace OnlineShaderCompiler.Framework
{
    public interface IShaderLanguage
    {
        string Name { get; }
        string DefaultCode { get; }
        ShaderProcessorParameter[] LanguageParameters { get; }
        IShaderProcessor[] Processors { get; }
    }

    public static class ShaderLanguages
    {
        public static readonly IShaderLanguage[] All =
        {
            new HlslLanguage()
        };
    }

    public interface IShaderProcessor
    {
        string Name { get; }

        ShaderProcessorParameter[] Parameters { get; }

        ShaderProcessorResult Process(string code, Dictionary<string, string> arguments);
    }

    public sealed class ShaderProcessorParameter
    {
        public string Name { get; }
        public string DisplayName { get; }
        public ShaderProcessorParameterType ParameterType { get; }
        public string[] Options { get; }
        public string DefaultValue { get; }

        internal ShaderProcessorParameter(string name, string displayName, ShaderProcessorParameterType parameterType, string[] options = null, string defaultValue = null)
        {
            Name = name;
            DisplayName = displayName;
            ParameterType = parameterType;
            Options = options ?? Array.Empty<string>();
            DefaultValue = defaultValue;
        }
    }

    public enum ShaderProcessorParameterType
    {
        TextBox,
        ComboBox,
        CheckBox
    }

    public sealed class ShaderProcessorResult
    {
        public ShaderProcessorOutput[] Outputs { get; }

        internal ShaderProcessorResult(params ShaderProcessorOutput[] outputs)
        {
            Outputs = outputs;
        }
    }

    public sealed class ShaderProcessorOutput
    {
        public string DisplayName { get; }
        public string Language { get; }
        public string Value { get; }

        internal ShaderProcessorOutput(string displayName, string language, string value)
        {
            DisplayName = displayName;
            Language = language;
            Value = value;
        }
    }
}
