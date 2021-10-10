#pragma once
#include "constants.hpp"
#include "types.hpp"
#include "bus.hpp"
#include <cstdint>

namespace CHIP8 {
    class CPU {
    private:
        ////////////////// registers 

        u16 pc = ROM_OEP; // pc is the pointer counter, 2 bytes.
        u16 sp = 0; // sp is the stack pointer, 2 bytes.
        u8 v[0x10] = { 0 }; // V (V0~VF) are the 16 common registers, each 1 byte.
        u16 idx = 0; // idx is the index register, 2 bytes.

        u64 fakeST = 0; // fake sound timer, origin sound timer register is 1 byte.
        u64 fakeDT = 0; // fake delay timer.

        u8* waitKey = nullptr;

        u16 stack[CHIP8_STACK_SIZE / 2] = { 0 };

        u64 instPerSecond = CHIP8_CPU_SPEED / CHIP8_CYCLE_PRE_INST;

        // cpu is connected to the bus
        BUS* bus = nullptr;

        ////////////////// insts

        void Syscall();
        void Cls();
        void Return();
        void JumpImm(u16 p);
        void CallImm(u16 p);
        void SkipIfEqualImm(u8 x, u8 imm);
        void SkipIfNotEqualImm(u8 x, u8 imm);
        void SkipIfEqualReg(u8 x, u8 y);
        void MovImm(u8 x, u8 imm);
        void AddImm(u8 x, u8 imm);
        void MovReg(u8 x, u8 y);
        void OrReg(u8 x, u8 y);
        void AndReg(u8 x, u8 y);
        void XorReg(u8 x, u8 y);
        void AddReg(u8 x, u8 y);
        void SubReg(u8 x, u8 y);
        void Shr(u8 x, u8 y);
        void SubReverse(u8 x, u8 y);
        void Shl(u8 x, u8 y);
        void SkipIfNotEqualReg(u8 x, u8 y);
        void LoadIndexImm(u16 imm);
        void JumpRelV0(u16 imm);
        void RandReg(u8 x, u8 imm);
        void Draw(u8 x, u8 y, u8 nibble);
        void SkipIfPressed(u8 x);
        void SkipIfNotPressed(u8 x);
        void LoadDT(u8 x);
        void WaitForKey(u8 x);
        void SetDTReg(u8 x);
        void SetSTReg(u8 x);
        void AddIndexReg(u8 x);
        void LoadFontReg(u8 x);
        void StoreBCD(u8 x);
        void StoreReg(u8 x);
        void LoadReg(u8 x);

        u8 GetTrueDT();
        u8 GetTrueST();
        u64 GetFakeTimer(u8 val);

    public:
        CPU();
        ~CPU();

        void InitBus(BUS* bus);
        void Reset();
        void StepOver();
        void Dispatch(u16 inst);

        u64 GetInstPerSecond();
        void SetInstPerSecond(u64 imm);
        bool IsWaitingKey();
        void UpdateWaitKey(u8 key);
        u64 GetFakeST();
    };
}
