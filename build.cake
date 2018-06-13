#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression
#addin nuget:?package=Cake.Git

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");
  });

string DownloadCompiler(string url, string binariesFolderName)
{
  var tempFileName = $"./build/{binariesFolderName}.zip";

  if (!FileExists(tempFileName)) {
    DownloadFile(url, tempFileName);
  }

  return tempFileName;
}

string DownloadAndUnzipCompiler(string url, string binariesFolderName)
{
  var tempFileName = DownloadCompiler(url, binariesFolderName);
  var unzippedFolder = $"./build/{binariesFolderName}";

  CleanDirectory(unzippedFolder);

  ZipUncompress(tempFileName, unzippedFolder);

  return unzippedFolder;
}

void DownloadAndUnzipCompiler(string url, string binariesFolderName, string filesToCopy)
{
  var unzippedFolder = DownloadAndUnzipCompiler(url, binariesFolderName);

  var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/{binariesFolderName}";
  EnsureDirectoryExists(binariesFolder);
  CleanDirectory(binariesFolder);

  CopyFiles(
    $"{unzippedFolder}/{filesToCopy}",
    binariesFolder,
    true);
}

Task("Download-Dxc")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fbin%2Fdxc-artifacts.zip?branch=master&pr=false",
      "Dxc",
      "**/*.*");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "Glslang",
      "bin/*.*");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
      "Mali",
      "Mali_Offline_Compiler_v6.2.0/**/*.*");
  });

Task("Download-SPIRV-Cross")
  .Does(() => {
    var tempFileName = DownloadCompiler(
      "https://sdk.lunarg.com/sdk/download/1.1.73.0/windows/VulkanSDK-1.1.73.0-Installer.exe?u=",
      "SpirVCross");

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/SpirVCross";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    StartProcess(
      @"C:\Program Files\7-Zip\7z.exe",
      $@"e -o""{binariesFolder}"" ""{tempFileName}"" Bin\spirv-cross.exe");
  });

Task("Download-SPIRV-Tools")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/KhronosGroup/SPIRV-Tools/releases/download/master-tot/SPIRV-Tools-master-windows-x64-Release.zip",
      "SpirVTools",
      "bin/*.*");
  });

Task("Download-XShaderCompiler")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/LukasBanana/XShaderCompiler/releases/download/v0.10-alpha/Xsc-v0.10-alpha.zip",
      "XShaderCompiler",
      "Xsc-v0.10-alpha/bin/Win32/xsc.exe");
  });

Task("Download-SPIRV-Cross-ISPC")
  .Does(() => {
    var unzippedFolder = DownloadAndUnzipCompiler(
      "https://github.com/GameTechDev/SPIRV-Cross/archive/master-ispc.zip",
      "SpirVCrossIspc");

    MSBuild(unzippedFolder + "/SPIRV-Cross-master-ispc/msvc/SPIRV-Cross.vcxproj", new MSBuildSettings()
      .SetConfiguration(configuration)
      .WithProperty("WindowsTargetPlatformVersion", "10.0.17134.0"));

    var binariesFolder = $"./src/ShaderPlayground.Core/Binaries/SpirVCrossIspc";
    EnsureDirectoryExists(binariesFolder);
    CleanDirectory(binariesFolder);

    CopyFiles(
      $"{unzippedFolder}/SPIRV-Cross-master-ispc/msvc/{configuration}/SPIRV-Cross.exe",
      binariesFolder,
      true);
  });

Task("Download-Slang")
  .Does(() => {
    DownloadAndUnzipCompiler(
      "https://github.com/shader-slang/slang/releases/download/v0.10.24/slang-0.10.24-win64.zip",
      "Slang",
      "bin/windows-x64/release/*.*");
  });

Task("Build-Fxc-Shim")
  .Does(() => {
    DotNetCorePublish("./shims/ShaderPlayground.Shims.Fxc/ShaderPlayground.Shims.Fxc.csproj", new DotNetCorePublishSettings
    {
      Configuration = configuration
    });

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries/Fxc");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries/Fxc");

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Fxc/bin/{configuration}/netcoreapp2.0/publish/**/*.*",
      "./src/ShaderPlayground.Core/Binaries/Fxc",
      true);
  });

Task("Build-HLSLcc-Shim")
  .Does(() => {
    MSBuild("./shims/ShaderPlayground.Shims.HLSLcc/ShaderPlayground.Shims.HLSLcc.vcxproj", new MSBuildSettings
    {
      Configuration = configuration
    });

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries/HLSLcc");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries/HLSLcc");

    CopyFiles(
      $"./shims/{configuration}/ShaderPlayground.Shims.HLSLcc.exe",
      "./src/ShaderPlayground.Core/Binaries/HLSLcc",
      true);
  });

Task("Build-Shims")
  .IsDependentOn("Build-Fxc-Shim")
  .IsDependentOn("Build-HLSLcc-Shim");

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
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(target);