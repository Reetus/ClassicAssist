#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

namespace ClassicAssist.Misc
{
    public enum Key
    {
        /// <summary>No key pressed. </summary>
        None = 0,

        /// <summary>The Cancel key. </summary>
        Cancel = 1,

        /// <summary>The Backspace key. </summary>
        Back = 2,

        /// <summary>The Tab key. </summary>
        Tab = 3,

        /// <summary>The Linefeed key. </summary>
        LineFeed = 4,

        /// <summary>The Clear key. </summary>
        Clear = 5,

        /// <summary>The Enter key. </summary>
        Enter = 6,

        /// <summary>The Return key. </summary>
        Return = 6,

        /// <summary>The Pause key. </summary>
        Pause = 7,

        /// <summary>The Caps Lock key. </summary>
        Capital = 8,

        /// <summary>The Caps Lock key. </summary>
        CapsLock = 8,

        /// <summary>The IME Hangul mode key. </summary>
        HangulMode = 9,

        /// <summary>The IME Kana mode key. </summary>
        KanaMode = 9,

        /// <summary>The IME Junja mode key. </summary>
        JunjaMode = 10, // 0x0000000A

        /// <summary>The IME Final mode key. </summary>
        FinalMode = 11, // 0x0000000B

        /// <summary>The IME Hanja mode key. </summary>
        HanjaMode = 12, // 0x0000000C

        /// <summary>The IME Kanji mode key. </summary>
        KanjiMode = 12, // 0x0000000C

        /// <summary>The ESC key. </summary>
        Escape = 13, // 0x0000000D

        /// <summary>The IME Convert key. </summary>
        ImeConvert = 14, // 0x0000000E

        /// <summary>The IME NonConvert key. </summary>
        ImeNonConvert = 15, // 0x0000000F

        /// <summary>The IME Accept key. </summary>
        ImeAccept = 16, // 0x00000010

        /// <summary>The IME Mode change request. </summary>
        ImeModeChange = 17, // 0x00000011

        /// <summary>The Spacebar key. </summary>
        Space = 18, // 0x00000012

        /// <summary>The Page Up key. </summary>
        PageUp = 19, // 0x00000013

        /// <summary>The Page Up key. </summary>
        Prior = 19, // 0x00000013

        /// <summary>The Page Down key. </summary>
        Next = 20, // 0x00000014

        /// <summary>The Page Down key. </summary>
        PageDown = 20, // 0x00000014

        /// <summary>The End key. </summary>
        End = 21, // 0x00000015

        /// <summary>The Home key. </summary>
        Home = 22, // 0x00000016

        /// <summary>The Left Arrow key. </summary>
        Left = 23, // 0x00000017

        /// <summary>The Up Arrow key. </summary>
        Up = 24, // 0x00000018

        /// <summary>The Right Arrow key. </summary>
        Right = 25, // 0x00000019

        /// <summary>The Down Arrow key. </summary>
        Down = 26, // 0x0000001A

        /// <summary>The Select key. </summary>
        Select = 27, // 0x0000001B

        /// <summary>The Print key. </summary>
        Print = 28, // 0x0000001C

        /// <summary>The Execute key. </summary>
        Execute = 29, // 0x0000001D

        /// <summary>The Print Screen key. </summary>
        PrintScreen = 30, // 0x0000001E

        /// <summary>The Print Screen key. </summary>
        Snapshot = 30, // 0x0000001E

        /// <summary>The Insert key. </summary>
        Insert = 31, // 0x0000001F

        /// <summary>The Delete key. </summary>
        Delete = 32, // 0x00000020

        /// <summary>The Help key. </summary>
        Help = 33, // 0x00000021

        /// <summary>The 0 (zero) key. </summary>
        D0 = 34, // 0x00000022

        /// <summary>The 1 (one) key. </summary>
        D1 = 35, // 0x00000023

        /// <summary>The 2 key. </summary>
        D2 = 36, // 0x00000024

        /// <summary>The 3 key. </summary>
        D3 = 37, // 0x00000025

        /// <summary>The 4 key. </summary>
        D4 = 38, // 0x00000026

        /// <summary>The 5 key. </summary>
        D5 = 39, // 0x00000027

        /// <summary>The 6 key. </summary>
        D6 = 40, // 0x00000028

        /// <summary>The 7 key. </summary>
        D7 = 41, // 0x00000029

        /// <summary>The 8 key. </summary>
        D8 = 42, // 0x0000002A

        /// <summary>The 9 key. </summary>
        D9 = 43, // 0x0000002B

        /// <summary>The A key. </summary>
        A = 44, // 0x0000002C

        /// <summary>The B key. </summary>
        B = 45, // 0x0000002D

        /// <summary>The C key. </summary>
        C = 46, // 0x0000002E

        /// <summary>The D key. </summary>
        D = 47, // 0x0000002F

        /// <summary>The E key. </summary>
        E = 48, // 0x00000030

        /// <summary>The F key. </summary>
        F = 49, // 0x00000031

        /// <summary>The G key. </summary>
        G = 50, // 0x00000032

        /// <summary>The H key. </summary>
        H = 51, // 0x00000033

        /// <summary>The I key. </summary>
        I = 52, // 0x00000034

        /// <summary>The J key. </summary>
        J = 53, // 0x00000035

        /// <summary>The K key. </summary>
        K = 54, // 0x00000036

        /// <summary>The L key. </summary>
        L = 55, // 0x00000037

        /// <summary>The M key. </summary>
        M = 56, // 0x00000038

        /// <summary>The N key. </summary>
        N = 57, // 0x00000039

        /// <summary>The O key. </summary>
        O = 58, // 0x0000003A

        /// <summary>The P key. </summary>
        P = 59, // 0x0000003B

        /// <summary>The Q key. </summary>
        Q = 60, // 0x0000003C

        /// <summary>The R key. </summary>
        R = 61, // 0x0000003D

        /// <summary>The S key. </summary>
        S = 62, // 0x0000003E

        /// <summary>The T key. </summary>
        T = 63, // 0x0000003F

        /// <summary>The U key. </summary>
        U = 64, // 0x00000040

        /// <summary>The V key. </summary>
        V = 65, // 0x00000041

        /// <summary>The W key. </summary>
        W = 66, // 0x00000042

        /// <summary>The X key. </summary>
        X = 67, // 0x00000043

        /// <summary>The Y key. </summary>
        Y = 68, // 0x00000044

        /// <summary>The Z key. </summary>
        Z = 69, // 0x00000045

        /// <summary>The left Windows logo key (Microsoft Natural Keyboard). </summary>
        LWin = 70, // 0x00000046

        /// <summary>The right Windows logo key (Microsoft Natural Keyboard). </summary>
        RWin = 71, // 0x00000047

        /// <summary>The Application key (Microsoft Natural Keyboard). </summary>
        Apps = 72, // 0x00000048

        /// <summary>The Computer Sleep key. </summary>
        Sleep = 73, // 0x00000049

        /// <summary>The 0 key on the numeric keypad. </summary>
        NumPad0 = 74, // 0x0000004A

        /// <summary>The 1 key on the numeric keypad. </summary>
        NumPad1 = 75, // 0x0000004B

        /// <summary>The 2 key on the numeric keypad. </summary>
        NumPad2 = 76, // 0x0000004C

        /// <summary>The 3 key on the numeric keypad. </summary>
        NumPad3 = 77, // 0x0000004D

        /// <summary>The 4 key on the numeric keypad. </summary>
        NumPad4 = 78, // 0x0000004E

        /// <summary>The 5 key on the numeric keypad. </summary>
        NumPad5 = 79, // 0x0000004F

        /// <summary>The 6 key on the numeric keypad. </summary>
        NumPad6 = 80, // 0x00000050

        /// <summary>The 7 key on the numeric keypad. </summary>
        NumPad7 = 81, // 0x00000051

        /// <summary>The 8 key on the numeric keypad. </summary>
        NumPad8 = 82, // 0x00000052

        /// <summary>The 9 key on the numeric keypad. </summary>
        NumPad9 = 83, // 0x00000053

        /// <summary>The Multiply key. </summary>
        Multiply = 84, // 0x00000054

        /// <summary>The Add key. </summary>
        Add = 85, // 0x00000055

        /// <summary>The Separator key. </summary>
        Separator = 86, // 0x00000056

        /// <summary>The Subtract key. </summary>
        Subtract = 87, // 0x00000057

        /// <summary>The Decimal key. </summary>
        Decimal = 88, // 0x00000058

        /// <summary>The Divide key. </summary>
        Divide = 89, // 0x00000059

        /// <summary>The F1 key. </summary>
        F1 = 90, // 0x0000005A

        /// <summary>The F2 key. </summary>
        F2 = 91, // 0x0000005B

        /// <summary>The F3 key. </summary>
        F3 = 92, // 0x0000005C

        /// <summary>The F4 key. </summary>
        F4 = 93, // 0x0000005D

        /// <summary>The F5 key. </summary>
        F5 = 94, // 0x0000005E

        /// <summary>The F6 key. </summary>
        F6 = 95, // 0x0000005F

        /// <summary>The F7 key. </summary>
        F7 = 96, // 0x00000060

        /// <summary>The F8 key. </summary>
        F8 = 97, // 0x00000061

        /// <summary>The F9 key. </summary>
        F9 = 98, // 0x00000062

        /// <summary>The F10 key. </summary>
        F10 = 99, // 0x00000063

        /// <summary>The F11 key. </summary>
        F11 = 100, // 0x00000064

        /// <summary>The F12 key. </summary>
        F12 = 101, // 0x00000065

        /// <summary>The F13 key. </summary>
        F13 = 102, // 0x00000066

        /// <summary>The F14 key. </summary>
        F14 = 103, // 0x00000067

        /// <summary>The F15 key. </summary>
        F15 = 104, // 0x00000068

        /// <summary>The F16 key. </summary>
        F16 = 105, // 0x00000069

        /// <summary>The F17 key. </summary>
        F17 = 106, // 0x0000006A

        /// <summary>The F18 key. </summary>
        F18 = 107, // 0x0000006B

        /// <summary>The F19 key. </summary>
        F19 = 108, // 0x0000006C

        /// <summary>The F20 key. </summary>
        F20 = 109, // 0x0000006D

        /// <summary>The F21 key. </summary>
        F21 = 110, // 0x0000006E

        /// <summary>The F22 key. </summary>
        F22 = 111, // 0x0000006F

        /// <summary>The F23 key. </summary>
        F23 = 112, // 0x00000070

        /// <summary>The F24 key. </summary>
        F24 = 113, // 0x00000071

        /// <summary>The Num Lock key. </summary>
        NumLock = 114, // 0x00000072

        /// <summary>The Scroll Lock key. </summary>
        Scroll = 115, // 0x00000073

        /// <summary>The left Shift key. </summary>
        LeftShift = 116, // 0x00000074

        /// <summary>The right Shift key. </summary>
        RightShift = 117, // 0x00000075

        /// <summary>The left CTRL key. </summary>
        LeftCtrl = 118, // 0x00000076

        /// <summary>The right CTRL key. </summary>
        RightCtrl = 119, // 0x00000077

        /// <summary>The left ALT key. </summary>
        LeftAlt = 120, // 0x00000078

        /// <summary>The right ALT key. </summary>
        RightAlt = 121, // 0x00000079

        /// <summary>The Browser Back key. </summary>
        BrowserBack = 122, // 0x0000007A

        /// <summary>The Browser Forward key. </summary>
        BrowserForward = 123, // 0x0000007B

        /// <summary>The Browser Refresh key. </summary>
        BrowserRefresh = 124, // 0x0000007C

        /// <summary>The Browser Stop key. </summary>
        BrowserStop = 125, // 0x0000007D

        /// <summary>The Browser Search key. </summary>
        BrowserSearch = 126, // 0x0000007E

        /// <summary>The Browser Favorites key. </summary>
        BrowserFavorites = 127, // 0x0000007F

        /// <summary>The Browser Home key. </summary>
        BrowserHome = 128, // 0x00000080

        /// <summary>The Volume Mute key. </summary>
        VolumeMute = 129, // 0x00000081

        /// <summary>The Volume Down key. </summary>
        VolumeDown = 130, // 0x00000082

        /// <summary>The Volume Up key. </summary>
        VolumeUp = 131, // 0x00000083

        /// <summary>The Media Next Track key. </summary>
        MediaNextTrack = 132, // 0x00000084

        /// <summary>The Media Previous Track key. </summary>
        MediaPreviousTrack = 133, // 0x00000085

        /// <summary>The Media Stop key. </summary>
        MediaStop = 134, // 0x00000086

        /// <summary>The Media Play Pause key. </summary>
        MediaPlayPause = 135, // 0x00000087

        /// <summary>The Launch Mail key. </summary>
        LaunchMail = 136, // 0x00000088

        /// <summary>The Select Media key. </summary>
        SelectMedia = 137, // 0x00000089

        /// <summary>The Launch Application1 key. </summary>
        LaunchApplication1 = 138, // 0x0000008A

        /// <summary>The Launch Application2 key. </summary>
        LaunchApplication2 = 139, // 0x0000008B

        /// <summary>The OEM 1 key. </summary>
        Oem1 = 140, // 0x0000008C

        /// <summary>The OEM Semicolon key. </summary>
        OemSemicolon = 140, // 0x0000008C

        /// <summary>The OEM Addition key. </summary>
        OemPlus = 141, // 0x0000008D

        /// <summary>The OEM Comma key. </summary>
        OemComma = 142, // 0x0000008E

        /// <summary>The OEM Minus key. </summary>
        OemMinus = 143, // 0x0000008F

        /// <summary>The OEM Period key. </summary>
        OemPeriod = 144, // 0x00000090

        /// <summary>The OEM 2 key. </summary>
        Oem2 = 145, // 0x00000091

        /// <summary>The OEM Question key. </summary>
        OemQuestion = 145, // 0x00000091

        /// <summary>The OEM 3 key. </summary>
        Oem3 = 146, // 0x00000092

        /// <summary>The OEM Tilde key. </summary>
        OemTilde = 146, // 0x00000092

        /// <summary>The ABNT_C1 (Brazilian) key. </summary>
        AbntC1 = 147, // 0x00000093

        /// <summary>The ABNT_C2 (Brazilian) key. </summary>
        AbntC2 = 148, // 0x00000094

        /// <summary>The OEM 4 key. </summary>
        Oem4 = 149, // 0x00000095

        /// <summary>The OEM Open Brackets key. </summary>
        OemOpenBrackets = 149, // 0x00000095

        /// <summary>The OEM 5 key. </summary>
        Oem5 = 150, // 0x00000096

        /// <summary>The OEM Pipe key. </summary>
        OemPipe = 150, // 0x00000096

        /// <summary>The OEM 6 key. </summary>
        Oem6 = 151, // 0x00000097

        /// <summary>The OEM Close Brackets key. </summary>
        OemCloseBrackets = 151, // 0x00000097

        /// <summary>The OEM 7 key. </summary>
        Oem7 = 152, // 0x00000098

        /// <summary>The OEM Quotes key. </summary>
        OemQuotes = 152, // 0x00000098

        /// <summary>The OEM 8 key. </summary>
        Oem8 = 153, // 0x00000099

        /// <summary>The OEM 102 key. </summary>
        Oem102 = 154, // 0x0000009A

        /// <summary>The OEM Backslash key. </summary>
        OemBackslash = 154, // 0x0000009A

        /// <summary>A special key masking the real key being processed by an IME. </summary>
        ImeProcessed = 155, // 0x0000009B

        /// <summary>A special key masking the real key being processed as a system key. </summary>
        System = 156, // 0x0000009C

        /// <summary>The DBE_ALPHANUMERIC key. </summary>
        DbeAlphanumeric = 157, // 0x0000009D

        /// <summary>The OEM ATTN key. </summary>
        OemAttn = 157, // 0x0000009D

        /// <summary>The DBE_KATAKANA key. </summary>
        DbeKatakana = 158, // 0x0000009E

        /// <summary>The OEM FINISH key. </summary>
        OemFinish = 158, // 0x0000009E

        /// <summary>The DBE_HIRAGANA key. </summary>
        DbeHiragana = 159, // 0x0000009F

        /// <summary>The OEM COPY key. </summary>
        OemCopy = 159, // 0x0000009F

        /// <summary>The DBE_SBCSCHAR key. </summary>
        DbeSbcsChar = 160, // 0x000000A0

        /// <summary>The OEM AUTO key. </summary>
        OemAuto = 160, // 0x000000A0

        /// <summary>The DBE_DBCSCHAR key. </summary>
        DbeDbcsChar = 161, // 0x000000A1

        /// <summary>The OEM ENLW key. </summary>
        OemEnlw = 161, // 0x000000A1

        /// <summary>The DBE_ROMAN key. </summary>
        DbeRoman = 162, // 0x000000A2

        /// <summary>The OEM BACKTAB key. </summary>
        OemBackTab = 162, // 0x000000A2

        /// <summary>The ATTN key. </summary>
        Attn = 163, // 0x000000A3

        /// <summary>The DBE_NOROMAN key. </summary>
        DbeNoRoman = 163, // 0x000000A3

        /// <summary>The CRSEL key. </summary>
        CrSel = 164, // 0x000000A4

        /// <summary>The DBE_ENTERWORDREGISTERMODE key. </summary>
        DbeEnterWordRegisterMode = 164, // 0x000000A4

        /// <summary>The DBE_ENTERIMECONFIGMODE key. </summary>
        DbeEnterImeConfigureMode = 165, // 0x000000A5

        /// <summary>The EXSEL key. </summary>
        ExSel = 165, // 0x000000A5

        /// <summary>The DBE_FLUSHSTRING key. </summary>
        DbeFlushString = 166, // 0x000000A6

        /// <summary>The ERASE EOF key. </summary>
        EraseEof = 166, // 0x000000A6

        /// <summary>The DBE_CODEINPUT key. </summary>
        DbeCodeInput = 167, // 0x000000A7

        /// <summary>The PLAY key. </summary>
        Play = 167, // 0x000000A7

        /// <summary>The DBE_NOCODEINPUT key. </summary>
        DbeNoCodeInput = 168, // 0x000000A8

        /// <summary>The ZOOM key. </summary>
        Zoom = 168, // 0x000000A8

        /// <summary>The DBE_DETERMINESTRING key. </summary>
        DbeDetermineString = 169, // 0x000000A9

        /// <summary>A constant reserved for future use. </summary>
        NoName = 169, // 0x000000A9

        /// <summary>The DBE_ENTERDLGCONVERSIONMODE key. </summary>
        DbeEnterDialogConversionMode = 170, // 0x000000AA

        /// <summary>The PA1 key. </summary>
        Pa1 = 170, // 0x000000AA

        /// <summary>The OEM Clear key. </summary>
        OemClear = 171, // 0x000000AB

        /// <summary>The key is used with another key to create a single combined character.</summary>
        DeadCharProcessed = 172 // 0x000000AC
    }

    public enum Keys
    {
        None = 0x00000000,
        LButton = 0x00000001,
        RButton = 0x00000002,
        Cancel = 0x00000003,
        MButton = 0x00000004,
        XButton1 = 0x00000005,
        XButton2 = 0x00000006,
        Back = 0x00000008,
        Tab = 0x00000009,
        LineFeed = 0x0000000A,
        Clear = 0x0000000C,
        Return = 0x0000000D,
        Enter = 0x0000000D,
        ShiftKey = 0x00000010,
        ControlKey = 0x00000011,
        Menu = 0x00000012,
        Pause = 0x00000013,
        CapsLock = 0x00000014,
        Capital = 0x00000014,
        KanaMode = 0x00000015,
        HanguelMode = 0x00000015,
        HangulMode = 0x00000015,
        JunjaMode = 0x00000017,
        FinalMode = 0x00000018,
        KanjiMode = 0x00000019,
        HanjaMode = 0x00000019,
        Escape = 0x0000001B,
        IMEConvert = 0x0000001C,
        IMENonconvert = 0x0000001D,
        IMEAceept = 0x0000001E,
        IMEModeChange = 0x0000001F,
        Space = 0x00000020,
        PageUp = 0x00000021,
        Prior = 0x00000021,
        PageDown = 0x00000022,
        Next = 0x00000022,
        End = 0x00000023,
        Home = 0x00000024,
        Left = 0x00000025,
        Up = 0x00000026,
        Right = 0x00000027,
        Down = 0x00000028,
        Select = 0x00000029,
        Print = 0x0000002A,
        Execute = 0x0000002B,
        PrintScreen = 0x0000002C,
        Snapshot = 0x0000002C,
        Insert = 0x0000002D,
        Delete = 0x0000002E,
        Help = 0x0000002F,
        D0 = 0x00000030,
        D1 = 0x00000031,
        D2 = 0x00000032,
        D3 = 0x00000033,
        D4 = 0x00000034,
        D5 = 0x00000035,
        D6 = 0x00000036,
        D7 = 0x00000037,
        D8 = 0x00000038,
        D9 = 0x00000039,
        A = 0x00000041,
        B = 0x00000042,
        C = 0x00000043,
        D = 0x00000044,
        E = 0x00000045,
        F = 0x00000046,
        G = 0x00000047,
        H = 0x00000048,
        I = 0x00000049,
        J = 0x0000004A,
        K = 0x0000004B,
        L = 0x0000004C,
        M = 0x0000004D,
        N = 0x0000004E,
        O = 0x0000004F,
        P = 0x00000050,
        Q = 0x00000051,
        R = 0x00000052,
        S = 0x00000053,
        T = 0x00000054,
        U = 0x00000055,
        V = 0x00000056,
        W = 0x00000057,
        X = 0x00000058,
        Y = 0x00000059,
        Z = 0x0000005A,
        LWin = 0x0000005B,
        RWin = 0x0000005C,
        Apps = 0x0000005D,
        NumPad0 = 0x00000060,
        NumPad1 = 0x00000061,
        NumPad2 = 0x00000062,
        NumPad3 = 0x00000063,
        NumPad4 = 0x00000064,
        NumPad5 = 0x00000065,
        NumPad6 = 0x00000066,
        NumPad7 = 0x00000067,
        NumPad8 = 0x00000068,
        NumPad9 = 0x00000069,
        Multiply = 0x0000006A,
        Add = 0x0000006B,
        Separator = 0x0000006C,
        Subtract = 0x0000006D,
        Decimal = 0x0000006E,
        Divide = 0x0000006F,
        F1 = 0x00000070,
        F2 = 0x00000071,
        F3 = 0x00000072,
        F4 = 0x00000073,
        F5 = 0x00000074,
        F6 = 0x00000075,
        F7 = 0x00000076,
        F8 = 0x00000077,
        F9 = 0x00000078,
        F10 = 0x00000079,
        F11 = 0x0000007A,
        F12 = 0x0000007B,
        F13 = 0x0000007C,
        F14 = 0x0000007D,
        F15 = 0x0000007E,
        F16 = 0x0000007F,
        F17 = 0x00000080,
        F18 = 0x00000081,
        F19 = 0x00000082,
        F20 = 0x00000083,
        F21 = 0x00000084,
        F22 = 0x00000085,
        F23 = 0x00000086,
        F24 = 0x00000087,
        NumLock = 0x00000090,
        Scroll = 0x00000091,
        LShiftKey = 0x000000A0,
        RShiftKey = 0x000000A1,
        LControlKey = 0x000000A2,
        RControlKey = 0x000000A3,
        LMenu = 0x000000A4,
        RMenu = 0x000000A5,
        BrowserBack = 0x000000A6,
        BrowserForward = 0x000000A7,
        BrowserRefresh = 0x000000A8,
        BrowserStop = 0x000000A9,
        BrowserSearch = 0x000000AA,
        BrowserFavorites = 0x000000AB,
        BrowserHome = 0x000000AC,
        VolumeMute = 0x000000AD,
        VolumeDown = 0x000000AE,
        VolumeUp = 0x000000AF,
        MediaNextTrack = 0x000000B0,
        MediaPreviousTrack = 0x000000B1,
        MediaStop = 0x000000B2,
        MediaPlayPause = 0x000000B3,
        LaunchMail = 0x000000B4,
        SelectMedia = 0x000000B5,
        LaunchApplication1 = 0x000000B6,
        LaunchApplication2 = 0x000000B7,
        OemSemicolon = 0x000000BA,
        Oemplus = 0x000000BB,
        Oemcomma = 0x000000BC,
        OemMinus = 0x000000BD,
        OemPeriod = 0x000000BE,
        OemQuestion = 0x000000BF,
        Oemtilde = 0x000000C0,
        OemOpenBrackets = 0x000000DB,
        OemPipe = 0x000000DC,
        OemCloseBrackets = 0x000000DD,
        OemQuotes = 0x000000DE,
        Oem8 = 0x000000DF,
        OemBackslash = 0x000000E2,
        ProcessKey = 0x000000E5,
        Attn = 0x000000F6,
        Crsel = 0x000000F7,
        Exsel = 0x000000F8,
        EraseEof = 0x000000F9,
        Play = 0x000000FA,
        Zoom = 0x000000FB,
        NoName = 0x000000FC,
        Pa1 = 0x000000FD,
        OemClear = 0x000000FE,
        KeyCode = 0x0000FFFF,
        Shift = 0x00010000,
        Control = 0x00020000,
        Alt = 0x00040000,
        Modifiers = unchecked( (int) 0xFFFF0000 ),
        IMEAccept = 0x0000001E,
        Oem1 = 0x000000BA,
        Oem102 = 0x000000E2,
        Oem2 = 0x000000BF,
        Oem3 = 0x000000C0,
        Oem4 = 0x000000DB,
        Oem5 = 0x000000DC,
        Oem6 = 0x000000DD,
        Oem7 = 0x000000DE,
        Packet = 0x000000E7,
        Sleep = 0x0000005F
    }
}