cd source

call python scripts/bootstrap.py
call gclient sync
call git checkout master

call gn gen out/Release

call ninja -C out/Release shader_translator