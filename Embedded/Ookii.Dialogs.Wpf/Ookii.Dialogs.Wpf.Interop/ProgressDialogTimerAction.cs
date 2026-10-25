using System;

namespace Ookii.Dialogs.Wpf.Interop;

[Flags]
internal enum ProgressDialogTimerAction : uint
{
	Reset = 1u,
	Pause = 2u,
	Resume = 3u
}
