cmake_minimum_required (VERSION 3.16)

project (CHIP8_Emu)

file (
    GLOB SOURCE_FILES
    ${PROJECT_SOURCE_DIR}/src/*
)

if (WIN32)
    # set /SUBSYSTEM:WINDOWS
    # https://stackoverflow.com/a/57645234
    add_executable (
        ${PROJECT_NAME}
        WIN32
        ${SOURCE_FILES}
    )
else ()
    add_executable (
        ${PROJECT_NAME}
        ${SOURCE_FILES}
    )
endif (WIN32)

target_include_directories (
    ${PROJECT_NAME}
    PRIVATE 
        ${PROJECT_SOURCE_DIR}/include
)

target_link_libraries (
    ${PROJECT_NAME}
    SDL2-static
    SDL2main
)
