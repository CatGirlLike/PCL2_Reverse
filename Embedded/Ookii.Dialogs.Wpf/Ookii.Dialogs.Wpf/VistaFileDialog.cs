using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf.Interop;
using Ookii.Dialogs.Wpf.Properties;

namespace Ookii.Dialogs.Wpf;

[DefaultEvent("FileOk")]
[DefaultProperty("FileName")]
public abstract class VistaFileDialog
{
	internal const int HelpButtonId = 16385;

	private FileDialog _downlevelDialog;

	private NativeMethods.FOS _options;

	private string _filter;

	private int _filterIndex;

	private string[] _fileNames;

	private string _defaultExt;

	private bool _addExtension;

	private string _initialDirectory;

	private string _title;

	private Window _owner;

	[Browsable(false)]
	public static bool IsVistaFileDialogSupported => NativeMethods.IsWindowsVistaOrLater;

	[Description("A value indicating whether the dialog box automatically adds an extension to a file name if the user omits the extension.")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool AddExtension
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.AddExtension;
			}
			return _addExtension;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.AddExtension = value;
			}
			else
			{
				_addExtension = value;
			}
		}
	}

	[Description("A value indicating whether the dialog box displays a warning if the user specifies a file name that does not exist.")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public virtual bool CheckFileExists
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.CheckFileExists;
			}
			return GetOption(NativeMethods.FOS.FOS_FILEMUSTEXIST);
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.CheckFileExists = value;
			}
			else
			{
				SetOption(NativeMethods.FOS.FOS_FILEMUSTEXIST, value);
			}
		}
	}

	[Description("A value indicating whether the dialog box displays a warning if the user specifies a path that does not exist.")]
	[DefaultValue(true)]
	[Category("Behavior")]
	public bool CheckPathExists
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.CheckPathExists;
			}
			return GetOption(NativeMethods.FOS.FOS_PATHMUSTEXIST);
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.CheckPathExists = value;
			}
			else
			{
				SetOption(NativeMethods.FOS.FOS_PATHMUSTEXIST, value);
			}
		}
	}

	[Category("Behavior")]
	[DefaultValue("")]
	[Description("The default file name extension.")]
	public string DefaultExt
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.DefaultExt;
			}
			return _defaultExt ?? string.Empty;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.DefaultExt = value;
				return;
			}
			if (value != null)
			{
				if (value.StartsWith(".", StringComparison.CurrentCulture))
				{
					value = value.Substring(1);
				}
				else if (value.Length == 0)
				{
					value = null;
				}
			}
			_defaultExt = value;
		}
	}

	[Category("Behavior")]
	[Description("A value indicating whether the dialog box returns the location of the file referenced by the shortcut or whether it returns the location of the shortcut (.lnk).")]
	[DefaultValue(true)]
	public bool DereferenceLinks
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.DereferenceLinks;
			}
			return !GetOption(NativeMethods.FOS.FOS_NODEREFERENCELINKS);
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.DereferenceLinks = value;
			}
			else
			{
				SetOption(NativeMethods.FOS.FOS_NODEREFERENCELINKS, !value);
			}
		}
	}

	[DefaultValue("")]
	[Category("Data")]
	[Description("A string containing the file name selected in the file dialog box.")]
	public string FileName
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.FileName;
			}
			if (_fileNames == null || _fileNames.Length == 0 || string.IsNullOrEmpty(_fileNames[0]))
			{
				return string.Empty;
			}
			return _fileNames[0];
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.FileName = value;
			}
			_fileNames = new string[1];
			_fileNames[0] = value;
		}
	}

	[Description("The file names of all selected files in the dialog box.")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string[] FileNames
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.FileNames;
			}
			return FileNamesInternal;
		}
	}

	[Description("The current file name filter string, which determines the choices that appear in the \"Save as file type\" or \"Files of type\" box in the dialog box.")]
	[Category("Behavior")]
	[Localizable(true)]
	[DefaultValue("")]
	public string Filter
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.Filter;
			}
			return _filter;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.Filter = value;
			}
			else
			{
				if (!(value != _filter))
				{
					return;
				}
				if (!string.IsNullOrEmpty(value))
				{
					string[] array = value.Split('|');
					if (array == null || array.Length % 2 != 0)
					{
						throw new ArgumentException(Resources.InvalidFilterString);
					}
				}
				else
				{
					value = null;
				}
				_filter = value;
			}
		}
	}

	[Description("The index of the filter currently selected in the file dialog box.")]
	[Category("Behavior")]
	[DefaultValue(1)]
	public int FilterIndex
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.FilterIndex;
			}
			return _filterIndex;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.FilterIndex = value;
			}
			else
			{
				_filterIndex = value;
			}
		}
	}

	[Description("The initial directory displayed by the file dialog box.")]
	[DefaultValue("")]
	[Category("Data")]
	public string InitialDirectory
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.InitialDirectory;
			}
			if (_initialDirectory != null)
			{
				return _initialDirectory;
			}
			return string.Empty;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.InitialDirectory = value;
			}
			else
			{
				_initialDirectory = value;
			}
		}
	}

	[DefaultValue(false)]
	[Description("A value indicating whether the dialog box restores the current directory before closing.")]
	[Category("Behavior")]
	public bool RestoreDirectory
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.RestoreDirectory;
			}
			return GetOption(NativeMethods.FOS.FOS_NOCHANGEDIR);
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.RestoreDirectory = value;
			}
			else
			{
				SetOption(NativeMethods.FOS.FOS_NOCHANGEDIR, value);
			}
		}
	}

	[Description("The file dialog box title.")]
	[Category("Appearance")]
	[DefaultValue("")]
	[Localizable(true)]
	public string Title
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.Title;
			}
			if (_title != null)
			{
				return _title;
			}
			return string.Empty;
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.Title = value;
			}
			else
			{
				_title = value;
			}
		}
	}

	[DefaultValue(true)]
	[Category("Behavior")]
	[Description("A value indicating whether the dialog box accepts only valid Win32 file names.")]
	public bool ValidateNames
	{
		get
		{
			if (DownlevelDialog != null)
			{
				return DownlevelDialog.ValidateNames;
			}
			return !GetOption(NativeMethods.FOS.FOS_NOVALIDATE);
		}
		set
		{
			if (DownlevelDialog != null)
			{
				DownlevelDialog.ValidateNames = value;
			}
			else
			{
				SetOption(NativeMethods.FOS.FOS_NOVALIDATE, !value);
			}
		}
	}

	[Browsable(false)]
	protected FileDialog DownlevelDialog
	{
		get
		{
			return _downlevelDialog;
		}
		set
		{
			_downlevelDialog = value;
			if (value != null)
			{
				value.FileOk += DownlevelDialog_FileOk;
			}
		}
	}

	internal string[] FileNamesInternal
	{
		private get
		{
			if (_fileNames == null)
			{
				return new string[0];
			}
			return (string[])_fileNames.Clone();
		}
		set
		{
			_fileNames = value;
		}
	}

	[Description("Event raised when the user clicks on the Open or Save button on a file dialog box.")]
	[Category("Action")]
	public event CancelEventHandler FileOk;

	protected VistaFileDialog()
	{
		Reset();
	}

	public virtual void Reset()
	{
		if (DownlevelDialog != null)
		{
			DownlevelDialog.Reset();
			return;
		}
		_fileNames = null;
		_filter = null;
		_filterIndex = 1;
		_addExtension = true;
		_defaultExt = null;
		_options = (NativeMethods.FOS)0u;
		_title = null;
		CheckPathExists = true;
	}

	public bool? ShowDialog()
	{
		return ShowDialog(null);
	}

	public bool? ShowDialog(Window owner)
	{
		_owner = owner;
		if (DownlevelDialog != null)
		{
			return DownlevelDialog.ShowDialog(owner);
		}
		IntPtr owner2 = ((owner == null) ? NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle);
		return ShowDialog(owner2);
	}

	public bool? ShowDialog(IntPtr owner)
	{
		IntPtr hwndOwner = ((owner == (IntPtr)0) ? NativeMethods.GetActiveWindow() : owner);
		return RunFileDialog(hwndOwner);
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

	internal bool GetOption(NativeMethods.FOS option)
	{
		return (_options & option) != 0;
	}

	internal virtual void GetResult(IFileDialog dialog)
	{
		if (!GetOption(NativeMethods.FOS.FOS_ALLOWMULTISELECT))
		{
			_fileNames = new string[1];
			dialog.GetResult(out var ppsi);
			ppsi.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out _fileNames[0]);
		}
	}

	protected virtual void OnFileOk(CancelEventArgs e)
	{
		this.FileOk?.Invoke(this, e);
	}

	internal bool PromptUser(string text, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult)
	{
		string caption = ((!string.IsNullOrEmpty(_title)) ? _title : ((this is VistaOpenFileDialog) ? ComDlgResources.LoadString(ComDlgResources.ComDlgResourceId.Open) : ComDlgResources.LoadString(ComDlgResources.ComDlgResourceId.ConfirmSaveAs)));
		MessageBoxOptions messageBoxOptions = MessageBoxOptions.None;
		if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
		{
			messageBoxOptions |= MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
		}
		return MessageBox.Show(_owner, text, caption, buttons, icon, defaultResult, messageBoxOptions) == MessageBoxResult.Yes;
	}

	internal virtual void SetDialogProperties(IFileDialog dialog)
	{
		dialog.Advise(new VistaFileDialogEvents(this), out var _);
		if (_fileNames != null && _fileNames.Length != 0 && !string.IsNullOrEmpty(_fileNames[0]))
		{
			string directoryName = Path.GetDirectoryName(_fileNames[0]);
			if (directoryName == null || !Directory.Exists(directoryName))
			{
				dialog.SetFileName(_fileNames[0]);
			}
			else
			{
				string fileName = Path.GetFileName(_fileNames[0]);
				dialog.SetFolder(NativeMethods.CreateItemFromParsingName(directoryName));
				dialog.SetFileName(fileName);
			}
		}
		if (!string.IsNullOrEmpty(_filter))
		{
			string[] array = _filter.Split('|');
			NativeMethods.COMDLG_FILTERSPEC[] array2 = new NativeMethods.COMDLG_FILTERSPEC[array.Length / 2];
			for (int i = 0; i < array.Length; i += 2)
			{
				array2[i / 2].pszName = array[i];
				array2[i / 2].pszSpec = array[i + 1];
			}
			dialog.SetFileTypes((uint)array2.Length, array2);
			if (_filterIndex > 0 && _filterIndex <= array2.Length)
			{
				dialog.SetFileTypeIndex((uint)_filterIndex);
			}
		}
		if (_addExtension && !string.IsNullOrEmpty(_defaultExt))
		{
			dialog.SetDefaultExtension(_defaultExt);
		}
		if (!string.IsNullOrEmpty(_initialDirectory))
		{
			IShellItem defaultFolder = NativeMethods.CreateItemFromParsingName(_initialDirectory);
			dialog.SetDefaultFolder(defaultFolder);
		}
		if (!string.IsNullOrEmpty(_title))
		{
			dialog.SetTitle(_title);
		}
		dialog.SetOptions(_options | NativeMethods.FOS.FOS_FORCEFILESYSTEM);
	}

	internal abstract IFileDialog CreateFileDialog();

	internal bool DoFileOk(IFileDialog dialog)
	{
		GetResult(dialog);
		CancelEventArgs cancelEventArgs = new CancelEventArgs();
		OnFileOk(cancelEventArgs);
		return !cancelEventArgs.Cancel;
	}

	private bool RunFileDialog(IntPtr hwndOwner)
	{
		IFileDialog fileDialog = null;
		try
		{
			fileDialog = CreateFileDialog();
			SetDialogProperties(fileDialog);
			int num = fileDialog.Show(hwndOwner);
			if (num < 0)
			{
				if (num == -2147023673)
				{
					return false;
				}
				throw Marshal.GetExceptionForHR(num);
			}
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

	private void DownlevelDialog_FileOk(object sender, CancelEventArgs e)
	{
		OnFileOk(e);
	}
}
