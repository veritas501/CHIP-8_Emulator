#include "cpu.hpp"
#include "bus.hpp"
#include "utils.hpp"
#include <cstring>
#include <cstdlib>
#include <iostream>

using namespace std;

CHIP8::CPU::CPU() {
    pc = ROM_OEP;
    instPerSecond = CHIP8_CPU_SPEED / CHIP8_CYCLE_PRE_INST;
}

CHIP8::CPU::~CPU() {

}

void CHIP8::CPU::InitBus(BUS* _bus) {
    bus = _bus;
}

void CHIP8::CPU::Reset() {
    pc = ROM_OEP;
    sp = 0;
    memset(v, 0, sizeof(v));
    idx = 0;
    fakeST = 0;
    fakeDT = 0;
    waitKey = nullptr;
}

void CHIP8::CPU::StepOver() {
    u16 inst = u16(bus->ram[pc] << 8) + u16(bus->ram[pc + 1]);
    pc += 2;
    Dispatch(inst);
}

void CHIP8::CPU::Dispatch(u16 inst) {
    u8 x = (inst & 0x0F00) >> 8;
    u8 y = (inst & 0x00F0) >> 4;
    u16 nnn = inst & 0x0FFF;
    u8 nibble = inst & 0x000F;
    u8 kk = inst & 0x00FF;

    if (inst == 0x00E0) {
        Cls();
    }
    else if (inst == 0x00EE) {
        Return();
    }
    else if ((inst & 0xF000) == 0x1000) {
        JumpImm(nnn);
    }
    else if ((inst & 0xF000) == 0x2000) {
        CallImm(nnn);
    }
    else if ((inst & 0xF000) == 0x3000) {
        SkipIfEqualImm(x, kk);
    }
    else if ((inst & 0xF000) == 0x4000) {
        SkipIfNotEqualImm(x, kk);
    }
    else if ((inst & 0xF00F) == 0x5000) {
        SkipIfEqualReg(x, y);
    }
    else if ((inst & 0xF000) == 0x6000) {
        MovImm(x, kk);
    }
    else if ((inst & 0xF000) == 0x7000) {
        AddImm(x, kk);
    }
    else if ((inst & 0xF00F) == 0x8000) {
        MovReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8001) {
        OrReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8002) {
        AndReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8003) {
        XorReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8004) {
        AddReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8005) {
        SubReg(x, y);
    }
    else if ((inst & 0xF00F) == 0x8006) {
        Shr(x, y);
    }
    else if ((inst & 0xF00F) == 0x8007) {
        SubReverse(x, y);
    }
    else if ((inst & 0xF00F) == 0x800E) {
        Shl(x, y);
    }
    else if ((inst & 0xF00F) == 0x9000) {
        SkipIfNotEqualReg(x, y);
    }
    else if ((inst & 0xF000) == 0xA000) {
        LoadIndexImm(nnn);
    }
    else if ((inst & 0xF000) == 0xB000) {
        JumpRelV0(nnn);
    }
    else if ((inst & 0xF000) == 0xC000) {
        RandReg(x, kk);
    }
    else if ((inst & 0xF000) == 0xD000) {
        Draw(x, y, nibble);
    }
    else if ((inst & 0xF0FF) == 0xE09E) {
        SkipIfPressed(x);
    }
    else if ((inst & 0xF0FF) == 0xE0A1) {
        SkipIfNotPressed(x);
    }
    else if ((inst & 0xF0FF) == 0xF007) {
        LoadDT(x);
    }
    else if ((inst & 0xF0FF) == 0xF00A) {
        WaitForKey(x);
    }
    else if ((inst & 0xF0FF) == 0xF015) {
        SetDTReg(x);
    }
    else if ((inst & 0xF0FF) == 0xF018) {
        SetSTReg(x);
    }
    else if ((inst & 0xF0FF) == 0xF01E) {
        AddIndexReg(x);
    }
    else if ((inst & 0xF0FF) == 0xF029) {
        LoadFontReg(x);
    }
    else if ((inst & 0xF0FF) == 0xF033) {
        StoreBCD(x);
    }
    else if ((inst & 0xF0FF) == 0xF055) {
        StoreReg(x);
    }
    else if ((inst & 0xF0FF) == 0xF065) {
        LoadReg(x);
    }
}

u64 CHIP8::CPU::GetInstPerSecond() {
    return instPerSecond;
}

void CHIP8::CPU::SetInstPerSecond(u64 imm) {
    instPerSecond = imm;
}

bool CHIP8::CPU::IsWaitingKey() {
    return waitKey ? true : false;
}

void CHIP8::CPU::UpdateWaitKey(u8 key) {
    *waitKey = key;
    waitKey = nullptr;
}


// ======================= insts =======================

// Syscall 0nnn - SYS <addr>
// Jump to a machine code routine at nnn. This instruction is only used on the old computers on which Chip-8
// was originally implemented. It is ignored by modern interpreters. This will not be implemented.
void CHIP8::CPU::Syscall() {
    // do nothing;
}

// Cls 00E0 - CLS
// Clear the display.
void CHIP8::CPU::Cls() {
    memset(bus->frameBuffer, 0, sizeof(bus->frameBuffer));
}

// Return 00EE - RET
// Return from a subroutine.The interpreter sets the program counter to the address at the top of the stack,
// then subtracts 1 from the stack pointer.
void CHIP8::CPU::Return() {
    if (sp != 0) {
        pc = stack[sp];
        sp--;
    }
}

// JumpImm 1nnn - JP addr
// Jump to location nnn. The interpreter sets the program counter to nnn.
void CHIP8::CPU::JumpImm(u16 p) {
    pc = p;
}

// CallImm 2nnn - CALL addr
// Call subroutine at nnn. The interpreter increments the stack pointer, then puts the current PC on the top
// of the stack. The PC is then set to nnn.
void CHIP8::CPU::CallImm(u16 p) {
    sp++;
    stack[sp] = pc;
    pc = p;
}

// SkipIfEqualImm 3xkk - SE Vx, byte
// Call subroutine at nnn. The interpreter increments the stack pointer, then puts the current PC on the top
// of the stack. The PC is then set to nnn.
void CHIP8::CPU::SkipIfEqualImm(u8 x, u8 imm) {
    if (v[x] == imm) {
        pc += 2;
    }
}

// SkipIfNotEqualImm 4xkk - SNE Vx, byte
// Skip next instruction if Vx != kk. The interpreter compares register Vx to kk, and if they are not equal,
// increments the program counter by 2.
void CHIP8::CPU::SkipIfNotEqualImm(u8 x, u8 imm) {
    if (v[x] != imm) {
        pc += 2;
    }
}

// SkipIfEqualReg 5xy0 - SE Vx, Vy
// Skip next instruction if Vx = Vy. The interpreter compares register Vx to register Vy, and if they are equal,
// increments the program counter by 2.
void CHIP8::CPU::SkipIfEqualReg(u8 x, u8 y) {
    if (v[x] != v[y]) {
        pc += 2;
    }
}

// MovImm 6xkk - LD Vx, byte
// Set Vx = kk. The interpreter puts the value kk into register Vx.
void CHIP8::CPU::MovImm(u8 x, u8 imm) {
    v[x] = imm;
}

// AddImm 7xkk - ADD Vx, byte
// Set Vx = Vx + kk. Adds the value kk to the value of register Vx, then stores the result in Vx.
void CHIP8::CPU::AddImm(u8 x, u8 imm) {
    v[x] += imm;
}

// MovReg 8xy0 - LD Vx, Vy
// Set Vx = Vy. Stores the value of register Vy in register Vx.
void CHIP8::CPU::MovReg(u8 x, u8 y) {
    v[x] = v[y];
}

// OrReg 8xy1 - OR Vx, Vy
// Set Vx = Vx OR Vy. Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx. A
// bitwise OR compares the corresponding bits from two values, and if either bit is 1, then the same bit in the
// result is also 1. Otherwise, it is 0.
void CHIP8::CPU::OrReg(u8 x, u8 y) {
    v[x] |= v[y];
}

// AndReg 8xy2 - AND Vx, Vy
// Set Vx = Vx AND Vy. Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
// A bitwise AND compares the corresponding bits from two values, and if both bits are 1, then the same bit
// in the result is also 1. Otherwise, it is 0.
void CHIP8::CPU::AndReg(u8 x, u8 y) {
    v[x] &= v[y];
}

// XorReg 8xy3 - XOR Vx, Vy
// Set Vx = Vx XOR Vy. Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result
// in Vx. An exclusive OR compares the corresponding bits from two values, and if the bits are not both the
// same, then the corresponding bit in the result is set to 1. Otherwise, it is 0.
void CHIP8::CPU::XorReg(u8 x, u8 y) {
    v[x] ^= v[y];
}

// AddReg 8xy4 - ADD Vx, Vy
// Set Vx = Vx + Vy, set VF = carry. The values of Vx and Vy are added together. If the result is greater
// than 8 bits (i.e., ¿ 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored
// in Vx.
void CHIP8::CPU::AddReg(u8 x, u8 y) {
    v[0xF] = v[x] > 0xff - v[y] ? 1 : 0;
    v[x] += v[y];
}

// SubReg 8xy5 - SUB Vx, Vy
// Set Vx = Vx - Vy, set VF = NOT borrow. If Vx ¿ Vy, then VF is set to 1, otherwise 0. Then Vy is
// subtracted from Vx, and the results stored in Vx.
void CHIP8::CPU::SubReg(u8 x, u8 y) {
    v[0xF] = v[x] > v[y] ? 1 : 0;
    v[x] -= v[y];
}

// Shr 8xy6 - SHR Vx {, Vy}
// Set Vx = Vx SHR 1. If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is
// divided by 2.
void CHIP8::CPU::Shr(u8 x, u8 y) {
    v[0xf] = v[x] & 1;
    v[x] >>= 1;
}

// SubReverse 8xy7 - SUBN Vx, Vy
// Set Vx = Vy - Vx, set VF = NOT borrow. If Vy ¿ Vx, then VF is set to 1, otherwise 0. Then Vx is
// subtracted from Vy, and the results stored in Vx.
void CHIP8::CPU::SubReverse(u8 x, u8 y) {
    v[0xF] = v[y] > v[x] ? 1 : 0;
    v[x] = v[y] - v[x];
}

// Shl 8xyE - SHL Vx {, Vy}
// Set Vx = Vx SHL 1. If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is
// multiplied by 2.
void CHIP8::CPU::Shl(u8 x, u8 y) {
    v[0xF] = v[x] >> 7;
    v[x] <<= 1;
}

// SkipIfNotEqualReg 9xy0 - SNE Vx, Vy
// Skip next instruction if Vx != Vy. The values of Vx and Vy are compared, and if they are not equal, the
// program counter is increased by 2.
void CHIP8::CPU::SkipIfNotEqualReg(u8 x, u8 y) {
    if (v[x] != v[y]) {
        pc += 2;
    }
}

// LoadIndexImm Annn - LD I, addr
// Set I = nnn. The value of register I is set to nnn.
void CHIP8::CPU::LoadIndexImm(u16 imm) {
    idx = imm;
}

// JumpRelV0 Bnnn - JP V0, addr
// Jump to location nnn + V0. The program counter is set to nnn plus the value of V0.
void CHIP8::CPU::JumpRelV0(u16 imm) {
    pc = u16(v[0]) + imm;
}

// RandReg Cxkk - RND Vx, byte
// Set Vx = random byte AND kk. The interpreter generates a random number from 0 to 255, which is then
// ANDed with the value kk. The results are stored in Vx. See instruction 8xy2 for more information on AND.
void CHIP8::CPU::RandReg(u8 x, u8 imm) {
    v[x] = rand() & imm;
}

// Draw Dxyn - DRW Vx, Vy, nibble
// Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision. The interpreter reads n
// bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen
// at coordinates (Vx, Vy). Sprites are XOR’d onto the existing screen. If this causes any pixels to be erased,
// VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside the coordinates of
// the display, it wraps around to the opposite side of the screen.
void CHIP8::CPU::Draw(u8 x, u8 y, u8 nibble) {
    u8 pixelFlipped = 0;
    u8* frameBuffer = bus->frameBuffer;

    u64 vx = u64(v[x]);
    u64 vy = u64(v[y]);

    for (int i = 0; i < nibble; i++) {
        u8 pixelLine = bus->ram[idx + i];
        for (int j = 0; j < 8; j++) {
            // if not overflow
            if (vx + j < SCR_WEIGHT && vy + i < SCR_HEIGHT) {
                u8 pixel = (pixelLine >> (7 - j)) & 1;
                auto pixelIndex = (vy + i) * SCR_WEIGHT + (vx + j);
                if (pixel && frameBuffer[pixelIndex]) {
                    pixelFlipped = 1;
                }
                frameBuffer[pixelIndex] ^= pixel;
            }
        }
    }

    v[0xF] = pixelFlipped & 1;
}

// SkipIfPressed Ex9E - SKP Vx
// Skip next instruction if key with the value of Vx is pressed. Checks the keyboard, and if the key corresponding
// to the value of Vx is currently in the down position, PC is increased by 2.
void CHIP8::CPU::SkipIfPressed(u8 x) {
    if (bus->k[v[x]]) {
        pc += 2;
    }
}

// SkipIfNotPressed ExA1 - SKNP Vx
// Skip next instruction if key with the value of Vx is not pressed. Checks the keyboard, and if the key
// corresponding to the value of Vx is currently in the up position, PC is increased by 2.
void CHIP8::CPU::SkipIfNotPressed(u8 x) {
    if (!bus->k[v[x]]) {
        pc += 2;
    }
}

// LoadDT Fx07 - LD Vx, DT
// Set Vx = delay timer value. The value of DT is placed into Vx.
void CHIP8::CPU::LoadDT(u8 x) {
    v[x] = GetTrueDT();
}

// WaitForKey Fx0A - LD Vx, K
// Wait for a key press, store the value of the key in Vx. All execution stops until a key is pressed, then the
// value of that key is stored in Vx.
void CHIP8::CPU::WaitForKey(u8 x) {
    waitKey = &v[x];
}

// SetDtReg Fx15 - LD DT, Vx
// Set delay timer = Vx. Delay Timer is set equal to the value of Vx.
void CHIP8::CPU::SetDTReg(u8 x) {
    fakeDT = GetFakeTimer(v[x]);
}

// SetStReg Fx18 - LD ST, Vx
// Set sound timer = Vx. Sound Timer is set equal to the value of Vx.
void CHIP8::CPU::SetSTReg(u8 x) {
    fakeST = GetFakeTimer(v[x]);
}

// AddIndexReg Fx1E - ADD I, Vx
// Set I = I + Vx. The values of I and Vx are added, and the results are stored in I.
void CHIP8::CPU::AddIndexReg(u8 x) {
    idx += u16(v[x]);
}

// LoadFontReg Fx29 - LD F, Vx
// Set I = location of sprite for digit Vx. The value of I is set to the location for the hexadecimal sprite
// corresponding to the value of Vx. See section 2.4, Display, for more information on the Chip-8 hexadecimal
// font. To obtain this value, multiply VX by 5 (all font data stored in first 80 bytes of memory).
void CHIP8::CPU::LoadFontReg(u8 x) {
    idx = u16(v[x] * 5);
}

// StoreBCD Fx33 - LD B, Vx
// Store BCD representation of Vx in memory locations I, I+1, and I+2. The interpreter takes the decimal
// value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and
// the ones digit at location I+2.
void CHIP8::CPU::StoreBCD(u8 x) {
    u32 vx = u32(v[x]);
    bus->ram[idx + 0] = u8((vx / 100) % 10);
    bus->ram[idx + 1] = u8((vx / 10) % 10);
    bus->ram[idx + 2] = u8(vx % 10);
}

// StoreReg Fx55 - LD [I], Vx
// Stores V0 to VX in memory starting at address I. I is then set to I + x + 1.
void CHIP8::CPU::StoreReg(u8 x) {
    for (int i = 0; i <= x; i++) {
        bus->ram[u32(idx) + i] = v[i];
    }
}

// LoadReg Fx65 - LD Vx, [I]
// Fills V0 to VX with values from memory starting at address I. I is then set to I + x + 1.
void CHIP8::CPU::LoadReg(u8 x) {
    for (int i = 0; i <= x; i++) {
        v[i] = bus->ram[u32(idx) + i];
    }
}

u8 CHIP8::CPU::GetTrueDT() {
    u64 now = GetNanoTimestamp();
    if (now < fakeDT) {
        return u8(double(fakeDT - now) / 1000000000.0l * 60.0l);
    }
    return 0;
}

u8 CHIP8::CPU::GetTrueST() {
    u64 now = GetNanoTimestamp();
    if (now < fakeST) {
        return u8(double(fakeST - now) / 1000000000.0l * 60.0l);
    }
    return 0;
}

u64 CHIP8::CPU::GetFakeTimer(u8 val) {
    u64 now = GetNanoTimestamp();
    return now + u64(val) * 1000000000l / 60;
}

u64 CHIP8::CPU::GetFakeST() {
    return fakeST;
}
