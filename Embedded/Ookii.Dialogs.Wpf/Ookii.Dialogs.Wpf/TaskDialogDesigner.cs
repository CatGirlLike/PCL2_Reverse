using System;
using System.ComponentModel.Design;
using Ookii.Dialogs.Wpf.Properties;

namespace Ookii.Dialogs.Wpf;

internal class TaskDialogDesigner : ComponentDesigner
{
	public override DesignerVerbCollection Verbs => new DesignerVerbCollection
	{
		new DesignerVerb(Resources.Preview, Preview)
	};

	private void Preview(object sender, EventArgs e)
	{
		((TaskDialog)base.Component).ShowDialog();
	}
}
