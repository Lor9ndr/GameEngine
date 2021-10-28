using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine
{
    public static class Logger
    {
        private static DebugProc _debugProcCallback = DebugCallback;
        private static GCHandle _debugProcCallbackHandle;
        private static void DebugCallback(DebugSource source,
                                   DebugType type,
                                   int id,
                                   DebugSeverity severity,
                                   int length,
                                   IntPtr message,
                                   IntPtr userParam)
        {
            string messageString = Marshal.PtrToStringAnsi(message, length);

            Console.WriteLine($"{severity} {type} | {messageString}");

            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(messageString);
            }
            _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static void CheckLastError()
        {
            ErrorCode errorCode = GL.GetError();

            if (errorCode != ErrorCode.NoError)
            {
                Console.WriteLine(errorCode.ToString());
            }
        }
        public static void ClearError()
        {
            while (GL.GetError() != ErrorCode.NoError)
            {

            }
        }
    }
}
