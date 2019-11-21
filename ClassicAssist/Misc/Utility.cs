using System;
using System.Runtime.InteropServices;

namespace ClassicAssist.Misc
{
    public static class Utility
    {
        public static T GetDelegateForFunctionPointer<T>(IntPtr ptr) where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }
    }
}