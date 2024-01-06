using System;
using System.Runtime.InteropServices;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable once CheckNamespace
namespace Assistant
{
    public static partial class Engine
    {
        [return: MarshalAs( UnmanagedType.I1 )]
        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        public delegate bool OnPacketSendRecv_new_intptr( IntPtr data, ref int length );

        public struct PluginHeader
        {
            public int ClientVersion;
            public IntPtr HWND;
            public IntPtr OnRecv;
            public IntPtr OnSend;
            public IntPtr OnHotkeyPressed;
            public IntPtr OnMouse;
            public IntPtr OnPlayerPositionChanged;
            public IntPtr OnClientClosing;
            public IntPtr OnInitialize;
            public IntPtr OnConnected;
            public IntPtr OnDisconnected;
            public IntPtr OnFocusGained;
            public IntPtr OnFocusLost;
            public IntPtr GetUOFilePath;
            public IntPtr Recv;
            public IntPtr Send;
            public IntPtr GetPacketLength;
            public IntPtr GetPlayerPosition;
            public IntPtr CastSpell;
            public IntPtr GetStaticImage;
            public IntPtr Tick;
            public IntPtr RequestMove;
            public IntPtr SetTitle;

            public IntPtr OnRecv_new, OnSend_new, Recv_new, Send_new;

            public IntPtr OnDrawCmdList;
            public IntPtr SDL_Window;
            public IntPtr OnWndProc;
            public IntPtr GetStaticData;
            public IntPtr GetTileData;
            public IntPtr GetCliloc;
        }
    }
}