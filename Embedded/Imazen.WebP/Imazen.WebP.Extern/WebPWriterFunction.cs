using System;
using System.Runtime.InteropServices;

namespace Imazen.WebP.Extern;

public delegate int WebPWriterFunction([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture);
