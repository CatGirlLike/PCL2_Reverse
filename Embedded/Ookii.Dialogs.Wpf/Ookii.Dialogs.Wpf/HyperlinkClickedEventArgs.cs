using System;

namespace Ookii.Dialogs.Wpf;

public class HyperlinkClickedEventArgs : EventArgs
{
	private string _href;

	public string Href => _href;

	public HyperlinkClickedEventArgs(string href)
	{
		_href = href;
	}
}
