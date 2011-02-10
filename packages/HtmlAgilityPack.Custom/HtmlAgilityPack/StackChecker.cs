using System;
using System.Runtime.InteropServices;


namespace HtmlAgilityPack
{
    public class StackChecker
    {
        public unsafe static bool HasSufficientStack(long bytes)
        {
            var stackInfo = new MEMORY_BASIC_INFORMATION();

            // We subtract one page for our request. VirtualQuery rounds UP to the next page.
            // Unfortunately, the stack grows down. If we're on the first page (last page in the
            // VirtualAlloc), we'll be moved to the next page, which is off the stack! Note this
            // doesn't work right for IA64 due to bigger pages.
            IntPtr currentAddr = new IntPtr((uint)&stackInfo - 4096);

            // Query for the current stack allocation information.
            VirtualQuery(currentAddr, ref stackInfo, sizeof(MEMORY_BASIC_INFORMATION));

            // If the current address minus the base (remember: the stack grows downward in the
            // address space) is greater than the number of bytes requested plus the reserved
            // space at the end, the request has succeeded.
            return ((uint)currentAddr.ToInt64() - stackInfo.AllocationBase) >
                   (bytes + STACK_RESERVED_SPACE);
        }

        // We are conservative here. We assume that the platform needs a whole 16 pages to
        // respond to stack overflow (using an x86/x64 page-size, not IA64). That's 64KB,
        // which means that for very small stacks (e.g. 128KB) we'll fail a lot of stack checks
        // incorrectly.
        private const long STACK_RESERVED_SPACE = 4096 * 16;


        [DllImport("kernel32.dll")]
        private static extern int VirtualQuery(IntPtr lpAddress,
                                               ref MEMORY_BASIC_INFORMATION lpBuffer,
                                               int dwLength);

        private struct MEMORY_BASIC_INFORMATION
        {
            internal uint BaseAddress;
            internal uint AllocationBase;
            internal uint AllocationProtect;
            internal uint RegionSize;
            internal uint State;
            internal uint Protect;
            internal uint Type;
        }
    }
}
