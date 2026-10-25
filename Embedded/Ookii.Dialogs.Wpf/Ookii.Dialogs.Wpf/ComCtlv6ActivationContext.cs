using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ookii.Dialogs.Wpf;

internal sealed class ComCtlv6ActivationContext : IDisposable
{
	private IntPtr _cookie;

	private static NativeMethods.ACTCTX _enableThemingActivationContext;

	private static ActivationContextSafeHandle _activationContext;

	private static bool _contextCreationSucceeded;

	private static readonly object _contextCreationLock = new object();

	public ComCtlv6ActivationContext(bool enable)
	{
		if (enable && NativeMethods.IsWindowsXPOrLater && EnsureActivateContextCreated() && !NativeMethods.ActivateActCtx(_activationContext, out _cookie))
		{
			_cookie = IntPtr.Zero;
		}
	}

	~ComCtlv6ActivationContext()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_cookie != IntPtr.Zero && NativeMethods.DeactivateActCtx(0u, _cookie))
		{
			_cookie = IntPtr.Zero;
		}
	}

	private static bool EnsureActivateContextCreated()
	{
		lock (_contextCreationLock)
		{
			if (_contextCreationSucceeded)
			{
				return _contextCreationSucceeded;
			}
			string text;
			using (Stream stream = typeof(ComCtlv6ActivationContext).Assembly.GetManifestResourceStream("Ookii.Dialogs.XPThemes.manifest"))
			{
				if (stream == null)
				{
					throw new InvalidOperationException("Unable to retrieve Ookii.Dialogs.XPThemes.manifest embedded resource");
				}
				text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				using FileStream destination = new FileStream(text, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
				stream.CopyTo(destination);
			}
			NativeMethods.ACTCTX enableThemingActivationContext = default(NativeMethods.ACTCTX);
			enableThemingActivationContext.cbSize = Marshal.SizeOf(typeof(NativeMethods.ACTCTX));
			enableThemingActivationContext.lpSource = text;
			_enableThemingActivationContext = enableThemingActivationContext;
			_activationContext = NativeMethods.CreateActCtx(ref _enableThemingActivationContext);
			_contextCreationSucceeded = !_activationContext.IsInvalid;
			try
			{
				File.Delete(text);
			}
			catch (Exception)
			{
			}
			return _contextCreationSucceeded;
		}
	}
}
