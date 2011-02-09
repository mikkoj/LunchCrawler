using System;
using System.Runtime.InteropServices;

namespace LunchCrawler.Common
{
    public static class MimeDetector
    {


        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(
                IntPtr pBC,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
                int cbSize,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
                int dwMimeFlags,
                out IntPtr ppwzMimeOut,
                int dwReserved);

        public static string DetermineMIMEType(string url, byte[] content)
        {
            IntPtr mimeout;
            var result = FindMimeFromData(IntPtr.Zero, url, content, content.Length, null, 0, out mimeout, 0);

            if (result != 0)
            {
                Marshal.FreeCoTaskMem(mimeout);
                return string.Empty;
            }

            string mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);

            return string.IsNullOrEmpty(mime) ? "mime/unknown" : mime.ToLower();
        }

        public static string DetermineMIMEType(string filepath)
        {
            using (System.IO.FileStream input = new System.IO.FileStream(filepath, System.IO.FileMode.Open))
            {
                int MaxContent = (int)input.Length;
                if (MaxContent > 4096) 
                    MaxContent = 4096;

                byte[] buf = new byte[MaxContent];
                input.Read(buf, 0, MaxContent);

                return DetermineMIMEType(filepath, buf);
            }
        }
    }
}
