#addin nuget:?package=SharpZipLib&version=1.0.0
#addin nuget:?package=Cake.Compression&version=0.2.1
#addin nuget:?package=Cake.Git&version=0.18.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var windowsSdkVersion = "10.0.18362.0";

void RunAndCheckResult(FilePath exe, ProcessSettings settings)
{
    var res = StartProcess(exe, settings);
    if (res != 0)
    {
      throw new Exception($"'{exe}' failed with error code {res}");
    }
}

string DownloadCompiler(string url, string binariesFolderName, string version, bool cache)
{
  var tempFileName = $"./build/{binariesFolderName}-{version}.zip";

  if (!cache || !FileExists(tempFileName)) {
    DownloadFile(url, tempFileName);
  }

  return tempFileName;
}

enum ZipFormat
{
  Zip,
  GZip,
  SevenZip
}

string GetFullPath(string fileName)
{
    if (FileExists(fileName))
    {
        return fileName;
    }

    var values = Environment.GetEnvironmentVariable("PATH");
    foreach (var path in values.Split(System.IO.Path.PathSeparator))
    {
        var dir = DirectoryPath.FromString(path);
        var fullPath = dir.GetFilePath(fileName).ToString();
        if (FileExists(fullPath))
        {
            return fullPath;
        }
    }
    return null;
}

void Unzip(string zip, string dstFolder, string filesToCopy, ZipFormat format)
{
  switch (format)
  {
    case ZipFormat.Zip:
    case ZipFormat.GZip:
    {
      if (filesToCopy == null)
      {
        EnsureDirectoryExists(dstFolder);
        CleanDirectory(dstFolder);
        if (format == ZipFormat.Zip)
        {
          ZipUncompress(zip, dstFolder);
        }
        else
        {
          GZipUncompress(zip, dstFolder);
        }
      } else
      {
        string unzippedFolder = $"{zip}.unzipped";
        EnsureDirectoryExists(unzippedFolder);
        CleanDirectory(unzippedFolder);
        if (format == ZipFormat.Zip)
        {
          ZipUncompress(zip, unzippedFolder);
        }
        else
        {
          GZipUncompress(zip, unzippedFolder);
        }
        EnsureDirectoryExists(dstFolder);
        CleanDirectory(dstFolder);
        CopyFiles($"{unzippedFolder}/{filesToCopy}", dstFolder, true);
        DeleteDirectory(unzippedFolder, new DeleteDirectorySettings {
          Recursive = true,
          Force = true
        });
      }
      break;
    }
    case ZipFormat.SevenZip:
    {
      string exe = @"C:\Program Files\7-Zip\7z.exe";
      if (!FileExists(exe))
      {
        exe = GetFullPath("7z.exe");
      }
      if (!FileExists(exe))
      {
        throw new InvalidOperationException("Unable to find 7z.exe in Program Files or PATH");
      }
      RunAndCheckResult(exe,
        new ProcessSettings
        {
          Arguments = $@"e -o""{dstFolder}"" ""{zip}"" {filesToCopy}"
        }
      );
      break;
    }
    default:
      throw new InvalidOperationException();
  }
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache, ZipFormat format = ZipFormat.Zip)
{
  var tempFileName = DownloadCompiler(url, binariesFolderName, version, cache);
  var unzippedFolder = $"./build/{binariesFolderName}/{version}";

  Unzip(tempFileName, unzippedFolder, null, format);

  return unzippedFolder;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName, string version, bool cache, string filesToCopy, ZipFormat format = ZipFormat.Zip)
{
  var tempFileName = DownloadCompiler(url, binariesFolderName, version, cache);
  var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/{binariesFolderName}/{version}";

  Unzip(tempFileName, binariesFolder, filesToCopy, format);

  return binariesFolder;
}

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries");
  });

Task("Download-Dxc")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fdxc-artifacts.zip?branch=master&pr=false&job=Image%3A%20Visual%20Studio%202017",
      "dxc",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "glslang",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
      "mali",
      "6.2.0",
      true,
      "Mali_Offline_Compiler_v6.2.0/**/*.*");
  });

Task("Download-SPIRV-Cross")
  .Does(() => {
    void DownloadSpirvCross(string version, string hash)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/KhronosGroup/SPIRV-Cross/releases/download/{version}/spirv-cross-vs2017-64bit-{hash}.tar.gz",
        "spirv-cross",
        version,
        true,
        "bin/spirv-cross.exe",
        ZipFormat.GZip);
    }

    DownloadSpirvCross("2019-06-21", "b4e0163749");
    DownloadSpirvCross("2020-01-16", "f9818f0804");
    DownloadSpirvCross("2020-09-17", "8891bd3512");

    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Cross/archive/master.zip",
      "spirv-cross",
      "trunk",
      false);

    var srcDirectory = $"{unzippedFolder}/SPIRV-Cross-master";
    RunAndCheckResult(
        @"cmake.exe",
        new ProcessSettings
        {
          Arguments = ".",
          WorkingDirectory = srcDirectory
        });

    MSBuild(srcDirectory + "/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{srcDirectory}/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Cross-ISPC")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/GameTechDev/SPIRV-Cross/archive/master-ispc.zip",
      "spirv-cross-ispc",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/SPIRV-Cross-master-ispc/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/spirv-cross-ispc/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master-ispc/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-SPIRV-Tools")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Tools/releases/download/master-tot/SPIRV-Tools-master-windows-x64-Release.zip",
      "spirv-tools",
      "trunk",
      false,
      "bin/*.*");
  });

Task("Download-XShaderCompiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/LukasBanana/XShaderCompiler/releases/download/v0.10-alpha/Xsc-v0.10-alpha.zip",
      "xshadercompiler",
      "v0.10-alpha",
      true,
      "Xsc-v0.10-alpha/bin/Win32/xsc.exe");
  });

Task("Download-Slang")
  .Does(() => {
    void DownloadSlang(string version)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/shader-slang/slang/releases/download/v{version}/slang-{version}-win64.zip",
        "slang",
        $"v{version}",
        true,
        "bin/windows-x64/release/*.*");
    }

    DownloadSlang("0.10.24");
    DownloadSlang("0.10.25");
    DownloadSlang("0.10.26");
    DownloadSlang("0.11.18");
    DownloadSlang("0.13.10");
  });

Task("Download-HLSLParser")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/Thekla/hlslparser/archive/master.zip",
      "hlslparser",
      "trunk",
      false);

    MSBuild(unzippedFolder + "/hlslparser-master/hlslparser.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .WithProperty("PlatformToolset", "v141"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/hlslparser/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/hlslparser-master/{configuration}/hlslparser.exe",
      binariesFolder,
      true);
  });

Task("Download-zstd")
  .Does(() => {
    void DownloadZstd(string version)
    {
      DownloadAndUnzipCompiler(
        $"https://github.com/facebook/zstd/releases/download/v{version}/zstd-v{version}-win64.zip",
        "zstd",
        $"v{version}",
        true,
        "zstd.exe");
    }

    DownloadZstd("1.3.4");
  });

Task("Download-LZMA")
  .Does(() => {
    void DownloadLzma(string version, string displayVersion)
    {
      DownloadAndUnzipCompiler(
        $"https://www.7-zip.org/a/lzma{version}.7z",
        "lzma",
        displayVersion,
        true,
        @"bin\lzma.exe",
        ZipFormat.SevenZip);
    }

    DownloadLzma("1805", "18.05");
  });

Task("Download-RGA")
  .Does(() => {
    var amdDriverExePath = DownloadCompiler(
      "https://drivers.amd.com/drivers/win10-64bit-radeon-software-adrenalin-2019-edition-19.9.2-sep23.exe",
      "amd-driver", 
      "19.9.2", 
      true);

    var amdDriverFolder = "./build/amd-driver/19.9.2";
    EnsureDirectoryExists(amdDriverFolder);
    CleanDirectory(amdDriverFolder);

    void ExtractFile(string fileName)
    {
      StartProcess(
        @"C:\Program Files\7-Zip\7z.exe",
        $@"e -o""{amdDriverFolder}"" ""{amdDriverExePath}"" Packages\Drivers\Display\WT6A_INF\B346681\{fileName}");
    }

    ExtractFile("atidxx64.dll");
    ExtractFile("amdvlk64.dll");

    var driverDllPaths = new[]
    {
      amdDriverFolder + "/atidxx64.dll",
      amdDriverFolder + "/amdvlk64.dll"
    };

    void DownloadRga(string version, string filesToCopy)
    {
      var binariesFolder = DownloadAndUnzipCompiler(
        $"https://github.com/GPUOpen-Tools/radeon_gpu_analyzer/releases/download/{version}/rga-windows-x64-{version}.zip",
        "rga",
        version,
        true,
        filesToCopy);

      CopyFiles(driverDllPaths, binariesFolder);
    }

    DownloadRga("2.0.1", "bin/**/*.*");
    DownloadRga("2.1", "**/*.*");
    DownloadRga("2.2", "**/*.*");
    DownloadRga("2.3", "**/*.*");
    DownloadRga("2.3.1", "**/*.*");
  });

Task("Download-IntelShaderAnalyzer")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/GameTechDev/IntelShaderAnalyzer/releases/download/v1/IntelShaderAnalyzer_v1.zip",
      "intelshaderanalyzer",
      "v1",
      true,
      "IntelShaderAnalyzer/*.*");
  });

  Task("Copy-PowerVR")
  .Does(() => {
    void CopyVersion(string version)
    {
      var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/powervr/{version}";
      EnsureDirectoryExists(binariesFolder);
      CleanDirectory(binariesFolder);

      CopyFiles($"./lib/PowerVR/{version}/*.*", binariesFolder);
    }

    CopyVersion("2018 R1");
  });

Task("Build-ANGLE")
  .Does(() => {
    RunAndCheckResult(MakeAbsolute(File("./external/angle/build.bat")), new ProcessSettings {
      WorkingDirectory = MakeAbsolute(Directory("./external/angle"))
    });

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/angle/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      "./external/angle/source/out/Release/shader_translator.exe",
      binariesFolder,
      true);
  });

Task("Build-Clspv")
  .Does(() => {
    RunAndCheckResult(MakeAbsolute(File("./external/clspv/build.bat")), new ProcessSettings {
      WorkingDirectory = MakeAbsolute(Directory("./external/clspv"))
    });

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/clspv/trunk";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      "./external/clspv/build/bin/clspv.exe",
      binariesFolder,
      true);
  });

Task("Build-Fxc-Shim")
  .Does(() => {
    DotNetCorePublish("./shims/ShaderPlayground.Shims.Fxc/ShaderPlayground.Shims.Fxc.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration
    });

    var fxcVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo("./lib/x64/d3dcompiler_47.dll").ProductVersion;
    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/fxc/{fxcVersion}";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Fxc/bin/{configuration}/netcoreapp3.1/publish/**/*.*",
      binariesFolder,
      true);
  });

Task("Build-HLSLcc-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.HLSLcc/ShaderPlayground.Shims.HLSLcc.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlslcc/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.HLSLcc/{configuration}/ShaderPlayground.Shims.HLSLcc.exe",
      binariesFolder,
      true);
  });

Task("Build-GLSL-Optimizer-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/Source/projects/vs2010/glsl_optimizer_lib.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.GlslOptimizer/ShaderPlayground.Shims.GlslOptimizer.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/glsl-optimizer/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.GlslOptimizer/{configuration}/ShaderPlayground.Shims.GlslOptimizer.exe",
      binariesFolder,
      true);
  });

Task("Build-HLSL2GLSL-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/Source/hlslang.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("PlatformToolset", "v141")
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .WithProperty("ForcedIncludeFiles", "<algorithm>")
      .SetConfiguration(configuration)
      .SetPlatformTarget(PlatformTarget.Win32));

    MSBuild("./shims/ShaderPlayground.Shims.Hlsl2Glsl/ShaderPlayground.Shims.Hlsl2Glsl.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/hlsl2glsl/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Hlsl2Glsl/{configuration}/ShaderPlayground.Shims.Hlsl2Glsl.exe",
      binariesFolder,
      true);
  });

Task("Build-SMOL-V-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Smolv/ShaderPlayground.Shims.Smolv.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/smol-v/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Smolv/{configuration}/ShaderPlayground.Shims.Smolv.exe",
      binariesFolder,
      true);
  });

Task("Build-Miniz-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Miniz/ShaderPlayground.Shims.Miniz.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/miniz/2.0.7";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Miniz/{configuration}/ShaderPlayground.Shims.Miniz.exe",
      binariesFolder,
      true);
  });

Task("Build-YARI-V-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.Yariv/ShaderPlayground.Shims.Yariv.vcxproj", new MSBuildSettings()
      .UseToolVersion(MSBuildToolVersion.VS2019)
      .WithProperty("WindowsTargetPlatformVersion", windowsSdkVersion)
      .SetConfiguration(configuration));

    var binariesFolder = "./src/ShaderPlayground.Core/Binaries/yari-v/trunk";

    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Yariv/{configuration}/ShaderPlayground.Shims.Yariv.exe",
      binariesFolder,
      true);
  });

Task("Build-Shims")
  .IsDependentOn("Build-Fxc-Shim")
  .IsDependentOn("Build-HLSLcc-Shim")
  .IsDependentOn("Build-GLSL-Optimizer-Shim")
  .IsDependentOn("Build-HLSL2GLSL-Shim")
  .IsDependentOn("Build-SMOL-V-Shim")
  .IsDependentOn("Build-Miniz-Shim")
  .IsDependentOn("Build-YARI-V-Shim");

Task("Build")
  .IsDependentOn("Build-Shims")
  .Does(() => {
    var outputFolder = "./build/site";
    EnsureDirectoryExists(outputFolder);
    CleanDirectory(outputFolder);

    DotNetCorePublish("./src/ShaderPlayground.Web/ShaderPlayground.Web.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration,
      OutputDirectory = outputFolder
    });

    ZipCompress(outputFolder, "./build/site.zip");
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    DotNetCoreTest("./src/ShaderPlayground.Core.Tests/ShaderPlayground.Core.Tests.csproj", new DotNetCoreTestSettings
    {
      Configuration = configuration
    });
  });

Task("Default")
  .IsDependentOn("Prepare-Build-Directory")
  .IsDependentOn("Download-Dxc")
  .IsDependentOn("Download-Glslang")
  .IsDependentOn("Download-Mali-Offline-Compiler")
  .IsDependentOn("Download-SPIRV-Cross")
  .IsDependentOn("Download-SPIRV-Tools")
  .IsDependentOn("Download-XShaderCompiler")
  .IsDependentOn("Download-SPIRV-Cross-ISPC")
  .IsDependentOn("Download-Slang")
  .IsDependentOn("Download-HLSLParser")
  .IsDependentOn("Download-zstd")
  .IsDependentOn("Download-LZMA")
  .IsDependentOn("Download-RGA")
  .IsDependentOn("Download-IntelShaderAnalyzer")
  .IsDependentOn("Copy-PowerVR")
  .IsDependentOn("Build-ANGLE")
  .IsDependentOn("Build-Clspv")
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(target);