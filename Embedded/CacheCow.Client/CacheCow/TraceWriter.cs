#define TRACE
using System;
using System.Diagnostics;

namespace CacheCow;

internal static class TraceWriter
{
	public const string CacheCowTraceSwitch = "CacheCow";

	public const string CacheCowTracingEnvVarName = "CacheCow.Tracing.Switch";

	internal static readonly TraceSwitch _switch;

	static TraceWriter()
	{
		_switch = new TraceSwitch("CacheCow", "CacheCow Trace Switch");
		ExamineEnvVar();
	}

	private static void ExamineEnvVar()
	{
		string text = Environment.GetEnvironmentVariable("CacheCow.Tracing.Switch") ?? "";
		if (text.Length > 0 && Enum.TryParse<TraceLevel>(text, out var result))
		{
			_switch.Level = result;
		}
	}

	public static void WriteLine(string message, TraceLevel level, params object[] args)
	{
		if (_switch.Level >= level)
		{
			string arg = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff");
			string arg2 = string.Empty;
			try
			{
				arg2 = new StackFrame(1).GetMethod().Name;
			}
			catch
			{
			}
			Trace.WriteLine($"{arg} - {arg2}: {((args.Length == 0) ? message : string.Format(message, args))}");
		}
	}
}
