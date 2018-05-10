#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");
  });

void DownloadCompiler(string url, string binariesFolderName, string filesToCopy)
{
  var tempFileName = $"./build/{binariesFolderName}.zip";
  var unzippedFolder = $"./build/{binariesFolderName}";

  if (!FileExists(tempFileName)) {
    DownloadFile(url, tempFileName);
  }

  CleanDirectory(unzippedFolder);

  ZipUncompress(tempFileName, unzippedFolder);

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
    DownloadCompiler(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fbin%2Fdxc-artifacts.zip?branch=master&pr=false",
      "Dxc",
      "**/*.*");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadCompiler(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "Glslang",
      "bin/*.*");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    DownloadCompiler(
      "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
      "Mali",
      "Mali_Offline_Compiler_v6.2.0/**/*.*");
  });

Task("Build-Shims")
  .Does(() => {
    DotNetCoreBuild("./shims/ShaderPlayground.Shims.sln", new DotNetCoreBuildSettings
    {
      Configuration = configuration
    });

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries/Fxc");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries/Fxc");

    CopyFiles(
      $"./shims/ShaderPlayground.Shims.Fxc/bin/{configuration}/netcoreapp2.0/**/*.*",
      "./src/ShaderPlayground.Core/Binaries/Fxc",
      true);
  });

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
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(target);