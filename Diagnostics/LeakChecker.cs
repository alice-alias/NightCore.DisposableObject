using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace NightCore.Diagnostics
{
    public static class LeakChecker
    {
        public static bool DebugWrite { get; set; }

        static readonly Dictionary<IntPtr, string> names = new Dictionary<IntPtr, string>();

        public static void Construct(GCHandle handle, string name)
            => Construct(GCHandle.ToIntPtr(handle), name);

        public static void Construct(IntPtr handle, string name)
        {
            lock (names)
            {
                names[handle] = name;

                if (DebugWrite)
                    Debug.WriteLine($"Constructed #{handle.ToString("x08")}: {name}");
            }
        }

        public static void Destruct(GCHandle handle)
            => Destruct(GCHandle.ToIntPtr(handle));

        public static void Destruct(IntPtr handle)
        {
            lock (names)
            {
                if (names.TryGetValue(handle, out var name) && DebugWrite)
                    Debug.WriteLine($"Destructed #{handle.ToString("x08")}: {name}");

                names.Remove(handle);
            }
        }
        public static void NotifyLeak(GCHandle handle)
            => NotifyLeak(GCHandle.ToIntPtr(handle));

        public static void NotifyLeak(IntPtr handle)
        {
            lock (names)
            {
                if (names.TryGetValue(handle, out var name) && DebugWrite)
                    Debug.WriteLine($"Automatically destructed #{handle.ToString("x08")}: {name}");
            }
        }

        public static void PrintAliveObjects() => PrintAliveObjects(x => Debug.WriteLine(x));

        public static void PrintAliveObjects(Action<string> writeLine)
        {
            lock (names)
            {
                if (names.Count == 0)
                {
                    writeLine("No leaked object.");
                }
                else
                {
                    writeLine("Leaked objects:");
                    foreach (var d in names.ToArray())
                        writeLine($"  #{d.Key.ToString("x08")}: {d.Value}");
                }
            }
        }
    }
}
