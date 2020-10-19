# build-docker.ps1 uses a Windows docker image to build the Shader Playground
# project and all compiler tools.
#
# Usage (from host):
#
#  Building:
#    .\build-docker.ps1 [build-options]
#
#  Clean build output (entire docker container):
#    .\build-docker.ps1 --clean
#
#  Clean everything (including docker image):
#    .\build-docker.ps1 --rebuild-image
#
#  Start an interactive shell in the docker container:
#    .\build-docker.ps1 --interactive

$ErrorActionPreference = "Stop"

$script_directory = split-path -parent $MyInvocation.MyCommand.Definition
$docker_source_path = "c:/src/shader-playground-host"
$docker_build_path = "c:/src/shader-playground"
$container_name = "shader-playground-build"
$image_name = "shader-playground-build-image"
$total_system_ram_gb = (Get-CimInstance Win32_PhysicalMemory | Measure-Object -Property capacity -Sum).sum /1gb

# This script is executed in both the host and docker environment. It uses the
# environment variable "inside_shader_playground_docker_image" to determine
# which environment it is currently executing. This is set in the Dockerfile.
if ($env:inside_shader_playground_docker_image)
{
    function Init() {
        echo "*** Initializing build directory ***"
        if (Test-Path "${docker_build_path}") { rm -r "${docker_build_path}" }
        mkdir "${docker_build_path}"
        cd "${docker_build_path}"

        git clone "${docker_source_path}" .
        git submodule update --init

        # Patch Hlsl2Glsl so that it actually builds
        git -C "${docker_build_path}/shims/ShaderPlayground.Shims.Hlsl2Glsl/Source" apply "${docker_source_path}/shims/ShaderPlayground.Shims.Hlsl2Glsl/add-algorithm-header.patch"
    }

    # Instead of building with the host's source tree mapped to the container,
    # the project is git-cloned to an isolated build directory, and build
    # artifacts are copied back. There are a couple of reasons for doing this:
    # (1) MSVC has a bug that prevents linking of libraries that are inside a
    #     docker-mapped directory. While Microsoft claims this has been fixed,
    #     the fix doesn't appear to be back ported to older toolchains required
    #     to build this project.
    #     See https://developercommunity.visualstudio.com/content/problem/888534/cl-vc-v1924-crashes-when-building-in-a-docker-volu.html
    # (2) Keeping build artifacts out of the source tree is good hygine, especially
    #     when docker container produced files can have exotic permissions.
    function Sync() {
        echo "*** Synchronizing changes into build directory ***"
        cd "${docker_build_path}"

        # Remove any local changes in ${docker_build_path}
        git checkout .
        git clean -fd

        # Update ${docker_source_path} to match HEAD of the host.
        $branch_name = git -C "${docker_build_path}" symbolic-ref -q --short HEAD
        git fetch origin
        git reset --hard "origin/${branch_name}"
        git submodule update --init

        # Apply any local file diffs
        $local_changes = "C:\TEMP\local-changes.patch"
        git -C "${docker_source_path}" diff HEAD --output "$local_changes"
        git apply "$local_changes"
    }

    function Build() {
        echo "*** Building ***"
        cd "${docker_build_path}"
        $full_args = ("--always-cache=true", "--Verbosity=Diagnostic") + $args
        echo "./build.ps1" @full_args # TEMP
        ./build.ps1 @full_args
    }

    $action = $args[0]
    switch -regex ($action)
    {
        "^init$"
        {
            Init
            Sync
            Break
        }
        "^sync$"
        {
            Sync
            Break
        }
        "^b(uild)?$"
        {
            Sync
            if ($args.Count -gt 1) {
                Build @($args[1..($args.Count-1)])
            } else {
                Build
            }
            Break
        }
    }
}
else
{
    $host_source_path = "$script_directory" -replace "\\", "/"

    function ContainerRun() {
        $params = $(echo @args)
        echo "Running: $params..."
        docker exec "${container_name}" "powershell" @args
        if ($lastexitcode -ne 0) {
            throw ("Running '$params' in docker container errored with: " + $errorMessage)
        }
    }

    function ImageExists() {
        return [bool]($(docker images -q "${image_name}"))
    }

    function MaybeCreateImage() {
        $image_exists = ImageExists
        if(!$image_exists) {
            echo "Creating docker image..."
            docker build "-m${total_system_ram_gb}GB" --tag ${image_name} .
        }
    }

    function DeleteImage() {
        $image_exists = ImageExists
        if ($image_exists) {
            echo "Deleting docker image..."
            docker rm -f "${image_name}"
        }
    }

    function ContainerExists() {
        return [bool]($(docker ps -a -f "name=${container_name}" --format '{{.Names}}') -match "${container_name}")
    }

    function ContainerIsRunning() {
        return [bool]($(docker ps -f "name=${container_name}" --format '{{.Names}}') -match "${container_name}")
    }

    function MaybeCreateContainer() {
        MaybeCreateImage

        $container_exists = ContainerExists
        if(!$container_exists) {
            echo "Building docker container..."
            docker create `
                -t -i `
                --name ${container_name} `
                --storage-opt size=120G `
                "-m${total_system_ram_gb}GB" `
                --cpu-count=$env:NUMBER_OF_PROCESSORS `
                --volume "${host_source_path}:${docker_source_path}:ro" `
                "${image_name}"

            docker start "${container_name}"

            # Initialize the build directory
            ContainerRun("${docker_source_path}/build-docker.ps1", "init")
        }
    }

    function DeleteContainer() {
        $container_exists = ContainerExists
        if ($container_exists) {
            echo "Deleting docker container..."
            docker container rm -f "${container_name}"
        }
    }

    function MaybeStartContainer() {
        MaybeCreateContainer

        $container_is_running = ContainerIsRunning
        if(!$container_is_running) {
            echo "Starting docker container..."
            docker start "${container_name}"
        }
    }

    function StopContainer() {
        echo "Stopping docker container..."
        $container_is_running = ContainerIsRunning
        if($container_is_running) {
            docker stop ${container_name}
        }
    }

    function Build() {
        echo "Building..."
        $full_args = ("${docker_source_path}/build-docker.ps1", "build") + $args
        ContainerRun $full_args
    }

    function CopyArtifact($relPath) {
        echo "Copying '${relPath}' from container..."
        if (Test-Path ".\${relPath}") { rm -r ".\${relPath}" }
        docker cp ${container_name}:"${docker_build_path}\${relPath}" ".\${relPath}"
    }

    function CopyBuildArtifacts() {
        echo "Copying build artifacts..."
        CopyArtifact("build")
        CopyArtifact("src\ShaderPlayground.Core\bin")
        CopyArtifact("src\ShaderPlayground.Core\Binaries")
        CopyArtifact("src\ShaderPlayground.Core.Tests\bin")
    }

    $action = $args[0]
    switch -regex ($action)
    {
        "^(-i|--interactive)$" {
            MaybeStartContainer
            docker exec -it "${container_name}" "powershell" "-NoExit" "function prompt(){'[DOCKER] ' + `$(Get-Location) + '> ' }"
            Break
        }
        "^--rebuild-image$" {
            DeleteContainer
            DeleteImage
            MaybeCreateImage
            MaybeCreateContainer
            Break
        }
        "^--clean$" {
            DeleteContainer
            Break
        }
        default {
            MaybeStartContainer
            Build @args
            StopContainer
            CopyBuildArtifacts
            Break
        }
    }
}
