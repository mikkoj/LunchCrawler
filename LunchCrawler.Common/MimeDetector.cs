using System;
using System.Runtime.InteropServices;

namespace LunchCrawler.Common
{
    public static class MimeDetector
    {
        // ReSharper disable InconsistentNaming
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(
            IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);
        // ReSharper restore InconsistentNaming


        /// <summary>
        /// Determines an appropriate MIME type from binary data.
        /// </summary>
        public static string DetermineMIMEType(byte[] content)
        {
            IntPtr mimeout;
            var result = FindMimeFromData(IntPtr.Zero, null, content, content.Length, null, 0, out mimeout, 0);

            if (result != 0)
            {
                Marshal.FreeCoTaskMem(mimeout);
                return string.Empty;
            }

            var mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);

            return string.IsNullOrEmpty(mime) ? "mime/unknown" : mime.ToLower();
        }
    }
}
