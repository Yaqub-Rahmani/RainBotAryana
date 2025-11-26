using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Fingerprint
{
    internal static class ZKInterop
    {
        // örnek: init / exit
        [DllImport("libzkfp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zkfp_init();

        [DllImport("libzkfp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zkfp_exit();

        // örnek: cihaz sayısı
        [DllImport("libzkfp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zkfp_get_device();

        // örnek: capture fonksiyonu (placeholder)
        // Gerçekte SDK bir handle ve buffer ister. Bu sadece örnek.
        [DllImport("libzkfp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zkfp_capture(out IntPtr data, out int len, int timeout);

        // Eğer SDK template oluşturma fonksiyonu veriyorsa:
        [DllImport("libzkfp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zkfp_img_to_template(IntPtr img, int width, int height, byte[] templateBuffer, ref int templateLength);

        // vb...
    }
}
