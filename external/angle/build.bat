cd source

if NOT EXIST "./scripts/bootstrap.py" (
    call git submodule update --init .
)

if EXIST "%PYTHON2%" (
    call %PYTHON2% scripts/bootstrap.py
) ELSE (
    call python scripts/bootstrap.py
)

call gclient sync --no-history
call gn gen out/Release
call ninja -C out/Release angle_shader_translator
