cd source

if NOT EXIST "./scripts/bootstrap.py" (
    call git submodule update --init .
)

mkdir build
cd build

cmake ..
cmake --build .