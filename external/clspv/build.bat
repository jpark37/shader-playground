cd source

python utils/fetch_sources.py

mkdir build
cd build

cmake ..
cmake --build .