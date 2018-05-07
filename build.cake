#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

var target = Argument("target", "Default");

Task("Prepare-Build-Directory")
  .Does(() => {
    EnsureDirectoryExists("./build");
    CleanDirectory("./build");
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

Task("Default")
  .IsDependentOn("Prepare-Build-Directory")
  .IsDependentOn("Download-Dxc")
  .Does(() => {
    Information("Hello World!");
  });

RunTarget(target);