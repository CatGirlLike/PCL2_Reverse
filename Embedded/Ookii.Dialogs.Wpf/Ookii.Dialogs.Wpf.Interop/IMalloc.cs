using System;
using System.Runtime.InteropServices;

namespace Ookii.Dialogs.Wpf.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00000002-0000-0000-C000-000000000046")]
internal interface IMalloc
{
	[PreserveSig]
	IntPtr Alloc([In] uint cb);

	[PreserveSig]
	IntPtr Realloc([In] IntPtr pv, [In] uint cb);

	[PreserveSig]
	void Free([In] IntPtr pv);

	[PreserveSig]
	uint GetSize([In] IntPtr pv);

	[PreserveSig]
	short DidAlloc([In] IntPtr pv);

	[PreserveSig]
	void HeapMinimize();
}
