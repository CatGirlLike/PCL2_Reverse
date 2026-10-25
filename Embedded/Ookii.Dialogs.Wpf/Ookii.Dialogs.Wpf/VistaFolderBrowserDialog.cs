using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Ookii.Dialogs.Wpf.Interop;
using Ookii.Dialogs.Wpf.Properties;

namespace Ookii.Dialogs.Wpf;

[DefaultEvent("HelpRequest")]
[Designer("System.Windows.Forms.Design.FolderBrowserDialogDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[DefaultProperty("SelectedPath")]
[Description("Prompts the user to select a folder.")]
public sealed class VistaFolderBrowserDialog
{
	private string _description;

	private string _selectedPath;

	private NativeMethods.FOS _options;

	private string[] _selectedPaths;

	[Browsable(false)]
	public static bool IsVistaFolderDialogSupported => NativeMethods.IsWindowsVistaOrLater;

	[Category("Folder Browsing")]
	[DefaultValue("")]
	[Localizable(true)]
	[Browsable(true)]
	[Description("The descriptive text displayed above the tree view control in the dialog box, or below the list view control in the Vista style dialog.")]
	public string Description
	{
		get
		{
			return _description ?? string.Empty;
		}
		set
		{
			_description = value;
		}
	}

	[Localizable(false)]
	[Description("The root folder where the browsing starts from. This property has no effect if the Vista style dialog is used.")]
	[Category("Folder Browsing")]
	[Browsable(true)]
	[DefaultValue(typeof(Environment.SpecialFolder), "Desktop")]
	public Environment.SpecialFolder RootFolder { get; set; }

	[Browsable(true)]
	[Editor("System.Windows.Forms.Design.SelectedPathEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	[Description("The path selected by the user.")]
	[DefaultValue("")]
	[Localizable(true)]
	[Category("Folder Browsing")]
	public string SelectedPath
	{
		get
		{
			return _selectedPath ?? _selectedPaths?.FirstOrDefault() ?? string.Empty;
		}
		set
		{
			_selectedPath = value;
		}
	}

	[Browsable(true)]
	[Localizable(false)]
	[Description("A value indicating whether the New Folder button appears in the folder browser dialog box. This property has no effect if the Vista style dialog is used; in that case, the New Folder button is always shown.")]
	[DefaultValue(true)]
	[Category("Folder Browsing")]
	public bool ShowNewFolderButton { get; set; }

	[Category("Folder Browsing")]
	[DefaultValue(false)]
	[Description("A value that indicates whether to use the value of the Description property as the dialog title for Vista style dialogs. This property has no effect on old style dialogs.")]
	public bool UseDescriptionForTitle { get; set; }

	[Description("A value indicating whether the dialog box allows multiple folders to be selected.")]
	[DefaultValue(false)]
	[Category("Behavior")]
	public bool Multiselect
	{
		get
		{
			return HasOption(NativeMethods.FOS.FOS_ALLOWMULTISELECT);
		}
		set
		{
			SetOption(NativeMethods.FOS.FOS_ALLOWMULTISELECT, value);
		}
	}

	[Description("The folder path of all selected folder in the dialog box.")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string[] SelectedPaths
	{
		get
		{
			string[] selectedPaths = _selectedPaths;
			if (selectedPaths == null)
			{
				string selectedPath = _selectedPath;
				if (string.IsNullOrWhiteSpace(selectedPath))
				{
					return new string[0];
				}
				return new string[1] { selectedPath };
			}
			return (string[])selectedPaths.Clone();
		}
		set
		{
			_selectedPaths = value;
		}
	}

	public VistaFolderBrowserDialog()
	{
		Reset();
	}

	public void Reset()
	{
		_description = string.Empty;
		UseDescriptionForTitle = false;
		_selectedPath = string.Empty;
		RootFolder = Environment.SpecialFolder.Desktop;
		ShowNewFolderButton = true;
		_selectedPaths = null;
		_options = (NativeMethods.FOS)0u;
	}

	public bool? ShowDialog()
	{
		return ShowDialog(null);
	}

	public bool? ShowDialog(Window owner)
	{
		IntPtr owner2 = ((owner == null) ? NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle);
		return ShowDialog(owner2);
	}

	public bool? ShowDialog(IntPtr owner)
	{
		IntPtr owner2 = ((owner == (IntPtr)0) ? NativeMethods.GetActiveWindow() : owner);
		return IsVistaFolderDialogSupported ? RunDialog(owner2) : RunDialogDownlevel(owner2);
	}

	internal void SetOption(NativeMethods.FOS option, bool value)
	{
		if (value)
		{
			_options |= option;
		}
		else
		{
			_options &= ~option;
		}
	}

	internal bool HasOption(NativeMethods.FOS option)
	{
		return (_options & option) != 0;
	}

	private bool RunDialog(IntPtr owner)
	{
		IFileDialog fileDialog = null;
		try
		{
			fileDialog = (NativeFileOpenDialog)new FileOpenDialogRCW();
			SetDialogProperties(fileDialog);
			int num = fileDialog.Show(owner);
			if (num < 0)
			{
				if (num == -2147023673)
				{
					return false;
				}
				throw Marshal.GetExceptionForHR(num);
			}
			GetResult(fileDialog);
			return true;
		}
		finally
		{
			if (fileDialog != null)
			{
				Marshal.FinalReleaseComObject(fileDialog);
			}
		}
	}

	private bool RunDialogDownlevel(IntPtr owner)
	{
		IntPtr ppidl = IntPtr.Zero;
		IntPtr intPtr = IntPtr.Zero;
		if (NativeMethods.SHGetSpecialFolderLocation(owner, RootFolder, ref ppidl) != 0 && NativeMethods.SHGetSpecialFolderLocation(owner, Environment.SpecialFolder.Desktop, ref ppidl) != 0)
		{
			throw new InvalidOperationException(Resources.FolderBrowserDialogNoRootFolder);
		}
		try
		{
			NativeMethods.BROWSEINFO lpbi = default(NativeMethods.BROWSEINFO);
			lpbi.hwndOwner = owner;
			lpbi.lpfn = BrowseCallbackProc;
			lpbi.lpszTitle = Description;
			lpbi.pidlRoot = ppidl;
			lpbi.pszDisplayName = new string('\0', 260);
			lpbi.ulFlags = NativeMethods.BrowseInfoFlags.ReturnOnlyFsDirs | NativeMethods.BrowseInfoFlags.NewDialogStyle;
			if (!ShowNewFolderButton)
			{
				lpbi.ulFlags |= NativeMethods.BrowseInfoFlags.NoNewFolderButton;
			}
			intPtr = NativeMethods.SHBrowseForFolder(ref lpbi);
			if (intPtr != IntPtr.Zero)
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				NativeMethods.SHGetPathFromIDList(intPtr, stringBuilder);
				SelectedPath = stringBuilder.ToString();
				return true;
			}
			return false;
		}
		finally
		{
			if (ppidl != IntPtr.Zero)
			{
				IMalloc malloc = NativeMethods.SHGetMalloc();
				malloc.Free(ppidl);
				Marshal.ReleaseComObject(malloc);
			}
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(intPtr);
			}
		}
	}

	private void SetDialogProperties(IFileDialog dialog)
	{
		if (!string.IsNullOrEmpty(_description))
		{
			if (UseDescriptionForTitle)
			{
				dialog.SetTitle(_description);
			}
			else
			{
				((IFileDialogCustomize)dialog).AddText(0, _description);
			}
		}
		dialog.SetOptions(NativeMethods.FOS.FOS_PICKFOLDERS | NativeMethods.FOS.FOS_FORCEFILESYSTEM | NativeMethods.FOS.FOS_FILEMUSTEXIST | _options);
		if (!string.IsNullOrEmpty(_selectedPath))
		{
			string directoryName = Path.GetDirectoryName(_selectedPath);
			if (directoryName == null || !Directory.Exists(directoryName))
			{
				dialog.SetFileName(_selectedPath);
				return;
			}
			string fileName = Path.GetFileName(_selectedPath);
			dialog.SetFolder(NativeMethods.CreateItemFromParsingName(directoryName));
			dialog.SetFileName(fileName);
		}
	}

	private void GetResult(IFileDialog dialog)
	{
		if (Multiselect)
		{
			((IFileOpenDialog)dialog).GetResults(out var ppenum);
			ppenum.GetCount(out var pdwNumItems);
			string[] array = new string[pdwNumItems];
			for (uint num = 0u; num < pdwNumItems; num++)
			{
				ppenum.GetItemAt(num, out var ppsi);
				ppsi.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out var ppszName);
				array[num] = ppszName;
			}
			SelectedPaths = array;
		}
		else
		{
			dialog.GetResult(out var ppsi2);
			ppsi2.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out _selectedPath);
		}
	}

	private int BrowseCallbackProc(IntPtr hwnd, NativeMethods.FolderBrowserDialogMessage msg, IntPtr lParam, IntPtr wParam)
	{
		switch (msg)
		{
		case NativeMethods.FolderBrowserDialogMessage.Initialized:
			if (SelectedPath.Length != 0)
			{
				NativeMethods.SendMessage(hwnd, NativeMethods.FolderBrowserDialogMessage.SetSelection, new IntPtr(1), SelectedPath);
			}
			break;
		case NativeMethods.FolderBrowserDialogMessage.SelChanged:
			if (lParam != IntPtr.Zero)
			{
				StringBuilder pszPath = new StringBuilder(260);
				bool flag = NativeMethods.SHGetPathFromIDList(lParam, pszPath);
				NativeMethods.SendMessage(hwnd, NativeMethods.FolderBrowserDialogMessage.EnableOk, IntPtr.Zero, flag ? new IntPtr(1) : IntPtr.Zero);
			}
			break;
		}
		return 0;
	}
}
