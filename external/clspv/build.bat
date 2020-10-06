cd source

if NOT EXIST "./scripts/bootstrap.py" (
    call git submodule update --init .
)

if EXIST "%PYTHON2%" (
    call %PYTHON2% utils/fetch_sources.py
) ELSE (
    call python utils/fetch_sources.py
)

mkdir build
cd build

cmake ..
cmake --build .