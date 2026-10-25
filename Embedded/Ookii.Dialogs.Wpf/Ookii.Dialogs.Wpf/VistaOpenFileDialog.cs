using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf.Interop;

namespace Ookii.Dialogs.Wpf;

[Description("Prompts the user to open a file.")]
public sealed class VistaOpenFileDialog : VistaFileDialog
{
	private bool _showReadOnly;

	private bool _readOnlyChecked;

	private const int _openDropDownId = 16386;

	private const int _openItemId = 16387;

	private const int _readOnlyItemId = 16388;

	[DefaultValue(true)]
	[Description("A value indicating whether the dialog box displays a warning if the user specifies a file name that does not exist.")]
	public override bool CheckFileExists
	{
		get
		{
			return base.CheckFileExists;
		}
		set
		{
			base.CheckFileExists = value;
		}
	}

	[Description("A value indicating whether the dialog box allows multiple files to be selected.")]
	[DefaultValue(false)]
	[Category("Behavior")]
	public bool Multiselect
	{
		get
		{
			if (base.DownlevelDialog != null)
			{
				return ((OpenFileDialog)base.DownlevelDialog).Multiselect;
			}
			return GetOption(NativeMethods.FOS.FOS_ALLOWMULTISELECT);
		}
		set
		{
			if (base.DownlevelDialog != null)
			{
				((OpenFileDialog)base.DownlevelDialog).Multiselect = value;
			}
			SetOption(NativeMethods.FOS.FOS_ALLOWMULTISELECT, value);
		}
	}

	[Description("A value indicating whether the dialog box contains a read-only check box.")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ShowReadOnly
	{
		get
		{
			if (base.DownlevelDialog != null)
			{
				return ((OpenFileDialog)base.DownlevelDialog).ShowReadOnly;
			}
			return _showReadOnly;
		}
		set
		{
			if (base.DownlevelDialog != null)
			{
				((OpenFileDialog)base.DownlevelDialog).ShowReadOnly = value;
			}
			else
			{
				_showReadOnly = value;
			}
		}
	}

	[DefaultValue(false)]
	[Description("A value indicating whether the read-only check box is selected.")]
	[Category("Behavior")]
	public bool ReadOnlyChecked
	{
		get
		{
			if (base.DownlevelDialog != null)
			{
				return ((OpenFileDialog)base.DownlevelDialog).ReadOnlyChecked;
			}
			return _readOnlyChecked;
		}
		set
		{
			if (base.DownlevelDialog != null)
			{
				((OpenFileDialog)base.DownlevelDialog).ReadOnlyChecked = value;
			}
			else
			{
				_readOnlyChecked = value;
			}
		}
	}

	public VistaOpenFileDialog()
	{
		if (!VistaFileDialog.IsVistaFileDialogSupported)
		{
			base.DownlevelDialog = new OpenFileDialog();
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (base.DownlevelDialog == null)
		{
			CheckFileExists = true;
			_showReadOnly = false;
			_readOnlyChecked = false;
		}
	}

	public Stream OpenFile()
	{
		if (base.DownlevelDialog != null)
		{
			return ((OpenFileDialog)base.DownlevelDialog).OpenFile();
		}
		return new FileStream(base.FileName, FileMode.Open, FileAccess.Read);
	}

	internal override IFileDialog CreateFileDialog()
	{
		return (NativeFileOpenDialog)new FileOpenDialogRCW();
	}

	internal override void SetDialogProperties(IFileDialog dialog)
	{
		base.SetDialogProperties(dialog);
		if (_showReadOnly)
		{
			IFileDialogCustomize obj = (IFileDialogCustomize)dialog;
			obj.EnableOpenDropDown(16386);
			obj.AddControlItem(16386, 16387, ComDlgResources.LoadString(ComDlgResources.ComDlgResourceId.OpenButton));
			obj.AddControlItem(16386, 16388, ComDlgResources.LoadString(ComDlgResources.ComDlgResourceId.ReadOnly));
		}
	}

	internal override void GetResult(IFileDialog dialog)
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
			base.FileNamesInternal = array;
		}
		else
		{
			base.FileNamesInternal = null;
		}
		if (ShowReadOnly)
		{
			((IFileDialogCustomize)dialog).GetSelectedControlItem(16386, out var pdwIDItem);
			_readOnlyChecked = pdwIDItem == 16388;
		}
		base.GetResult(dialog);
	}
}
