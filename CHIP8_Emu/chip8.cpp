#include "chip8.hpp"
#include "utils.hpp"
#include <ctime>
#include <fstream>

CHIP8::Chip8::Chip8() {
    // init random seed
    srand((unsigned int)time(NULL));

    bus = new BUS();
    cpu = new CPU();
    cpu->InitBus(bus);
    InitFont();
    LoadGameIntoRam(BOOT_ROM, sizeof(BOOT_ROM));
}

CHIP8::Chip8::~Chip8() {

}

void CHIP8::Chip8::LoadGame(std::string filename) {
    std::ifstream ifs(filename, std::ios::binary);
    if (ifs) {
        ifs.seekg(0, ifs.end);
        size_t fileSize = (size_t)ifs.tellg();
        ifs.seekg(0, ifs.beg);

        u8* buffer = new u8[fileSize];
        ifs.read((char*)buffer, fileSize);
        LoadGameIntoRam(buffer, fileSize);
        Reset();

        ifs.close();
        delete[] buffer;
    }
}

int CHIP8::Chip8::LoadGameIntoRam(const u8* game, size_t size) {
    if (size <= sizeof(bus->ram)) {
        memcpy(bus->ram + ROM_OEP, game, size);
        return 0;
    }

    return 1;
}

void CHIP8::Chip8::Reset() {
    bus->Reset();
    cpu->Reset();
}

int CHIP8::Chip8::InitFont() {
    if (sizeof(FONTS) <= sizeof(bus->ram)) {
        memcpy(bus->ram, FONTS, sizeof(FONTS));
        return 0;
    }
    return 1;
}

void CHIP8::Chip8::Process() {
    u64 now = GetNanoTimestamp();
    if (oldTime == 0) {
        oldTime = now;
    }

    u64 stepCount = u64(double(now - oldTime) / 1000000000.0l * double(cpu->GetInstPerSecond()));
    for (u64 i = 0; i < stepCount; i++) {
        if (cpu->IsWaitingKey()) {
            break;
        }
        cpu->StepOver();
    }
    oldTime = now;
}

void CHIP8::Chip8::PressKey(u32 index) {
    if (index < 0x10) {
        bus->k[index] = true;

        // if waiting for a key, set it now
        if (cpu->IsWaitingKey()) {
            cpu->UpdateWaitKey(index);
        }
    }
}

void CHIP8::Chip8::ReleaseKey(u32 index) {
    if (index < 0x10) {
        bus->k[index] = false;
    }
}

u8 CHIP8::Chip8::GetPixel(u32 x, u32 y) {
    return bus->frameBuffer[x + y * SCR_WEIGHT];
}

u64 CHIP8::Chip8::GetFakeST() {
    return cpu->GetFakeST();
}
