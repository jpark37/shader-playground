using System;
using System.IO;
using System.Linq;

namespace ShaderPlayground.Core
{
    internal static class CommonParameters
    {
        private static readonly string[] ShaderStageOptions =
        {
            "vert",
            "tesc",
            "tese",
            "geom",
            "frag",
            "comp"
        };

        public static readonly ShaderCompilerParameter GlslShaderStage = new ShaderCompilerParameter(
            "ShaderStage", 
            "Shader stage", 
            ShaderCompilerParameterType.ComboBox, 
            ShaderStageOptions, 
            defaultValue: "frag");

        public static readonly ShaderCompilerParameter HlslEntryPoint = new ShaderCompilerParameter(
            "EntryPoint",
            "Entry point",
            ShaderCompilerParameterType.TextBox,
            defaultValue: "PSMain");

        public static readonly ShaderCompilerParameter SpirVEntryPoint = new ShaderCompilerParameter(
            "EntryPoint",
            "Entry point",
            ShaderCompilerParameterType.TextBox,
            defaultValue: "main",
            description: "Ignored when input language is GLSL.");

        public const string OutputLanguageParameterName = "OutputLanguage";

        public static ShaderCompilerParameter CreateOutputParameter(string[] languages)
        {
            return new ShaderCompilerParameter(
                OutputLanguageParameterName,
                "Output format",
                ShaderCompilerParameterType.ComboBox,
                languages,
                languages[0]);
        }

        public const string InputLanguageParameterName = "__InputLanguage";

        private const string VersionParameterName = "Version";

        public static ShaderCompilerParameter CreateVersionParameter(string binaryFolderName)
        {
            var fullBinaryFolderName = Path.Combine(AppContext.BaseDirectory, "Binaries", binaryFolderName);

            var versionDirectories = Directory
                .GetDirectories(fullBinaryFolderName)
                .Select(x => new DirectoryInfo(x));

            var versions = versionDirectories
                .Select(x => x.Name)
                .ToArray();

            var trunkDescription = string.Empty;
            var trunkVersion = versionDirectories.FirstOrDefault(x => x.Name == "trunk");
            if (trunkVersion != null)
            {
                var trunkVersionLastUpdated = trunkVersion.LastWriteTimeUtc;
                trunkDescription = $"Updated from trunk on {trunkVersionLastUpdated.ToString("yyyy-MM-dd")}";
            }

            return new ShaderCompilerParameter(
                VersionParameterName,
                "Version",
                ShaderCompilerParameterType.ComboBox,
                versions,
                versions.Last(),
                trunkDescription);
        }

        public static string GetBinaryPath(string binaryFolderName, ShaderCompilerArguments arguments, string executableFileName)
        {
            return Path.Combine(
                AppContext.BaseDirectory, 
                "Binaries", 
                binaryFolderName,
                arguments.GetString(VersionParameterName), 
                executableFileName);
        }
    }
}
