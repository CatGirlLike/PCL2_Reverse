using System.ComponentModel;

namespace Ookii.Dialogs.Wpf;

public class TaskDialogItemClickedEventArgs : CancelEventArgs
{
	private readonly TaskDialogItem _item;

	public TaskDialogItem Item => _item;

	public TaskDialogItemClickedEventArgs(TaskDialogItem item)
	{
		_item = item;
	}
}
