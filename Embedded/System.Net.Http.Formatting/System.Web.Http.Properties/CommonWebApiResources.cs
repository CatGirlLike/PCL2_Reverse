using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace System.Web.Http.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CommonWebApiResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				Assembly assembly = typeof(CommonWebApiResources).Assembly;
				string text = (from s in assembly.GetManifestResourceNames()
					where s.EndsWith("CommonWebApiResources.resources", StringComparison.OrdinalIgnoreCase)
					select s).Single();
				text = text.Substring(0, text.Length - 10);
				resourceMan = new ResourceManager(text, assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string ArgumentInvalidAbsoluteUri => ResourceManager.GetString("ArgumentInvalidAbsoluteUri", resourceCulture);

	internal static string ArgumentInvalidHttpUriScheme => ResourceManager.GetString("ArgumentInvalidHttpUriScheme", resourceCulture);

	internal static string ArgumentMustBeGreaterThanOrEqualTo => ResourceManager.GetString("ArgumentMustBeGreaterThanOrEqualTo", resourceCulture);

	internal static string ArgumentMustBeLessThanOrEqualTo => ResourceManager.GetString("ArgumentMustBeLessThanOrEqualTo", resourceCulture);

	internal static string ArgumentNullOrEmpty => ResourceManager.GetString("ArgumentNullOrEmpty", resourceCulture);

	internal static string ArgumentUriHasQueryOrFragment => ResourceManager.GetString("ArgumentUriHasQueryOrFragment", resourceCulture);

	internal static string InvalidEnumArgument => ResourceManager.GetString("InvalidEnumArgument", resourceCulture);

	internal CommonWebApiResources()
	{
	}
}
