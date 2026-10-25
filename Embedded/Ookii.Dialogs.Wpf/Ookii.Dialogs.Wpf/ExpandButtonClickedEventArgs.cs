using System;

namespace Ookii.Dialogs.Wpf;

public class ExpandButtonClickedEventArgs : EventArgs
{
	private bool _expanded;

	public bool Expanded => _expanded;

	public ExpandButtonClickedEventArgs(bool expanded)
	{
		_expanded = expanded;
	}
}
