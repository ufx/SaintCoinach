using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Graphics.Viewer.Interop {
    public static class FbxInterop {
        static object _Lock = new object();

        static bool _Initialized;

        static bool _IsThreaded;
        static volatile bool _IsLive;

        static volatile bool _IsAwaiting = false;
        static Func<object> _CurrentAction;
        static object _CurrentResult;

        static FbxInterop() {
        }

        public static void InitializeSTA() {
            if (_Initialized)
                throw new InvalidOperationException();
            initFbxInterop();

            _IsThreaded = false;
            _Initialized = true;
        }
        public static void InitializeMTA() {
            if (_Initialized)
                throw new InvalidOperationException();
            var t = new System.Threading.Thread(FbxLoop);
            t.Name = "FBX thread";
            t.IsBackground = true;
            t.Start();

            _IsThreaded = true;
            _IsLive = true;
            _Initialized = true;
        }

        internal static void Execute(Action action) {
            Execute<object>(() => { action(); return null; });
        }
        internal static T Execute<T>(Func<T> func) {
            if (!_Initialized)
                throw new InvalidOperationException();

            if (!_IsThreaded)
                return func();

            T result;
            lock (_Lock) {
                _CurrentAction = () => (object)func();
                _IsAwaiting = true;
                while (_IsAwaiting) {
                    if (!_IsLive)
                        throw new InvalidProgramException();
                }
                result = (T)_CurrentResult;
            }
            return result;
        }

        static void FbxLoop() {
            try {
                initFbxInterop();

                while (true) {
                    if (_IsAwaiting) {
                        _CurrentResult = _CurrentAction();
                        _IsAwaiting = false;
                    }
                    else
                        System.Threading.Thread.Sleep(5);
                }
            }
            finally {
                _IsLive = false;
                quitFbxInterop();
            }
        }

        [DllImport("fbxInterop.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void initFbxInterop();
        [DllImport("fbxInterop.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void quitFbxInterop();
    }
}
