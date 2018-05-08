#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

var target = Argument("target", "Default");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");
  });

Task("Download-Dxc")
  .Does(() => {
    DownloadFile(
      "https://ci.appveyor.com/api/projects/antiagainst/directxshadercompiler/artifacts/build%2FRelease%2Fbin%2Fdxc-artifacts.zip?branch=master&pr=false",
      "./build/dxc-artifacts.zip");

    ZipUncompress(
      "./build/dxc-artifacts.zip", 
      "./src/ShaderPlayground.Core/Binaries/Dxc");
  });

Task("Download-Glslang")
  .Does(() => {
    DownloadFile(
      "https://github.com/KhronosGroup/glslang/releases/download/master-tot/glslang-master-windows-x64-Release.zip",
      "./build/glslang.zip");

    DeleteDirectory("./build/glslang", true);

    ZipUncompress(
      "./build/glslang.zip", 
      "./build/glslang");

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries/Glslang");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries/Glslang");

    CopyFiles(
      "./build/glslang/bin/*.*",
      "./src/ShaderPlayground.Core/Binaries/Glslang");
  });

Task("Download-Mali-Offline-Compiler")
  .Does(() => {
    if (!FileExists("./build/mali-offline-compiler.zip")) {
      DownloadFile(
        "https://armkeil.blob.core.windows.net/developer/Files/downloads/opengl-es-open-cl-offline-compiler/6.2/Mali_Offline_Compiler_v6.2.0.7d271f_Windows_x64.zip",
        "./build/mali-offline-compiler.zip");
    }

    ZipUncompress(
      "./build/mali-offline-compiler.zip", 
      "./build/mali-offline-compiler");

    EnsureDirectoryExists("./src/ShaderPlayground.Core/Binaries/Mali");
    CleanDirectory("./src/ShaderPlayground.Core/Binaries/Mali");

    CopyFiles(
      "./build/mali-offline-compiler/Mali_Offline_Compiler_v6.2.0/**/*.*",
      "./src/ShaderPlayground.Core/Binaries/Mali",
      true);
  });

Task("Default")
  .IsDependentOn("Prepare-Build-Directory")
  .IsDependentOn("Download-Dxc")
  .IsDependentOn("Download-Glslang")
  .IsDependentOn("Download-Mali-Offline-Compiler")
  .Does(() => {
    Information("Hello World!");
  });

RunTarget(target);