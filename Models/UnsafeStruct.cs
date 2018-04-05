using System.Runtime.InteropServices;

namespace GreenPower.Models
{
    public class UnsafeStruct
    {
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct RECEIVED
        {
            [FieldOffset(0)]
            public fixed byte _byte[4];
            [FieldOffset(0)]
            public uint _uint;
            [FieldOffset(0)]
            public float _float;
            [FieldOffset(0)]
            public int _int;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct TOSEND
        {
            [FieldOffset(0)]
            public uint _uint;
            [FieldOffset(0)]
            public fixed byte _byte[4];
        }        
    }
}