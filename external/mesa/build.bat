cd source

mkdir build
cd build

meson --default-library=shared -Dzlib:default_library=static -Dc_std=c17 -Dcpp_std=vc++latest -Db_vscrt=mt --cmake-prefix-path="C:\llvm-10" --pkg-config-path="C:\llvm-10\lib\pkgconfig;C:\llvm-10\share\pkgconfig;C:\spirv-tools\lib\pkgconfig" -Dllvm=enabled -Dshared-llvm=disabled -Dvulkan-drivers=amd -Dgallium-drivers= -Dmicrosoft-clc=disabled -Dbuild-tests=true -Dwerror=true --backend=vs --buildtype=release
msbuild mesa.sln