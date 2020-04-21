using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach {
    public static class ByteArrayExtensions {
        public static T ToStructure<T>(this byte[] bytes, int offset) where T : struct {
            return ToStructure<T>(bytes, ref offset);
        }
        public static T ToStructure<T>(this byte[] bytes, ref int offset) where T : struct {
            Type t = typeof(T);
            int size = Marshal.SizeOf(t);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(bytes, offset, ptr, size);
                offset += size;
                return (T)Marshal.PtrToStructure(ptr, t);
            } finally {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static T[] ToStructures<T>(this byte[] bytes, int count, int offset) where T : struct {
            return ToStructures<T>(bytes, count, ref offset);
        }
        public static T[] ToStructures<T>(this byte[] bytes, int count, ref int offset) where T : struct {
            T[] values = new T[count];
            for (int i = 0; i < count; ++i)
                values[i] = ToStructure<T>(bytes, ref offset);
            return values;
        }

        public static string ReadString(this byte[] buffer, int offset) {
            return ReadString(buffer, ref offset);
        }
        public static string ReadString(this byte[] buffer, ref int offset) {
            int strEnd = offset - 1;
            while (buffer[++strEnd] != 0) { }
            int size = strEnd - offset;

            string value = Encoding.ASCII.GetString(buffer, offset, size);

            offset = strEnd + 1;
            return value;
        }
    }
}
