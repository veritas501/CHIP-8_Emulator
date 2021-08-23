#pragma once
#include "constants.hpp"
#include "types.hpp"
#include <cstdint>

namespace CHIP8 {
    class BUS {
    public:
        // chip8's ram
        u8 ram[CHIP8_RAM_SIZE];

        // chip8's 16 keys
        bool k[0x10];

        // chip8's screen frame buffer
        u8 frameBuffer[SCR_WEIGHT * SCR_HEIGHT];

        void Reset();
        bool KeyIsPressed(u8 index);
    };
}
