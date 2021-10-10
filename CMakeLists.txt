cmake_minimum_required (VERSION 3.16)

project (CHIP8_Emulator)

# set sdl2 compile options
set (SDL_SHARED OFF CACHE BOOL "" FORCE)
set (SDL_STATIC ON CACHE BOOL "" FORCE)
set (SDL_STATIC_PIC ON CACHE BOOL "" FORCE)

add_subdirectory (libs/sdl2)
add_subdirectory (CHIP8_Emu)
