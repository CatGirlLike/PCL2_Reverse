using System.ComponentModel;
using System.Threading;

namespace Ookii.Dialogs.Wpf;

public class ProgressDialogDoWorkEventArgs : DoWorkEventArgs
{
	public CancellationToken CancellationToken { get; }

	public ProgressDialogDoWorkEventArgs(object argument, CancellationToken cancellationToken)
		: base(argument)
	{
		CancellationToken = cancellationToken;
	}
}
