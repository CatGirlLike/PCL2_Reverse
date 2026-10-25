using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Imazen.WebP.Extern;

public static class LoadLibrary
{
	private static object lockObj = new object();

	private static Dictionary<string, IntPtr> loaded = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);

	[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, uint dwFlags);

	public static bool EnsureLoadedByPath(string fullPath, bool throwException)
	{
		fullPath = Path.GetFullPath(fullPath);
		lock (lockObj)
		{
			if (loaded.TryGetValue(fullPath, out var value))
			{
				return true;
			}
			value = LoadByPath(fullPath, throwException);
			if (value != IntPtr.Zero)
			{
				loaded.Add(fullPath, value);
				return true;
			}
		}
		return false;
	}

	public static IntPtr LoadByPath(string fullPath, bool throwException)
	{
		IntPtr intPtr = LoadLibraryEx(fullPath, IntPtr.Zero, 2304u);
		if (intPtr == IntPtr.Zero && throwException)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return intPtr;
	}

	public static void LoadWebPOrFail()
	{
		if (!AutoLoadNearby("libwebp.dll", throwFailure: true))
		{
			throw new FileNotFoundException("Failed to locate libwebp.dll");
		}
	}

	public static bool AutoLoadNearby(string name, bool throwFailure)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		return AutoLoad(name, new string[2]
		{
			Path.GetDirectoryName(executingAssembly.Location),
			Path.GetDirectoryName(new Uri(executingAssembly.CodeBase).LocalPath)
		}, throwFailure, throwFailure);
	}

	public static bool AutoLoad(string name, string[] searchFolders, bool throwNotFound, bool throwExceptions)
	{
		string text = "";
		for (int i = 0; i < searchFolders.Length; i++)
		{
			string text2 = Path.Combine(Path.Combine(searchFolders[i], (IntPtr.Size == 8) ? "x64" : "x86"), name);
			if (string.IsNullOrEmpty(Path.GetExtension(text2)))
			{
				text2 += ".dll";
			}
			text = text + "\"" + text2 + "\", ";
			if (File.Exists(text2) && EnsureLoadedByPath(text2, throwExceptions))
			{
				return true;
			}
		}
		if (throwNotFound)
		{
			throw new FileNotFoundException("Failed to locate '" + name + "' as " + text.TrimEnd(' ', ','));
		}
		return false;
	}
}
