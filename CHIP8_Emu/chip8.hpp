#pragma once
#include "cpu.hpp"
#include "bus.hpp"
#include <string>

namespace CHIP8 {
    class Chip8 {
    private:
        CPU* cpu = nullptr;
        BUS* bus = nullptr;
        u64 oldTime = 0;

    public:
        Chip8();
        ~Chip8();
        void LoadGame(std::string filename);
        int LoadGameIntoRam(const u8* game, size_t size);
        void Reset();
        int InitFont();
        void Process();
        void PressKey(u32 index);
        void ReleaseKey(u32 index);
        u8 GetPixel(u32 x, u32 y);
        u64 GetFakeST();
    };
}
