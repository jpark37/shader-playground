## Building Shader Playground

Shader Playground depends on a significant number of compilers, and building
these requires a number of different toolchains installed. These toolchains can
be manually installed, or you can use a Docker image and container.

### Building with the Docker image

#### Pre-requisites

The following must be installed:

* [Docker Desktop for Windows](https://docs.docker.com/docker-for-windows/install/)
* [Git](https://git-scm.com/download/win)

#### Building

1. [First, ensure that Docker is running with `Windows containers`](https://docs.docker.com/docker-for-windows/#:~:text=Switch%20between%20Windows%20and%20Linux,Linux%20containers%20(the%20default)).

1. Fetch the Shader Playground source:

```powershell
# cd to your preferred location
git clone https://github.com/tgjones/shader-playground.git
cd shader-playground
```

2. Build Docker image, container, and project:

```powershell
.\build-docker.ps1
```

Be aware that this will take a _long_ time, the first time, exceptionally so.

The build artifacts will copied into the project sub-directories.

### Building without the Docker image

#### Pre-requisites

The following must be installed:
* [Visual Studio 2017](https://my.visualstudio.com/Downloads?q=visual%20studio%202017)
 * And these extra packages:
    * `Microsoft.VisualStudio.Workload.VCTools`
    * `Microsoft.VisualStudio.Workload.NativeDesktop`
    * `Microsoft.VisualStudio.Component.VC.ATLMFC`
    * `Microsoft.Component.MSBuild`
* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
 * And these extra packages:
    `Microsoft.VisualStudio.Component.VC.ATLMFC`
    `Microsoft.VisualStudio.Workload.NativeDesktop`
    `Microsoft.VisualStudio.Workload.VCTools`
    `Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools`
    `Microsoft.VisualStudio.Workload.NetCoreBuildTools`
    `Microsoft.VisualStudio.Workload.WebBuildTools`
    `Microsoft.VisualStudio.Component.VC.ATLMFC`
* [Windows SDK 19041](https://download.microsoft.com/download/1/c/3/1c3d5161-d9e9-4e4b-9b43-b70fe8be268c/windowssdk/winsdksetup.exe)
* [Windows SDK 17134](https://download.microsoft.com/download/5/A/0/5A08CEF4-3EC9-494A-9578-AB687E716C12/windowssdk/winsdksetup.exe)
* [CMake](https://cmake.org/download/)
* [7-Zip](https://www.7-zip.org/)
  * `7zip.exe` also be added to `PATH`
* [python2](https://www.python.org/download/releases/2.0/)
  * `PYTHON2` enviroment variable must point to `python.exe`. `python.exe` must not be on `PATH`
* [python3](https://www.python.org/downloads/)
  * `PYTHON3` must be on `PATH`
* [depot_tools](https://storage.googleapis.com/chrome-infra/depot_tools.zip)
  * This directory must be on `PATH`

 #### Building

1. Fetch the Shader Playground source:

```powershell
# cd to your preferred location
git clone https://github.com/tgjones/shader-playground.git
cd shader-playground
```

2. Build Docker image, container, and project:

```powershell
.\build.ps1
```
