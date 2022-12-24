﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Hotkeys;

namespace ClassicAssist.Misc
{
    public static class SDLKeys
    {
        /* Key modifiers (bitfield) available for hot keys */
        [Flags]
        public enum ModKey : ushort
        {
            None = SDL_Keymod.KMOD_NONE,
            LeftShift = SDL_Keymod.KMOD_LSHIFT,
            RightShift = SDL_Keymod.KMOD_RSHIFT,
            LeftCtrl = SDL_Keymod.KMOD_LCTRL,
            RightCtrl = SDL_Keymod.KMOD_RCTRL,
            LeftAlt = SDL_Keymod.KMOD_LALT,
            RightAlt = SDL_Keymod.KMOD_RALT
        }

        public enum SDL_Keycode
        {
            SDLK_UNKNOWN = 0,

            SDLK_RETURN = '\r',
            SDLK_ESCAPE = 27, // '\033'
            SDLK_BACKSPACE = '\b',
            SDLK_TAB = '\t',
            SDLK_SPACE = ' ',
            SDLK_EXCLAIM = '!',
            SDLK_QUOTEDBL = '"',
            SDLK_HASH = '#',
            SDLK_PERCENT = '%',
            SDLK_DOLLAR = '$',
            SDLK_AMPERSAND = '&',
            SDLK_QUOTE = '\'',
            SDLK_LEFTPAREN = '(',
            SDLK_RIGHTPAREN = ')',
            SDLK_ASTERISK = '*',
            SDLK_PLUS = '+',
            SDLK_COMMA = ',',
            SDLK_MINUS = '-',
            SDLK_PERIOD = '.',
            SDLK_SLASH = '/',
            SDLK_0 = '0',
            SDLK_1 = '1',
            SDLK_2 = '2',
            SDLK_3 = '3',
            SDLK_4 = '4',
            SDLK_5 = '5',
            SDLK_6 = '6',
            SDLK_7 = '7',
            SDLK_8 = '8',
            SDLK_9 = '9',
            SDLK_COLON = ':',
            SDLK_SEMICOLON = ';',
            SDLK_LESS = '<',
            SDLK_EQUALS = '=',
            SDLK_GREATER = '>',
            SDLK_QUESTION = '?',
            SDLK_AT = '@',

            /*
			Skip uppercase letters
			*/
            SDLK_LEFTBRACKET = '[',
            SDLK_BACKSLASH = '\\',
            SDLK_RIGHTBRACKET = ']',
            SDLK_CARET = '^',
            SDLK_UNDERSCORE = '_',
            SDLK_BACKQUOTE = '`',
            SDLK_a = 'a',
            SDLK_b = 'b',
            SDLK_c = 'c',
            SDLK_d = 'd',
            SDLK_e = 'e',
            SDLK_f = 'f',
            SDLK_g = 'g',
            SDLK_h = 'h',
            SDLK_i = 'i',
            SDLK_j = 'j',
            SDLK_k = 'k',
            SDLK_l = 'l',
            SDLK_m = 'm',
            SDLK_n = 'n',
            SDLK_o = 'o',
            SDLK_p = 'p',
            SDLK_q = 'q',
            SDLK_r = 'r',
            SDLK_s = 's',
            SDLK_t = 't',
            SDLK_u = 'u',
            SDLK_v = 'v',
            SDLK_w = 'w',
            SDLK_x = 'x',
            SDLK_y = 'y',
            SDLK_z = 'z',
            SDLK_æ = 'æ',
            SDLK_ø = 'ø',
            SDLK_å = 'å',

            SDLK_CAPSLOCK = SDL_Scancode.SDL_SCANCODE_CAPSLOCK | SDLK_SCANCODE_MASK,

            SDLK_F1 = SDL_Scancode.SDL_SCANCODE_F1 | SDLK_SCANCODE_MASK,
            SDLK_F2 = SDL_Scancode.SDL_SCANCODE_F2 | SDLK_SCANCODE_MASK,
            SDLK_F3 = SDL_Scancode.SDL_SCANCODE_F3 | SDLK_SCANCODE_MASK,
            SDLK_F4 = SDL_Scancode.SDL_SCANCODE_F4 | SDLK_SCANCODE_MASK,
            SDLK_F5 = SDL_Scancode.SDL_SCANCODE_F5 | SDLK_SCANCODE_MASK,
            SDLK_F6 = SDL_Scancode.SDL_SCANCODE_F6 | SDLK_SCANCODE_MASK,
            SDLK_F7 = SDL_Scancode.SDL_SCANCODE_F7 | SDLK_SCANCODE_MASK,
            SDLK_F8 = SDL_Scancode.SDL_SCANCODE_F8 | SDLK_SCANCODE_MASK,
            SDLK_F9 = SDL_Scancode.SDL_SCANCODE_F9 | SDLK_SCANCODE_MASK,
            SDLK_F10 = SDL_Scancode.SDL_SCANCODE_F10 | SDLK_SCANCODE_MASK,
            SDLK_F11 = SDL_Scancode.SDL_SCANCODE_F11 | SDLK_SCANCODE_MASK,
            SDLK_F12 = SDL_Scancode.SDL_SCANCODE_F12 | SDLK_SCANCODE_MASK,

            SDLK_PRINTSCREEN = SDL_Scancode.SDL_SCANCODE_PRINTSCREEN | SDLK_SCANCODE_MASK,
            SDLK_SCROLLLOCK = SDL_Scancode.SDL_SCANCODE_SCROLLLOCK | SDLK_SCANCODE_MASK,
            SDLK_PAUSE = SDL_Scancode.SDL_SCANCODE_PAUSE | SDLK_SCANCODE_MASK,
            SDLK_INSERT = SDL_Scancode.SDL_SCANCODE_INSERT | SDLK_SCANCODE_MASK,
            SDLK_HOME = SDL_Scancode.SDL_SCANCODE_HOME | SDLK_SCANCODE_MASK,
            SDLK_PAGEUP = SDL_Scancode.SDL_SCANCODE_PAGEUP | SDLK_SCANCODE_MASK,
            SDLK_DELETE = 127,
            SDLK_END = SDL_Scancode.SDL_SCANCODE_END | SDLK_SCANCODE_MASK,
            SDLK_PAGEDOWN = SDL_Scancode.SDL_SCANCODE_PAGEDOWN | SDLK_SCANCODE_MASK,
            SDLK_RIGHT = SDL_Scancode.SDL_SCANCODE_RIGHT | SDLK_SCANCODE_MASK,
            SDLK_LEFT = SDL_Scancode.SDL_SCANCODE_LEFT | SDLK_SCANCODE_MASK,
            SDLK_DOWN = SDL_Scancode.SDL_SCANCODE_DOWN | SDLK_SCANCODE_MASK,
            SDLK_UP = SDL_Scancode.SDL_SCANCODE_UP | SDLK_SCANCODE_MASK,

            SDLK_NUMLOCKCLEAR = SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_DIVIDE = SDL_Scancode.SDL_SCANCODE_KP_DIVIDE | SDLK_SCANCODE_MASK,
            SDLK_KP_MULTIPLY = SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY | SDLK_SCANCODE_MASK,
            SDLK_KP_MINUS = SDL_Scancode.SDL_SCANCODE_KP_MINUS | SDLK_SCANCODE_MASK,
            SDLK_KP_PLUS = SDL_Scancode.SDL_SCANCODE_KP_PLUS | SDLK_SCANCODE_MASK,
            SDLK_KP_ENTER = SDL_Scancode.SDL_SCANCODE_KP_ENTER | SDLK_SCANCODE_MASK,
            SDLK_KP_1 = SDL_Scancode.SDL_SCANCODE_KP_1 | SDLK_SCANCODE_MASK,
            SDLK_KP_2 = SDL_Scancode.SDL_SCANCODE_KP_2 | SDLK_SCANCODE_MASK,
            SDLK_KP_3 = SDL_Scancode.SDL_SCANCODE_KP_3 | SDLK_SCANCODE_MASK,
            SDLK_KP_4 = SDL_Scancode.SDL_SCANCODE_KP_4 | SDLK_SCANCODE_MASK,
            SDLK_KP_5 = SDL_Scancode.SDL_SCANCODE_KP_5 | SDLK_SCANCODE_MASK,
            SDLK_KP_6 = SDL_Scancode.SDL_SCANCODE_KP_6 | SDLK_SCANCODE_MASK,
            SDLK_KP_7 = SDL_Scancode.SDL_SCANCODE_KP_7 | SDLK_SCANCODE_MASK,
            SDLK_KP_8 = SDL_Scancode.SDL_SCANCODE_KP_8 | SDLK_SCANCODE_MASK,
            SDLK_KP_9 = SDL_Scancode.SDL_SCANCODE_KP_9 | SDLK_SCANCODE_MASK,
            SDLK_KP_0 = SDL_Scancode.SDL_SCANCODE_KP_0 | SDLK_SCANCODE_MASK,
            SDLK_KP_PERIOD = SDL_Scancode.SDL_SCANCODE_KP_PERIOD | SDLK_SCANCODE_MASK,

            SDLK_APPLICATION = SDL_Scancode.SDL_SCANCODE_APPLICATION | SDLK_SCANCODE_MASK,
            SDLK_POWER = SDL_Scancode.SDL_SCANCODE_POWER | SDLK_SCANCODE_MASK,
            SDLK_KP_EQUALS = SDL_Scancode.SDL_SCANCODE_KP_EQUALS | SDLK_SCANCODE_MASK,
            SDLK_F13 = SDL_Scancode.SDL_SCANCODE_F13 | SDLK_SCANCODE_MASK,
            SDLK_F14 = SDL_Scancode.SDL_SCANCODE_F14 | SDLK_SCANCODE_MASK,
            SDLK_F15 = SDL_Scancode.SDL_SCANCODE_F15 | SDLK_SCANCODE_MASK,
            SDLK_F16 = SDL_Scancode.SDL_SCANCODE_F16 | SDLK_SCANCODE_MASK,
            SDLK_F17 = SDL_Scancode.SDL_SCANCODE_F17 | SDLK_SCANCODE_MASK,
            SDLK_F18 = SDL_Scancode.SDL_SCANCODE_F18 | SDLK_SCANCODE_MASK,
            SDLK_F19 = SDL_Scancode.SDL_SCANCODE_F19 | SDLK_SCANCODE_MASK,
            SDLK_F20 = SDL_Scancode.SDL_SCANCODE_F20 | SDLK_SCANCODE_MASK,
            SDLK_F21 = SDL_Scancode.SDL_SCANCODE_F21 | SDLK_SCANCODE_MASK,
            SDLK_F22 = SDL_Scancode.SDL_SCANCODE_F22 | SDLK_SCANCODE_MASK,
            SDLK_F23 = SDL_Scancode.SDL_SCANCODE_F23 | SDLK_SCANCODE_MASK,
            SDLK_F24 = SDL_Scancode.SDL_SCANCODE_F24 | SDLK_SCANCODE_MASK,
            SDLK_EXECUTE = SDL_Scancode.SDL_SCANCODE_EXECUTE | SDLK_SCANCODE_MASK,
            SDLK_HELP = SDL_Scancode.SDL_SCANCODE_HELP | SDLK_SCANCODE_MASK,
            SDLK_MENU = SDL_Scancode.SDL_SCANCODE_MENU | SDLK_SCANCODE_MASK,
            SDLK_SELECT = SDL_Scancode.SDL_SCANCODE_SELECT | SDLK_SCANCODE_MASK,
            SDLK_STOP = SDL_Scancode.SDL_SCANCODE_STOP | SDLK_SCANCODE_MASK,
            SDLK_AGAIN = SDL_Scancode.SDL_SCANCODE_AGAIN | SDLK_SCANCODE_MASK,
            SDLK_UNDO = SDL_Scancode.SDL_SCANCODE_UNDO | SDLK_SCANCODE_MASK,
            SDLK_CUT = SDL_Scancode.SDL_SCANCODE_CUT | SDLK_SCANCODE_MASK,
            SDLK_COPY = SDL_Scancode.SDL_SCANCODE_COPY | SDLK_SCANCODE_MASK,
            SDLK_PASTE = SDL_Scancode.SDL_SCANCODE_PASTE | SDLK_SCANCODE_MASK,
            SDLK_FIND = SDL_Scancode.SDL_SCANCODE_FIND | SDLK_SCANCODE_MASK,
            SDLK_MUTE = SDL_Scancode.SDL_SCANCODE_MUTE | SDLK_SCANCODE_MASK,
            SDLK_VOLUMEUP = SDL_Scancode.SDL_SCANCODE_VOLUMEUP | SDLK_SCANCODE_MASK,
            SDLK_VOLUMEDOWN = SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN | SDLK_SCANCODE_MASK,
            SDLK_KP_COMMA = SDL_Scancode.SDL_SCANCODE_KP_COMMA | SDLK_SCANCODE_MASK,

            SDLK_KP_EQUALSAS400 = SDL_Scancode.SDL_SCANCODE_KP_EQUALSAS400 | SDLK_SCANCODE_MASK,

            SDLK_ALTERASE = SDL_Scancode.SDL_SCANCODE_ALTERASE | SDLK_SCANCODE_MASK,
            SDLK_SYSREQ = SDL_Scancode.SDL_SCANCODE_SYSREQ | SDLK_SCANCODE_MASK,
            SDLK_CANCEL = SDL_Scancode.SDL_SCANCODE_CANCEL | SDLK_SCANCODE_MASK,
            SDLK_CLEAR = SDL_Scancode.SDL_SCANCODE_CLEAR | SDLK_SCANCODE_MASK,
            SDLK_PRIOR = SDL_Scancode.SDL_SCANCODE_PRIOR | SDLK_SCANCODE_MASK,
            SDLK_RETURN2 = SDL_Scancode.SDL_SCANCODE_RETURN2 | SDLK_SCANCODE_MASK,
            SDLK_SEPARATOR = SDL_Scancode.SDL_SCANCODE_SEPARATOR | SDLK_SCANCODE_MASK,
            SDLK_OUT = SDL_Scancode.SDL_SCANCODE_OUT | SDLK_SCANCODE_MASK,
            SDLK_OPER = SDL_Scancode.SDL_SCANCODE_OPER | SDLK_SCANCODE_MASK,
            SDLK_CLEARAGAIN = SDL_Scancode.SDL_SCANCODE_CLEARAGAIN | SDLK_SCANCODE_MASK,
            SDLK_CRSEL = SDL_Scancode.SDL_SCANCODE_CRSEL | SDLK_SCANCODE_MASK,
            SDLK_EXSEL = SDL_Scancode.SDL_SCANCODE_EXSEL | SDLK_SCANCODE_MASK,

            SDLK_KP_00 = SDL_Scancode.SDL_SCANCODE_KP_00 | SDLK_SCANCODE_MASK,
            SDLK_KP_000 = SDL_Scancode.SDL_SCANCODE_KP_000 | SDLK_SCANCODE_MASK,

            SDLK_THOUSANDSSEPARATOR = SDL_Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR | SDLK_SCANCODE_MASK,

            SDLK_DECIMALSEPARATOR = SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR | SDLK_SCANCODE_MASK,
            SDLK_CURRENCYUNIT = SDL_Scancode.SDL_SCANCODE_CURRENCYUNIT | SDLK_SCANCODE_MASK,

            SDLK_CURRENCYSUBUNIT = SDL_Scancode.SDL_SCANCODE_CURRENCYSUBUNIT | SDLK_SCANCODE_MASK,
            SDLK_KP_LEFTPAREN = SDL_Scancode.SDL_SCANCODE_KP_LEFTPAREN | SDLK_SCANCODE_MASK,
            SDLK_KP_RIGHTPAREN = SDL_Scancode.SDL_SCANCODE_KP_RIGHTPAREN | SDLK_SCANCODE_MASK,
            SDLK_KP_LEFTBRACE = SDL_Scancode.SDL_SCANCODE_KP_LEFTBRACE | SDLK_SCANCODE_MASK,
            SDLK_KP_RIGHTBRACE = SDL_Scancode.SDL_SCANCODE_KP_RIGHTBRACE | SDLK_SCANCODE_MASK,
            SDLK_KP_TAB = SDL_Scancode.SDL_SCANCODE_KP_TAB | SDLK_SCANCODE_MASK,
            SDLK_KP_BACKSPACE = SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE | SDLK_SCANCODE_MASK,
            SDLK_KP_A = SDL_Scancode.SDL_SCANCODE_KP_A | SDLK_SCANCODE_MASK,
            SDLK_KP_B = SDL_Scancode.SDL_SCANCODE_KP_B | SDLK_SCANCODE_MASK,
            SDLK_KP_C = SDL_Scancode.SDL_SCANCODE_KP_C | SDLK_SCANCODE_MASK,
            SDLK_KP_D = SDL_Scancode.SDL_SCANCODE_KP_D | SDLK_SCANCODE_MASK,
            SDLK_KP_E = SDL_Scancode.SDL_SCANCODE_KP_E | SDLK_SCANCODE_MASK,
            SDLK_KP_F = SDL_Scancode.SDL_SCANCODE_KP_F | SDLK_SCANCODE_MASK,
            SDLK_KP_XOR = SDL_Scancode.SDL_SCANCODE_KP_XOR | SDLK_SCANCODE_MASK,
            SDLK_KP_POWER = SDL_Scancode.SDL_SCANCODE_KP_POWER | SDLK_SCANCODE_MASK,
            SDLK_KP_PERCENT = SDL_Scancode.SDL_SCANCODE_KP_PERCENT | SDLK_SCANCODE_MASK,
            SDLK_KP_LESS = SDL_Scancode.SDL_SCANCODE_KP_LESS | SDLK_SCANCODE_MASK,
            SDLK_KP_GREATER = SDL_Scancode.SDL_SCANCODE_KP_GREATER | SDLK_SCANCODE_MASK,
            SDLK_KP_AMPERSAND = SDL_Scancode.SDL_SCANCODE_KP_AMPERSAND | SDLK_SCANCODE_MASK,

            SDLK_KP_DBLAMPERSAND = SDL_Scancode.SDL_SCANCODE_KP_DBLAMPERSAND | SDLK_SCANCODE_MASK,

            SDLK_KP_VERTICALBAR = SDL_Scancode.SDL_SCANCODE_KP_VERTICALBAR | SDLK_SCANCODE_MASK,

            SDLK_KP_DBLVERTICALBAR = SDL_Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR | SDLK_SCANCODE_MASK,
            SDLK_KP_COLON = SDL_Scancode.SDL_SCANCODE_KP_COLON | SDLK_SCANCODE_MASK,
            SDLK_KP_HASH = SDL_Scancode.SDL_SCANCODE_KP_HASH | SDLK_SCANCODE_MASK,
            SDLK_KP_SPACE = SDL_Scancode.SDL_SCANCODE_KP_SPACE | SDLK_SCANCODE_MASK,
            SDLK_KP_AT = SDL_Scancode.SDL_SCANCODE_KP_AT | SDLK_SCANCODE_MASK,
            SDLK_KP_EXCLAM = SDL_Scancode.SDL_SCANCODE_KP_EXCLAM | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMSTORE = SDL_Scancode.SDL_SCANCODE_KP_MEMSTORE | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMRECALL = SDL_Scancode.SDL_SCANCODE_KP_MEMRECALL | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMCLEAR = SDL_Scancode.SDL_SCANCODE_KP_MEMCLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMADD = SDL_Scancode.SDL_SCANCODE_KP_MEMADD | SDLK_SCANCODE_MASK,

            SDLK_KP_MEMSUBTRACT = SDL_Scancode.SDL_SCANCODE_KP_MEMSUBTRACT | SDLK_SCANCODE_MASK,

            SDLK_KP_MEMMULTIPLY = SDL_Scancode.SDL_SCANCODE_KP_MEMMULTIPLY | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMDIVIDE = SDL_Scancode.SDL_SCANCODE_KP_MEMDIVIDE | SDLK_SCANCODE_MASK,
            SDLK_KP_PLUSMINUS = SDL_Scancode.SDL_SCANCODE_KP_PLUSMINUS | SDLK_SCANCODE_MASK,
            SDLK_KP_CLEAR = SDL_Scancode.SDL_SCANCODE_KP_CLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_CLEARENTRY = SDL_Scancode.SDL_SCANCODE_KP_CLEARENTRY | SDLK_SCANCODE_MASK,
            SDLK_KP_BINARY = SDL_Scancode.SDL_SCANCODE_KP_BINARY | SDLK_SCANCODE_MASK,
            SDLK_KP_OCTAL = SDL_Scancode.SDL_SCANCODE_KP_OCTAL | SDLK_SCANCODE_MASK,
            SDLK_KP_DECIMAL = SDL_Scancode.SDL_SCANCODE_KP_DECIMAL | SDLK_SCANCODE_MASK,

            SDLK_KP_HEXADECIMAL = SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL | SDLK_SCANCODE_MASK,

            SDLK_LCTRL = SDL_Scancode.SDL_SCANCODE_LCTRL | SDLK_SCANCODE_MASK,
            SDLK_LSHIFT = SDL_Scancode.SDL_SCANCODE_LSHIFT | SDLK_SCANCODE_MASK,
            SDLK_LALT = SDL_Scancode.SDL_SCANCODE_LALT | SDLK_SCANCODE_MASK,
            SDLK_LGUI = SDL_Scancode.SDL_SCANCODE_LGUI | SDLK_SCANCODE_MASK,
            SDLK_RCTRL = SDL_Scancode.SDL_SCANCODE_RCTRL | SDLK_SCANCODE_MASK,
            SDLK_RSHIFT = SDL_Scancode.SDL_SCANCODE_RSHIFT | SDLK_SCANCODE_MASK,
            SDLK_RALT = SDL_Scancode.SDL_SCANCODE_RALT | SDLK_SCANCODE_MASK,
            SDLK_RGUI = SDL_Scancode.SDL_SCANCODE_RGUI | SDLK_SCANCODE_MASK,

            SDLK_MODE = SDL_Scancode.SDL_SCANCODE_MODE | SDLK_SCANCODE_MASK,

            SDLK_AUDIONEXT = SDL_Scancode.SDL_SCANCODE_AUDIONEXT | SDLK_SCANCODE_MASK,
            SDLK_AUDIOPREV = SDL_Scancode.SDL_SCANCODE_AUDIOPREV | SDLK_SCANCODE_MASK,
            SDLK_AUDIOSTOP = SDL_Scancode.SDL_SCANCODE_AUDIOSTOP | SDLK_SCANCODE_MASK,
            SDLK_AUDIOPLAY = SDL_Scancode.SDL_SCANCODE_AUDIOPLAY | SDLK_SCANCODE_MASK,
            SDLK_AUDIOMUTE = SDL_Scancode.SDL_SCANCODE_AUDIOMUTE | SDLK_SCANCODE_MASK,
            SDLK_MEDIASELECT = SDL_Scancode.SDL_SCANCODE_MEDIASELECT | SDLK_SCANCODE_MASK,
            SDLK_WWW = SDL_Scancode.SDL_SCANCODE_WWW | SDLK_SCANCODE_MASK,
            SDLK_MAIL = SDL_Scancode.SDL_SCANCODE_MAIL | SDLK_SCANCODE_MASK,
            SDLK_CALCULATOR = SDL_Scancode.SDL_SCANCODE_CALCULATOR | SDLK_SCANCODE_MASK,
            SDLK_COMPUTER = SDL_Scancode.SDL_SCANCODE_COMPUTER | SDLK_SCANCODE_MASK,
            SDLK_AC_SEARCH = SDL_Scancode.SDL_SCANCODE_AC_SEARCH | SDLK_SCANCODE_MASK,
            SDLK_AC_HOME = SDL_Scancode.SDL_SCANCODE_AC_HOME | SDLK_SCANCODE_MASK,
            SDLK_AC_BACK = SDL_Scancode.SDL_SCANCODE_AC_BACK | SDLK_SCANCODE_MASK,
            SDLK_AC_FORWARD = SDL_Scancode.SDL_SCANCODE_AC_FORWARD | SDLK_SCANCODE_MASK,
            SDLK_AC_STOP = SDL_Scancode.SDL_SCANCODE_AC_STOP | SDLK_SCANCODE_MASK,
            SDLK_AC_REFRESH = SDL_Scancode.SDL_SCANCODE_AC_REFRESH | SDLK_SCANCODE_MASK,
            SDLK_AC_BOOKMARKS = SDL_Scancode.SDL_SCANCODE_AC_BOOKMARKS | SDLK_SCANCODE_MASK,

            SDLK_BRIGHTNESSDOWN = SDL_Scancode.SDL_SCANCODE_BRIGHTNESSDOWN | SDLK_SCANCODE_MASK,
            SDLK_BRIGHTNESSUP = SDL_Scancode.SDL_SCANCODE_BRIGHTNESSUP | SDLK_SCANCODE_MASK,
            SDLK_DISPLAYSWITCH = SDL_Scancode.SDL_SCANCODE_DISPLAYSWITCH | SDLK_SCANCODE_MASK,

            SDLK_KBDILLUMTOGGLE = SDL_Scancode.SDL_SCANCODE_KBDILLUMTOGGLE | SDLK_SCANCODE_MASK,
            SDLK_KBDILLUMDOWN = SDL_Scancode.SDL_SCANCODE_KBDILLUMDOWN | SDLK_SCANCODE_MASK,
            SDLK_KBDILLUMUP = SDL_Scancode.SDL_SCANCODE_KBDILLUMUP | SDLK_SCANCODE_MASK,
            SDLK_EJECT = SDL_Scancode.SDL_SCANCODE_EJECT | SDLK_SCANCODE_MASK,
            SDLK_SLEEP = SDL_Scancode.SDL_SCANCODE_SLEEP | SDLK_SCANCODE_MASK
        }

        /* Key modifiers (bitfield) */
        [Flags]
        public enum SDL_Keymod : ushort
        {
            KMOD_NONE = 0x0000,
            KMOD_LSHIFT = 0x0001,
            KMOD_RSHIFT = 0x0002,
            KMOD_LCTRL = 0x0040,
            KMOD_RCTRL = 0x0080,
            KMOD_LALT = 0x0100,
            KMOD_RALT = 0x0200,
            KMOD_LGUI = 0x0400,
            KMOD_RGUI = 0x0800,
            KMOD_NUM = 0x1000,
            KMOD_CAPS = 0x2000,
            KMOD_MODE = 0x4000,
            KMOD_RESERVED = 0x8000,

            /* These are defines in the SDL headers */
            KMOD_CTRL = KMOD_LCTRL | KMOD_RCTRL,
            KMOD_SHIFT = KMOD_LSHIFT | KMOD_RSHIFT,
            KMOD_ALT = KMOD_LALT | KMOD_RALT,
            KMOD_GUI = KMOD_LGUI | KMOD_RGUI
        }

        /* Scancodes based off USB keyboard page (0x07) */
        public enum SDL_Scancode
        {
            SDL_SCANCODE_UNKNOWN = 0,

            SDL_SCANCODE_A = 4,
            SDL_SCANCODE_B = 5,
            SDL_SCANCODE_C = 6,
            SDL_SCANCODE_D = 7,
            SDL_SCANCODE_E = 8,
            SDL_SCANCODE_F = 9,
            SDL_SCANCODE_G = 10,
            SDL_SCANCODE_H = 11,
            SDL_SCANCODE_I = 12,
            SDL_SCANCODE_J = 13,
            SDL_SCANCODE_K = 14,
            SDL_SCANCODE_L = 15,
            SDL_SCANCODE_M = 16,
            SDL_SCANCODE_N = 17,
            SDL_SCANCODE_O = 18,
            SDL_SCANCODE_P = 19,
            SDL_SCANCODE_Q = 20,
            SDL_SCANCODE_R = 21,
            SDL_SCANCODE_S = 22,
            SDL_SCANCODE_T = 23,
            SDL_SCANCODE_U = 24,
            SDL_SCANCODE_V = 25,
            SDL_SCANCODE_W = 26,
            SDL_SCANCODE_X = 27,
            SDL_SCANCODE_Y = 28,
            SDL_SCANCODE_Z = 29,

            SDL_SCANCODE_1 = 30,
            SDL_SCANCODE_2 = 31,
            SDL_SCANCODE_3 = 32,
            SDL_SCANCODE_4 = 33,
            SDL_SCANCODE_5 = 34,
            SDL_SCANCODE_6 = 35,
            SDL_SCANCODE_7 = 36,
            SDL_SCANCODE_8 = 37,
            SDL_SCANCODE_9 = 38,
            SDL_SCANCODE_0 = 39,

            SDL_SCANCODE_RETURN = 40,
            SDL_SCANCODE_ESCAPE = 41,
            SDL_SCANCODE_BACKSPACE = 42,
            SDL_SCANCODE_TAB = 43,
            SDL_SCANCODE_SPACE = 44,

            SDL_SCANCODE_MINUS = 45,
            SDL_SCANCODE_EQUALS = 46,
            SDL_SCANCODE_LEFTBRACKET = 47,
            SDL_SCANCODE_RIGHTBRACKET = 48,
            SDL_SCANCODE_BACKSLASH = 49,
            SDL_SCANCODE_NONUSHASH = 50,
            SDL_SCANCODE_SEMICOLON = 51,
            SDL_SCANCODE_APOSTROPHE = 52,
            SDL_SCANCODE_GRAVE = 53,
            SDL_SCANCODE_COMMA = 54,
            SDL_SCANCODE_PERIOD = 55,
            SDL_SCANCODE_SLASH = 56,

            SDL_SCANCODE_CAPSLOCK = 57,

            SDL_SCANCODE_F1 = 58,
            SDL_SCANCODE_F2 = 59,
            SDL_SCANCODE_F3 = 60,
            SDL_SCANCODE_F4 = 61,
            SDL_SCANCODE_F5 = 62,
            SDL_SCANCODE_F6 = 63,
            SDL_SCANCODE_F7 = 64,
            SDL_SCANCODE_F8 = 65,
            SDL_SCANCODE_F9 = 66,
            SDL_SCANCODE_F10 = 67,
            SDL_SCANCODE_F11 = 68,
            SDL_SCANCODE_F12 = 69,

            SDL_SCANCODE_PRINTSCREEN = 70,
            SDL_SCANCODE_SCROLLLOCK = 71,
            SDL_SCANCODE_PAUSE = 72,
            SDL_SCANCODE_INSERT = 73,
            SDL_SCANCODE_HOME = 74,
            SDL_SCANCODE_PAGEUP = 75,
            SDL_SCANCODE_DELETE = 76,
            SDL_SCANCODE_END = 77,
            SDL_SCANCODE_PAGEDOWN = 78,
            SDL_SCANCODE_RIGHT = 79,
            SDL_SCANCODE_LEFT = 80,
            SDL_SCANCODE_DOWN = 81,
            SDL_SCANCODE_UP = 82,

            SDL_SCANCODE_NUMLOCKCLEAR = 83,
            SDL_SCANCODE_KP_DIVIDE = 84,
            SDL_SCANCODE_KP_MULTIPLY = 85,
            SDL_SCANCODE_KP_MINUS = 86,
            SDL_SCANCODE_KP_PLUS = 87,
            SDL_SCANCODE_KP_ENTER = 88,
            SDL_SCANCODE_KP_1 = 89,
            SDL_SCANCODE_KP_2 = 90,
            SDL_SCANCODE_KP_3 = 91,
            SDL_SCANCODE_KP_4 = 92,
            SDL_SCANCODE_KP_5 = 93,
            SDL_SCANCODE_KP_6 = 94,
            SDL_SCANCODE_KP_7 = 95,
            SDL_SCANCODE_KP_8 = 96,
            SDL_SCANCODE_KP_9 = 97,
            SDL_SCANCODE_KP_0 = 98,
            SDL_SCANCODE_KP_PERIOD = 99,

            SDL_SCANCODE_NONUSBACKSLASH = 100,
            SDL_SCANCODE_APPLICATION = 101,
            SDL_SCANCODE_POWER = 102,
            SDL_SCANCODE_KP_EQUALS = 103,
            SDL_SCANCODE_F13 = 104,
            SDL_SCANCODE_F14 = 105,
            SDL_SCANCODE_F15 = 106,
            SDL_SCANCODE_F16 = 107,
            SDL_SCANCODE_F17 = 108,
            SDL_SCANCODE_F18 = 109,
            SDL_SCANCODE_F19 = 110,
            SDL_SCANCODE_F20 = 111,
            SDL_SCANCODE_F21 = 112,
            SDL_SCANCODE_F22 = 113,
            SDL_SCANCODE_F23 = 114,
            SDL_SCANCODE_F24 = 115,
            SDL_SCANCODE_EXECUTE = 116,
            SDL_SCANCODE_HELP = 117,
            SDL_SCANCODE_MENU = 118,
            SDL_SCANCODE_SELECT = 119,
            SDL_SCANCODE_STOP = 120,
            SDL_SCANCODE_AGAIN = 121,
            SDL_SCANCODE_UNDO = 122,
            SDL_SCANCODE_CUT = 123,
            SDL_SCANCODE_COPY = 124,
            SDL_SCANCODE_PASTE = 125,
            SDL_SCANCODE_FIND = 126,
            SDL_SCANCODE_MUTE = 127,
            SDL_SCANCODE_VOLUMEUP = 128,
            SDL_SCANCODE_VOLUMEDOWN = 129,

            /* not sure whether there's a reason to enable these */
            /*	SDL_SCANCODE_LOCKINGCAPSLOCK = 130, */
            /*	SDL_SCANCODE_LOCKINGNUMLOCK = 131, */
            /*	SDL_SCANCODE_LOCKINGSCROLLLOCK = 132, */
            SDL_SCANCODE_KP_COMMA = 133,
            SDL_SCANCODE_KP_EQUALSAS400 = 134,

            SDL_SCANCODE_INTERNATIONAL1 = 135,
            SDL_SCANCODE_INTERNATIONAL2 = 136,
            SDL_SCANCODE_INTERNATIONAL3 = 137,
            SDL_SCANCODE_INTERNATIONAL4 = 138,
            SDL_SCANCODE_INTERNATIONAL5 = 139,
            SDL_SCANCODE_INTERNATIONAL6 = 140,
            SDL_SCANCODE_INTERNATIONAL7 = 141,
            SDL_SCANCODE_INTERNATIONAL8 = 142,
            SDL_SCANCODE_INTERNATIONAL9 = 143,
            SDL_SCANCODE_LANG1 = 144,
            SDL_SCANCODE_LANG2 = 145,
            SDL_SCANCODE_LANG3 = 146,
            SDL_SCANCODE_LANG4 = 147,
            SDL_SCANCODE_LANG5 = 148,
            SDL_SCANCODE_LANG6 = 149,
            SDL_SCANCODE_LANG7 = 150,
            SDL_SCANCODE_LANG8 = 151,
            SDL_SCANCODE_LANG9 = 152,

            SDL_SCANCODE_ALTERASE = 153,
            SDL_SCANCODE_SYSREQ = 154,
            SDL_SCANCODE_CANCEL = 155,
            SDL_SCANCODE_CLEAR = 156,
            SDL_SCANCODE_PRIOR = 157,
            SDL_SCANCODE_RETURN2 = 158,
            SDL_SCANCODE_SEPARATOR = 159,
            SDL_SCANCODE_OUT = 160,
            SDL_SCANCODE_OPER = 161,
            SDL_SCANCODE_CLEARAGAIN = 162,
            SDL_SCANCODE_CRSEL = 163,
            SDL_SCANCODE_EXSEL = 164,

            SDL_SCANCODE_KP_00 = 176,
            SDL_SCANCODE_KP_000 = 177,
            SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
            SDL_SCANCODE_DECIMALSEPARATOR = 179,
            SDL_SCANCODE_CURRENCYUNIT = 180,
            SDL_SCANCODE_CURRENCYSUBUNIT = 181,
            SDL_SCANCODE_KP_LEFTPAREN = 182,
            SDL_SCANCODE_KP_RIGHTPAREN = 183,
            SDL_SCANCODE_KP_LEFTBRACE = 184,
            SDL_SCANCODE_KP_RIGHTBRACE = 185,
            SDL_SCANCODE_KP_TAB = 186,
            SDL_SCANCODE_KP_BACKSPACE = 187,
            SDL_SCANCODE_KP_A = 188,
            SDL_SCANCODE_KP_B = 189,
            SDL_SCANCODE_KP_C = 190,
            SDL_SCANCODE_KP_D = 191,
            SDL_SCANCODE_KP_E = 192,
            SDL_SCANCODE_KP_F = 193,
            SDL_SCANCODE_KP_XOR = 194,
            SDL_SCANCODE_KP_POWER = 195,
            SDL_SCANCODE_KP_PERCENT = 196,
            SDL_SCANCODE_KP_LESS = 197,
            SDL_SCANCODE_KP_GREATER = 198,
            SDL_SCANCODE_KP_AMPERSAND = 199,
            SDL_SCANCODE_KP_DBLAMPERSAND = 200,
            SDL_SCANCODE_KP_VERTICALBAR = 201,
            SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
            SDL_SCANCODE_KP_COLON = 203,
            SDL_SCANCODE_KP_HASH = 204,
            SDL_SCANCODE_KP_SPACE = 205,
            SDL_SCANCODE_KP_AT = 206,
            SDL_SCANCODE_KP_EXCLAM = 207,
            SDL_SCANCODE_KP_MEMSTORE = 208,
            SDL_SCANCODE_KP_MEMRECALL = 209,
            SDL_SCANCODE_KP_MEMCLEAR = 210,
            SDL_SCANCODE_KP_MEMADD = 211,
            SDL_SCANCODE_KP_MEMSUBTRACT = 212,
            SDL_SCANCODE_KP_MEMMULTIPLY = 213,
            SDL_SCANCODE_KP_MEMDIVIDE = 214,
            SDL_SCANCODE_KP_PLUSMINUS = 215,
            SDL_SCANCODE_KP_CLEAR = 216,
            SDL_SCANCODE_KP_CLEARENTRY = 217,
            SDL_SCANCODE_KP_BINARY = 218,
            SDL_SCANCODE_KP_OCTAL = 219,
            SDL_SCANCODE_KP_DECIMAL = 220,
            SDL_SCANCODE_KP_HEXADECIMAL = 221,

            SDL_SCANCODE_LCTRL = 224,
            SDL_SCANCODE_LSHIFT = 225,
            SDL_SCANCODE_LALT = 226,
            SDL_SCANCODE_LGUI = 227,
            SDL_SCANCODE_RCTRL = 228,
            SDL_SCANCODE_RSHIFT = 229,
            SDL_SCANCODE_RALT = 230,
            SDL_SCANCODE_RGUI = 231,

            SDL_SCANCODE_MODE = 257,

            /* These come from the USB consumer page (0x0C) */
            SDL_SCANCODE_AUDIONEXT = 258,
            SDL_SCANCODE_AUDIOPREV = 259,
            SDL_SCANCODE_AUDIOSTOP = 260,
            SDL_SCANCODE_AUDIOPLAY = 261,
            SDL_SCANCODE_AUDIOMUTE = 262,
            SDL_SCANCODE_MEDIASELECT = 263,
            SDL_SCANCODE_WWW = 264,
            SDL_SCANCODE_MAIL = 265,
            SDL_SCANCODE_CALCULATOR = 266,
            SDL_SCANCODE_COMPUTER = 267,
            SDL_SCANCODE_AC_SEARCH = 268,
            SDL_SCANCODE_AC_HOME = 269,
            SDL_SCANCODE_AC_BACK = 270,
            SDL_SCANCODE_AC_FORWARD = 271,
            SDL_SCANCODE_AC_STOP = 272,
            SDL_SCANCODE_AC_REFRESH = 273,
            SDL_SCANCODE_AC_BOOKMARKS = 274,

            /* These come from other sources, and are mostly mac related */
            SDL_SCANCODE_BRIGHTNESSDOWN = 275,
            SDL_SCANCODE_BRIGHTNESSUP = 276,
            SDL_SCANCODE_DISPLAYSWITCH = 277,
            SDL_SCANCODE_KBDILLUMTOGGLE = 278,
            SDL_SCANCODE_KBDILLUMDOWN = 279,
            SDL_SCANCODE_KBDILLUMUP = 280,
            SDL_SCANCODE_EJECT = 281,
            SDL_SCANCODE_SLEEP = 282,

            SDL_SCANCODE_APP1 = 283,
            SDL_SCANCODE_APP2 = 284,

            /* This is not a key, simply marks the number of scancodes
			 * so that you know how big to make your arrays. */
            SDL_NUM_SCANCODES = 512
        }

        public const int SDLK_SCANCODE_MASK = 1 << 30;

        public const int SDL_BUTTON_LEFT = 1;
        public const int SDL_BUTTON_MIDDLE = 2;
        public const int SDL_BUTTON_RIGHT = 3;
        public const int SDL_BUTTON_X1 = 4;
        public const int SDL_BUTTON_X2 = 5;

        private static readonly Dictionary<int, Key> INTERNAL_keyMap = new Dictionary<int, Key>
        {
            { (int) SDL_Keycode.SDLK_a, Key.A },
            { (int) SDL_Keycode.SDLK_b, Key.B },
            { (int) SDL_Keycode.SDLK_c, Key.C },
            { (int) SDL_Keycode.SDLK_d, Key.D },
            { (int) SDL_Keycode.SDLK_e, Key.E },
            { (int) SDL_Keycode.SDLK_f, Key.F },
            { (int) SDL_Keycode.SDLK_g, Key.G },
            { (int) SDL_Keycode.SDLK_h, Key.H },
            { (int) SDL_Keycode.SDLK_i, Key.I },
            { (int) SDL_Keycode.SDLK_j, Key.J },
            { (int) SDL_Keycode.SDLK_k, Key.K },
            { (int) SDL_Keycode.SDLK_l, Key.L },
            { (int) SDL_Keycode.SDLK_m, Key.M },
            { (int) SDL_Keycode.SDLK_n, Key.N },
            { (int) SDL_Keycode.SDLK_o, Key.O },
            { (int) SDL_Keycode.SDLK_p, Key.P },
            { (int) SDL_Keycode.SDLK_q, Key.Q },
            { (int) SDL_Keycode.SDLK_r, Key.R },
            { (int) SDL_Keycode.SDLK_s, Key.S },
            { (int) SDL_Keycode.SDLK_t, Key.T },
            { (int) SDL_Keycode.SDLK_u, Key.U },
            { (int) SDL_Keycode.SDLK_v, Key.V },
            { (int) SDL_Keycode.SDLK_w, Key.W },
            { (int) SDL_Keycode.SDLK_x, Key.X },
            { (int) SDL_Keycode.SDLK_y, Key.Y },
            { (int) SDL_Keycode.SDLK_z, Key.Z },
            //{ (int) SDL_Keycode.SDLK_æ, Key. },
            //{ (int) SDL_Keycode.SDLK_ø, Key.Z },
            //{ (int) SDL_Keycode.SDLK_å, Key.Z },
            { (int) SDL_Keycode.SDLK_0, Key.D0 },
            { (int) SDL_Keycode.SDLK_1, Key.D1 },
            { (int) SDL_Keycode.SDLK_2, Key.D2 },
            { (int) SDL_Keycode.SDLK_3, Key.D3 },
            { (int) SDL_Keycode.SDLK_4, Key.D4 },
            { (int) SDL_Keycode.SDLK_5, Key.D5 },
            { (int) SDL_Keycode.SDLK_6, Key.D6 },
            { (int) SDL_Keycode.SDLK_7, Key.D7 },
            { (int) SDL_Keycode.SDLK_8, Key.D8 },
            { (int) SDL_Keycode.SDLK_9, Key.D9 },
            { (int) SDL_Keycode.SDLK_KP_0, Key.NumPad0 },
            { (int) SDL_Keycode.SDLK_KP_1, Key.NumPad1 },
            { (int) SDL_Keycode.SDLK_KP_2, Key.NumPad2 },
            { (int) SDL_Keycode.SDLK_KP_3, Key.NumPad3 },
            { (int) SDL_Keycode.SDLK_KP_4, Key.NumPad4 },
            { (int) SDL_Keycode.SDLK_KP_5, Key.NumPad5 },
            { (int) SDL_Keycode.SDLK_KP_6, Key.NumPad6 },
            { (int) SDL_Keycode.SDLK_KP_7, Key.NumPad7 },
            { (int) SDL_Keycode.SDLK_KP_8, Key.NumPad8 },
            { (int) SDL_Keycode.SDLK_KP_9, Key.NumPad9 },
            { (int) SDL_Keycode.SDLK_KP_CLEAR, Key.OemClear },
            { (int) SDL_Keycode.SDLK_KP_DECIMAL, Key.Decimal },
            { (int) SDL_Keycode.SDLK_KP_DIVIDE, Key.Divide },
            { (int) SDL_Keycode.SDLK_KP_ENTER, Key.Enter },
            { (int) SDL_Keycode.SDLK_KP_MINUS, Key.Subtract },
            { (int) SDL_Keycode.SDLK_KP_MULTIPLY, Key.Multiply },
            { (int) SDL_Keycode.SDLK_KP_PERIOD, Key.OemPeriod },
            { (int) SDL_Keycode.SDLK_KP_PLUS, Key.Add },
            { (int) SDL_Keycode.SDLK_F1, Key.F1 },
            { (int) SDL_Keycode.SDLK_F2, Key.F2 },
            { (int) SDL_Keycode.SDLK_F3, Key.F3 },
            { (int) SDL_Keycode.SDLK_F4, Key.F4 },
            { (int) SDL_Keycode.SDLK_F5, Key.F5 },
            { (int) SDL_Keycode.SDLK_F6, Key.F6 },
            { (int) SDL_Keycode.SDLK_F7, Key.F7 },
            { (int) SDL_Keycode.SDLK_F8, Key.F8 },
            { (int) SDL_Keycode.SDLK_F9, Key.F9 },
            { (int) SDL_Keycode.SDLK_F10, Key.F10 },
            { (int) SDL_Keycode.SDLK_F11, Key.F11 },
            { (int) SDL_Keycode.SDLK_F12, Key.F12 },
            { (int) SDL_Keycode.SDLK_F13, Key.F13 },
            { (int) SDL_Keycode.SDLK_F14, Key.F14 },
            { (int) SDL_Keycode.SDLK_F15, Key.F15 },
            { (int) SDL_Keycode.SDLK_F16, Key.F16 },
            { (int) SDL_Keycode.SDLK_F17, Key.F17 },
            { (int) SDL_Keycode.SDLK_F18, Key.F18 },
            { (int) SDL_Keycode.SDLK_F19, Key.F19 },
            { (int) SDL_Keycode.SDLK_F20, Key.F20 },
            { (int) SDL_Keycode.SDLK_F21, Key.F21 },
            { (int) SDL_Keycode.SDLK_F22, Key.F22 },
            { (int) SDL_Keycode.SDLK_F23, Key.F23 },
            { (int) SDL_Keycode.SDLK_F24, Key.F24 },
            { (int) SDL_Keycode.SDLK_SPACE, Key.Space },
            { (int) SDL_Keycode.SDLK_UP, Key.Up },
            { (int) SDL_Keycode.SDLK_DOWN, Key.Down },
            { (int) SDL_Keycode.SDLK_LEFT, Key.Left },
            { (int) SDL_Keycode.SDLK_RIGHT, Key.Right },
            { (int) SDL_Keycode.SDLK_LALT, Key.LeftAlt },
            { (int) SDL_Keycode.SDLK_RALT, Key.RightAlt },
            { (int) SDL_Keycode.SDLK_LCTRL, Key.LeftCtrl },
            { (int) SDL_Keycode.SDLK_RCTRL, Key.RightCtrl },
            { (int) SDL_Keycode.SDLK_LGUI, Key.LWin },
            { (int) SDL_Keycode.SDLK_RGUI, Key.RWin },
            { (int) SDL_Keycode.SDLK_LSHIFT, Key.LeftShift },
            { (int) SDL_Keycode.SDLK_RSHIFT, Key.RightShift },
            { (int) SDL_Keycode.SDLK_APPLICATION, Key.Apps },
            { (int) SDL_Keycode.SDLK_SLASH, Key.OemQuestion },
            { (int) SDL_Keycode.SDLK_BACKSLASH, Key.Oem5 },
            { (int) SDL_Keycode.SDLK_LEFTBRACKET, Key.OemOpenBrackets },
            { (int) SDL_Keycode.SDLK_RIGHTBRACKET, Key.OemCloseBrackets },
            { (int) SDL_Keycode.SDLK_CAPSLOCK, Key.CapsLock },
            { (int) SDL_Keycode.SDLK_COMMA, Key.OemComma },
            { (int) SDL_Keycode.SDLK_DELETE, Key.Delete },
            { (int) SDL_Keycode.SDLK_END, Key.End },
            { (int) SDL_Keycode.SDLK_BACKSPACE, Key.Back },
            { (int) SDL_Keycode.SDLK_RETURN, Key.Enter },
            { (int) SDL_Keycode.SDLK_ESCAPE, Key.Escape },
            { (int) SDL_Keycode.SDLK_HOME, Key.Home },
            { (int) SDL_Keycode.SDLK_INSERT, Key.Insert },
            { (int) SDL_Keycode.SDLK_MINUS, Key.OemMinus },
            { (int) SDL_Keycode.SDLK_NUMLOCKCLEAR, Key.NumLock },
            { (int) SDL_Keycode.SDLK_PAGEUP, Key.PageUp },
            { (int) SDL_Keycode.SDLK_PAGEDOWN, Key.PageDown },
            { (int) SDL_Keycode.SDLK_PAUSE, Key.Pause },
            { (int) SDL_Keycode.SDLK_PERIOD, Key.OemPeriod },
            { (int) SDL_Keycode.SDLK_EQUALS, Key.OemPlus },
            { (int) SDL_Keycode.SDLK_PRINTSCREEN, Key.PrintScreen },
            { (int) SDL_Keycode.SDLK_QUOTE, Key.OemQuotes },
            { (int) SDL_Keycode.SDLK_SCROLLLOCK, Key.Scroll },
            { (int) SDL_Keycode.SDLK_SEMICOLON, Key.OemSemicolon },
            { (int) SDL_Keycode.SDLK_SLEEP, Key.Sleep },
            { (int) SDL_Keycode.SDLK_TAB, Key.Tab },
            { (int) SDL_Keycode.SDLK_BACKQUOTE, Key.OemTilde },
            { (int) SDL_Keycode.SDLK_VOLUMEUP, Key.VolumeUp },
            { (int) SDL_Keycode.SDLK_VOLUMEDOWN, Key.VolumeDown },
            { (int) SDL_Keycode.SDLK_AUDIONEXT, Key.MediaNextTrack },
            { (int) SDL_Keycode.SDLK_AUDIOPREV, Key.MediaPreviousTrack },
            { (int) SDL_Keycode.SDLK_AUDIOSTOP, Key.MediaStop },
            { (int) SDL_Keycode.SDLK_AUDIOPLAY, Key.MediaPlayPause },
            { (int) SDL_Keycode.SDLK_AUDIOMUTE, Key.VolumeMute },
            { (int) SDL_Keycode.SDLK_MEDIASELECT, Key.SelectMedia },
            { '²' /* FIXME: AZERTY SDL2? -flibit */, Key.OemTilde },
            { 'é' /* FIXME: BEPO SDL2? -flibit */, Key.None },
            { '|' /* FIXME: Norwegian SDL2? -flibit */, Key.OemPipe },
            { '+' /* FIXME: Norwegian SDL2? -flibit */, Key.OemPlus },
            { 'ø' /* FIXME: Norwegian SDL2? -flibit */, Key.OemSemicolon },
            { 'æ' /* FIXME: Norwegian SDL2? -flibit */, Key.OemQuotes },
            { (int) SDL_Keycode.SDLK_UNKNOWN, Key.None }
        };

        private static readonly Dictionary<int, Dictionary<int, Key>> _localeKeyMap =
            new Dictionary<int, Dictionary<int, Key>>
            {
                {
                    1055, /* Turkish (Turkey) */
                    new Dictionary<int, Key> { { 34, Key.Oem3 }, { 42, Key.Oem8 }, { 60, Key.OemBackslash } }
                },
                {
                    2060, /* French (Belgium) */
                    new Dictionary<int, Key>
                    {
                        { 36, Key.Oem1 },
                        { 41, Key.OemOpenBrackets },
                        { 58, Key.OemQuestion },
                        { 59, Key.OemPeriod },
                        { 60, Key.OemBackslash },
                        { 94, Key.Oem6 },
                        { 178, Key.OemQuotes },
                        { 181, Key.Oem5 },
                        { 249, Key.Oem3 }
                    }
                },
                {
                    3082, /* Spanish (Spain) */
                    new Dictionary<int, Key>
                    {
                        { 39, Key.OemOpenBrackets },
                        { 43, Key.DeadCharProcessed },
                        { 45, Key.OemMinus },
                        { 60, Key.OemBackslash },
                        { 96, Key.Oem1 },
                        { 161, Key.Oem6 },
                        { 180, Key.OemQuotes },
                        { 186, Key.Oem5 },
                        { 231, Key.OemQuestion },
                        { 241, Key.Oem3 }
                    }
                },
                {
                    1044, /* Norwegian - (Norway) "nb-NO" */
                    new Dictionary<int, Key>
                    {
                        { 168, Key.Oem1 },
                        { 248, Key.Oem3 },
                        { 229, Key.Oem6 },
                        { 92, Key.OemOpenBrackets },
                        { 39, Key.OemQuestion },
                        { 60, Key.OemBackslash }
                    }
                },
                {
                    1046, /* Brazilian - (BR) "ABNT2/ABNT"*/
                    new Dictionary<int, Key>
                    {
                        { 39, Key.Oem3 },
                        { 47, Key.AbntC1 },
                        { 59, Key.OemQuestion },
                        { 91, Key.Oem6 },
                        { 92, Key.OemBackslash },
                        { 93, Key.Oem5 },
                        { 126, Key.OemQuotes },
                        { 180, Key.OemOpenBrackets },
                        { 231, Key.Oem1 }
                    }
                },
                {
                    1031, /* de-DE */
                    new Dictionary<int, Key>
                    {
                        { 35, Key.OemQuestion },
                        { 60, Key.OemBackslash },
                        { 180, Key.Oem6 },
                        { 223, Key.OemOpenBrackets },
                        { 228, Key.OemQuotes },
                        { 246, Key.Oem3 },
                        { 252, Key.Oem1 }
                    }
                }
            };

        public static IEnumerable<Keys> ToKeysList( this ModKey flagsEnumValue )
        {
            Keys ToKey( ModKey keymod )
            {
                return Keys.None;
            }

            return Enum.GetValues( typeof( ModKey ) ).Cast<ModKey>().Where( e => flagsEnumValue.HasFlag( e ) )
                .Select( ToKey );
        }

        public static ModKey KeymodFromKeyList( IEnumerable<Key> keys )
        {
            ModKey keymod = ModKey.None;

            foreach ( Key key in keys )
            {
                switch ( key )
                {
                    case Key.LeftShift:
                        keymod |= ModKey.LeftShift;
                        break;
                    case Key.RightShift:
                        keymod |= ModKey.RightShift;
                        break;
                    case Key.LeftCtrl:
                        keymod |= ModKey.LeftCtrl;
                        break;
                    case Key.RightCtrl:
                        keymod |= ModKey.RightCtrl;
                        break;
                    case Key.LeftAlt:
                        keymod |= ModKey.LeftAlt;
                        break;
                    case Key.RightAlt:
                        keymod |= ModKey.RightAlt;
                        break;
                }
            }

            return keymod;
        }

        public static Key SDLKeyToKeys( int sdlKey )
        {
            if ( _localeKeyMap.ContainsKey( Engine.KeyboardLayoutId ) &&
                 _localeKeyMap[Engine.KeyboardLayoutId].ContainsKey( sdlKey ) )
            {
                return _localeKeyMap[Engine.KeyboardLayoutId][sdlKey];
            }

            return INTERNAL_keyMap.TryGetValue( sdlKey, out Key keys ) ? keys : Key.None;
        }

        public static ModKey IntToModKey( int mod )
        {
            return (ModKey) ( mod & (int) ( ModKey.RightAlt | ModKey.RightCtrl | ModKey.RightShift | ModKey.LeftAlt |
                                            ModKey.LeftCtrl | ModKey.LeftShift ) );
        }

        public static SDL_Keycode SDL_SCANCODE_TO_KEYCODE( SDL_Scancode X )
        {
            return (SDL_Keycode) ( (int) X | SDLK_SCANCODE_MASK );
        }

        public static MouseOptions MouseButtonToMouseOptions( int button )
        {
            switch ( button )
            {
                case SDL_BUTTON_LEFT:
                    return MouseOptions.LeftButton;
                case SDL_BUTTON_MIDDLE:
                    return MouseOptions.MiddleButton;
                case SDL_BUTTON_RIGHT:
                    return MouseOptions.RightButton;
                case SDL_BUTTON_X1:
                    return MouseOptions.XButton1;
                case SDL_BUTTON_X2:
                    return MouseOptions.XButton2;
                default:
                    return MouseOptions.None;
            }
        }
    }
}