using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace LethalCompanyStatTracker {
    public static class MethodRedirect {
        //Credit for this method goes to https://github.com/zbigniewcebula
        public static unsafe void Redirect(this MethodInfo srcMethod, MethodInfo destMethod) {
            if (IntPtr.Size == sizeof(Int64)) {
                long srcBase = srcMethod.MethodHandle.GetFunctionPointer().ToInt64();
                long destBase = destMethod.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* pointerRawSource = (byte*)srcBase;

                // Pointer to insert jump address into native code
                long* pointerRawAddress = (long*)(pointerRawSource + 0x02);

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *(pointerRawSource + 0x00) = 0x48;
                *(pointerRawSource + 0x01) = 0xB8;
                *pointerRawAddress = destBase; // ( pointerRawSource + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *(pointerRawSource + 0x0A) = 0xFF;
                *(pointerRawSource + 0x0B) = 0xE0;
            } else {
                int srcBase = srcMethod.MethodHandle.GetFunctionPointer().ToInt32();
                int destBase = destMethod.MethodHandle.GetFunctionPointer().ToInt32();

                byte* pointerRawSource = (byte*)srcBase;

                // Pointer to insert jump address into native code
                int* pointerRawAddress = (int*)(pointerRawSource + 1);

                // Jump offset (less instruction size)
                int offset = destBase - srcBase - 5;

                // Insert 32-bit relative jump into native code
                *pointerRawSource = 0xE9;
                *pointerRawAddress = offset;
            }
        }
    }
}
