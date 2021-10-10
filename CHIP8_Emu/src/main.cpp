#include "chip8.hpp"
#include "hal.hpp"
#include "utils.hpp"
#include <iostream>
#include <cstring>
#include <SDL.h>

#define VERSION "v1.0.1"

using namespace std;
using namespace CHIP8;

Chip8* vm = nullptr;
SDL_Window* window = nullptr;
SDL_Renderer* renderer = nullptr;
SDL_Texture* texture = nullptr;
SDL_AudioDeviceID audio_device = 0;
SDL_AudioSpec audio_spec;

void CreateWindowAndRenderer() {
    // create window
    window = SDL_CreateWindow(
        "CHIP-8 Emulator " VERSION,
        SDL_WINDOWPOS_CENTERED,
        SDL_WINDOWPOS_CENTERED,
        SCR_WEIGHT * HI_RES,
        SCR_HEIGHT * HI_RES,
        SDL_WINDOW_OPENGL | SDL_WINDOW_RESIZABLE
    );
    if (!window) {
        cerr << "Call SDL_CreateWindow failed" << endl;
        exit(1);
    }


    // create renderer
    renderer = SDL_CreateRenderer(window, -1,
        SDL_RENDERER_PRESENTVSYNC);
    if (!renderer) {
        cerr << "Call SDL_CreateRenderer failed" << endl;
        exit(1);
    }


    // create texture for game
    texture = SDL_CreateTexture(
        renderer,
        SDL_PIXELFORMAT_RGBA32,
        SDL_TEXTUREACCESS_TARGET,
        SCR_WEIGHT, SCR_HEIGHT
    );
    if (!texture) {
        cerr << "Call SDL_CreateTexture failed" << endl;
        exit(1);
    }
}

bool ProcessEvents() {
    SDL_Event sdl_event{};
    while (SDL_PollEvent(&sdl_event) != NULL) {
        switch (sdl_event.type) {
        case SDL_QUIT: {
            return false;
        }
        case SDL_DROPFILE: {
            if (sdl_event.drop.file) {
                vm->LoadGame(sdl_event.drop.file);
                SDL_free(sdl_event.drop.file);
            }
            break;
        }
        case SDL_KEYDOWN:
        case SDL_KEYUP: {
            auto scanCode = sdl_event.key.keysym.scancode;
            auto it = keyMap.find(scanCode);
            if (it != keyMap.end()) {
                int keyIndex = it->second;
                if (sdl_event.type == SDL_KEYUP) {
                    vm->ReleaseKey(keyIndex);
                }
                else {
                    vm->PressKey(keyIndex);
                }
            }

            // ESC for reset
            if (sdl_event.type == SDL_KEYUP &&
                sdl_event.key.keysym.scancode == SDL_SCANCODE_ESCAPE) {
                vm->Reset();
            }
            break;
        }
        default:
            break;
        }
    }
    return true;
}

void Render() {
    if (SDL_SetRenderTarget(renderer, texture)) {
        cerr << "Call SDL_SetRenderTarget failed" << endl;
        exit(1);
    }

    // clean render with background color
    SDL_SetRenderDrawColor(
        renderer,
        PIXEL_WHITE[0],
        PIXEL_WHITE[1],
        PIXEL_WHITE[2],
        SDL_ALPHA_OPAQUE);
    SDL_RenderClear(renderer);

    // draw pixels
    SDL_SetRenderDrawColor(
        renderer,
        PIXEL_BLACK[0],
        PIXEL_BLACK[1],
        PIXEL_BLACK[2],
        SDL_ALPHA_OPAQUE);
    for (int i = 0; i < SCR_WEIGHT; i++) {
        for (int j = 0; j < SCR_HEIGHT; j++) {
            if (vm->GetPixel(i, j)) {
                SDL_RenderDrawPoint(renderer, i, j);
            }
        }
    }

    SDL_SetRenderTarget(renderer, NULL);

    SDL_Rect dst = { 0, 0, SCR_WEIGHT, SCR_HEIGHT };
    int winW, winH;
    SDL_GetWindowSize(window, &winW, &winH);
    SDL_Rect src = { 0, 0, winW, winH };

    // copy texture to renderer
    if (SDL_RenderCopy(renderer, texture, &dst, &src)) {
        cerr << "Call SDL_RenderCopy failed" << endl;
        exit(1);
    }

    // present
    SDL_RenderPresent(renderer);
}

float sound_x = 0;

void SDLCALL FillAudio(void* unused, Uint8* stream, int len) {
    if (vm && GetNanoTimestamp() < vm->GetFakeST()) {
        for (int i = 0; i < len / sizeof(float); i++) {
            sound_x += 0.01f;

            auto approx_sin = [](float t) {
                float j = t * 0.15915f;
                j = j - (int)j;
                return 20.785f * j * (j - 0.5f) * (j - 1.0f);
            };

            float sample = approx_sin(sound_x * 15) * 0.3f;
            *(float*)(stream + i * sizeof(float)) = sample;
        }
    }
    else {
        memset(stream, 0, len);
        sound_x = 0;
    }
}

void CreateAudio() {
    SDL_zero(audio_spec);
    audio_spec.freq = 44100;
    audio_spec.format = AUDIO_F32SYS;
    audio_spec.channels = 1;
    audio_spec.samples = 32;
    audio_spec.callback = FillAudio;
    audio_device = SDL_OpenAudioDevice(NULL, 0, &audio_spec, NULL, 0);
    if (!audio_device) {
        cerr << "Call SDL_OpenAudioDevice failed" << endl;
        exit(1);
    }
    // open audio device
    SDL_PauseAudioDevice(audio_device, SDL_FALSE);
}

#ifdef __cplusplus
extern "C"
#endif
int main(int argc, char* argv[]) {
    if (SDL_Init(SDL_INIT_EVERYTHING)) {
        cerr << "Call SDL_Init failed" << endl;
        cerr << SDL_GetError() << endl;
        exit(1);
    }
    CreateWindowAndRenderer();
    CreateAudio();

    vm = new Chip8();

    while (ProcessEvents()) {
        vm->Process();
        Render();
    }

    return 0;
}
