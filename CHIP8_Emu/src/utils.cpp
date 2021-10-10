#include "utils.hpp"
#include <chrono>

using namespace std::chrono;

u64 GetNanoTimestamp() {
    nanoseconds ns = duration_cast<nanoseconds>(
        system_clock::now().time_since_epoch());
    return u64(ns.count());
}
