#pragma once
#ifdef _WIN64
#include <windows.h>

// Alphabet keys A-Z (ASCII: 0x41–0x5A)
enum class KeyAlpha : u32 {
    A = 'A',
    B = 'B',
    C = 'C',
    D = 'D',
    E = 'E',
    F = 'F',
    G = 'G',
    H = 'H',
    I = 'I',
    J = 'J',
    K = 'K',
    L = 'L',
    M = 'M',
    N = 'N',
    O = 'O',
    P = 'P',
    Q = 'Q',
    R = 'R',
    S = 'S',
    T = 'T',
    U = 'U',
    V = 'V',
    W = 'W',
    X = 'X',
    Y = 'Y',
    Z = 'Z'
};

// Number keys 0-9 (ASCII: 0x30–0x39)
enum class KeyNumber : u32 {
    Num0 = '0',
    Num1 = '1',
    Num2 = '2',
    Num3 = '3',
    Num4 = '4',
    Num5 = '5',
    Num6 = '6',
    Num7 = '7',
    Num8 = '8',
    Num9 = '9'
};


// Numpad keys
enum class KeyNumpad : u32 {
    Numpad0 = VK_NUMPAD0,
    Numpad1 = VK_NUMPAD1,
    Numpad2 = VK_NUMPAD2,
    Numpad3 = VK_NUMPAD3,
    Numpad4 = VK_NUMPAD4,
    Numpad5 = VK_NUMPAD5,
    Numpad6 = VK_NUMPAD6,
    Numpad7 = VK_NUMPAD7,
    Numpad8 = VK_NUMPAD8,
    Numpad9 = VK_NUMPAD9,
    Multiply = VK_MULTIPLY,
    Add = VK_ADD,
    Separator = VK_SEPARATOR,
    Subtract = VK_SUBTRACT,
    Decimal = VK_DECIMAL,
    Divide = VK_DIVIDE
};

// Function keys
enum class KeyFunction : u32 {
    F1 = VK_F1,
    F2 = VK_F2,
    F3 = VK_F3,
    F4 = VK_F4,
    F5 = VK_F5,
    F6 = VK_F6,
    F7 = VK_F7,
    F8 = VK_F8,
    F9 = VK_F9,
    F10 = VK_F10,
    F11 = VK_F11,
    F12 = VK_F12,
    F13 = VK_F13,
    F14 = VK_F14,
    F15 = VK_F15,
    F16 = VK_F16,
    F17 = VK_F17,
    F18 = VK_F18,
    F19 = VK_F19,
    F20 = VK_F20,
    F21 = VK_F21,
    F22 = VK_F22,
    F23 = VK_F23,
    F24 = VK_F24
};

// Control keys
enum class KeyControl : u32 {
    Backspace = VK_BACK,
    Tab = VK_TAB,
    Enter = VK_RETURN,
    Shift = VK_SHIFT,
    Ctrl = VK_CONTROL,
    Alt = VK_MENU,
    Pause = VK_PAUSE,
    CapsLock = VK_CAPITAL,
    Esc = VK_ESCAPE,
    Space = VK_SPACE,
    Insert = VK_INSERT,
    Delete = VK_DELETE,
    Home = VK_HOME,
    End = VK_END,
    PageUp = VK_PRIOR,
    PageDown = VK_NEXT,
    Left = VK_LEFT,
    Right = VK_RIGHT,
    Up = VK_UP,
    Down = VK_DOWN,
    NumLock = VK_NUMLOCK,
    ScrollLock = VK_SCROLL
};

// Mouse buttons
enum class MouseButton : u32 {
    Left = VK_LBUTTON,
    Right = VK_RBUTTON,
    Middle = VK_MBUTTON,
    XButton1 = VK_XBUTTON1,
    XButton2 = VK_XBUTTON2
};
#endif