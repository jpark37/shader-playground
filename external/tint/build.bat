cd source

if NOT EXIST "./scripts/bootstrap.py" (
    call git submodule update --init .
)

if NOT EXIST ".gclient" (
    copy standalone.gclient .gclient
    call gclient sync --no-history
)

mkdir build
cd build

cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release
