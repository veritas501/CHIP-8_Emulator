#include "bus.hpp"
#include <cstring>

using namespace CHIP8;

void BUS::Reset() {
    // reset key status
    for (int i = 0; i < sizeof(k); i++) {
        k[i] = false;
    }

    // clean framebuffer
    memset(frameBuffer, 0, sizeof(frameBuffer));

    // TODO maybe we need clean ram too?
}


// KeyIsPressed check if the key is pressed
bool BUS::KeyIsPressed(u8 index) {
    if (index < sizeof(k)) {
        return k[index];
    }
    return false;
}
