using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using Ookii.Dialogs.Wpf.Interop;

namespace Ookii.Dialogs.Wpf;

internal static class NativeMethods
{
	[Flags]
	public enum LoadLibraryExFlags : uint
	{
		DontResolveDllReferences = 1u,
		LoadLibraryAsDatafile = 2u,
		LoadWithAlteredSearchPath = 8u,
		LoadIgnoreCodeAuthzLevel = 0x10u
	}

	public delegate uint TaskDialogCallback(IntPtr hwnd, uint uNotification, IntPtr wParam, IntPtr lParam, IntPtr dwRefData);

	public enum TaskDialogNotifications
	{
		Created,
		Navigated,
		ButtonClicked,
		HyperlinkClicked,
		Timer,
		Destroyed,
		RadioButtonClicked,
		DialogConstructed,
		VerificationClicked,
		Help,
		ExpandoButtonClicked
	}

	[Flags]
	public enum TaskDialogCommonButtonFlags
	{
		OkButton = 1,
		YesButton = 2,
		NoButton = 4,
		CancelButton = 8,
		RetryButton = 0x10,
		CloseButton = 0x20
	}

	[Flags]
	public enum TaskDialogFlags
	{
		EnableHyperLinks = 1,
		UseHIconMain = 2,
		UseHIconFooter = 4,
		AllowDialogCancellation = 8,
		UseCommandLinks = 0x10,
		UseCommandLinksNoIcon = 0x20,
		ExpandFooterArea = 0x40,
		ExpandedByDefault = 0x80,
		VerificationFlagChecked = 0x100,
		ShowProgressBar = 0x200,
		ShowMarqueeProgressBar = 0x400,
		CallbackTimer = 0x800,
		PositionRelativeToWindow = 0x1000,
		RtlLayout = 0x2000,
		NoDefaultRadioButton = 0x4000,
		CanBeMinimized = 0x8000
	}

	public enum TaskDialogMessages
	{
		NavigatePage = 1125,
		ClickButton = 1126,
		SetMarqueeProgressBar = 1127,
		SetProgressBarState = 1128,
		SetProgressBarRange = 1129,
		SetProgressBarPos = 1130,
		SetProgressBarMarquee = 1131,
		SetElementText = 1132,
		ClickRadioButton = 1134,
		EnableButton = 1135,
		EnableRadioButton = 1136,
		ClickVerification = 1137,
		UpdateElementText = 1138,
		SetButtonElevationRequiredState = 1139,
		UpdateIcon = 1140
	}

	public enum TaskDialogElements
	{
		Content,
		ExpandedInformation,
		Footer,
		MainInstruction
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct TASKDIALOG_BUTTON
	{
		public int nButtonID;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszButtonText;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct TASKDIALOGCONFIG
	{
		public uint cbSize;

		public IntPtr hwndParent;

		public IntPtr hInstance;

		public TaskDialogFlags dwFlags;

		public TaskDialogCommonButtonFlags dwCommonButtons;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszWindowTitle;

		public IntPtr hMainIcon;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszMainInstruction;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszContent;

		public uint cButtons;

		public IntPtr pButtons;

		public int nDefaultButton;

		public uint cRadioButtons;

		public IntPtr pRadioButtons;

		public int nDefaultRadioButton;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszVerificationText;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszExpandedInformation;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszExpandedControlText;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszCollapsedControlText;

		public IntPtr hFooterIcon;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszFooterText;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public TaskDialogCallback pfCallback;

		public IntPtr lpCallbackData;

		public uint cxWidth;
	}

	public struct ACTCTX
	{
		public int cbSize;

		public uint dwFlags;

		public string lpSource;

		public ushort wProcessorArchitecture;

		public ushort wLangId;

		public string lpAssemblyDirectory;

		public string lpResourceName;

		public string lpApplicationName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	internal struct COMDLG_FILTERSPEC
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszName;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszSpec;
	}

	internal enum FDAP
	{
		FDAP_BOTTOM,
		FDAP_TOP
	}

	internal enum FDE_SHAREVIOLATION_RESPONSE
	{
		FDESVR_DEFAULT,
		FDESVR_ACCEPT,
		FDESVR_REFUSE
	}

	internal enum FDE_OVERWRITE_RESPONSE
	{
		FDEOR_DEFAULT,
		FDEOR_ACCEPT,
		FDEOR_REFUSE
	}

	internal enum SIATTRIBFLAGS
	{
		SIATTRIBFLAGS_AND = 1,
		SIATTRIBFLAGS_OR,
		SIATTRIBFLAGS_APPCOMPAT
	}

	internal enum SIGDN : uint
	{
		SIGDN_NORMALDISPLAY = 0u,
		SIGDN_PARENTRELATIVEPARSING = 2147581953u,
		SIGDN_DESKTOPABSOLUTEPARSING = 2147647488u,
		SIGDN_PARENTRELATIVEEDITING = 2147684353u,
		SIGDN_DESKTOPABSOLUTEEDITING = 2147794944u,
		SIGDN_FILESYSPATH = 2147844096u,
		SIGDN_URL = 2147909632u,
		SIGDN_PARENTRELATIVEFORADDRESSBAR = 2147991553u,
		SIGDN_PARENTRELATIVE = 2148007937u
	}

	[Flags]
	internal enum FOS : uint
	{
		FOS_OVERWRITEPROMPT = 2u,
		FOS_STRICTFILETYPES = 4u,
		FOS_NOCHANGEDIR = 8u,
		FOS_PICKFOLDERS = 0x20u,
		FOS_FORCEFILESYSTEM = 0x40u,
		FOS_ALLNONSTORAGEITEMS = 0x80u,
		FOS_NOVALIDATE = 0x100u,
		FOS_ALLOWMULTISELECT = 0x200u,
		FOS_PATHMUSTEXIST = 0x800u,
		FOS_FILEMUSTEXIST = 0x1000u,
		FOS_CREATEPROMPT = 0x2000u,
		FOS_SHAREAWARE = 0x4000u,
		FOS_NOREADONLYRETURN = 0x8000u,
		FOS_NOTESTFILECREATE = 0x10000u,
		FOS_HIDEMRUPLACES = 0x20000u,
		FOS_HIDEPINNEDPLACES = 0x40000u,
		FOS_NODEREFERENCELINKS = 0x100000u,
		FOS_DONTADDTORECENT = 0x2000000u,
		FOS_FORCESHOWHIDDEN = 0x10000000u,
		FOS_DEFAULTNOMINIMODE = 0x20000000u
	}

	internal enum CDCONTROLSTATE
	{
		CDCS_INACTIVE,
		CDCS_ENABLED,
		CDCS_VISIBLE
	}

	internal enum FFFP_MODE
	{
		FFFP_EXACTMATCH,
		FFFP_NEARESTPARENTMATCH
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	internal struct KNOWNFOLDER_DEFINITION
	{
		internal KF_CATEGORY category;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszName;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszCreator;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszDescription;

		internal Guid fidParent;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszRelativePath;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszParsingName;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszToolTip;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszLocalizedName;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszIcon;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string pszSecurity;

		internal uint dwAttributes;

		internal KF_DEFINITION_FLAGS kfdFlags;

		internal Guid ftidType;
	}

	internal enum KF_CATEGORY
	{
		KF_CATEGORY_VIRTUAL = 1,
		KF_CATEGORY_FIXED,
		KF_CATEGORY_COMMON,
		KF_CATEGORY_PERUSER
	}

	[Flags]
	internal enum KF_DEFINITION_FLAGS
	{
		KFDF_PERSONALIZE = 1,
		KFDF_LOCAL_REDIRECT_ONLY = 2,
		KFDF_ROAMABLE = 4
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct PROPERTYKEY
	{
		internal Guid fmtid;

		internal uint pid;
	}

	[Flags]
	public enum FormatMessageFlags
	{
		FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100,
		FORMAT_MESSAGE_IGNORE_INSERTS = 0x200,
		FORMAT_MESSAGE_FROM_STRING = 0x400,
		FORMAT_MESSAGE_FROM_HMODULE = 0x800,
		FORMAT_MESSAGE_FROM_SYSTEM = 0x1000,
		FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000
	}

	[Flags]
	public enum CREDUI_FLAGS
	{
		INCORRECT_PASSWORD = 1,
		DO_NOT_PERSIST = 2,
		REQUEST_ADMINISTRATOR = 4,
		EXCLUDE_CERTIFICATES = 8,
		REQUIRE_CERTIFICATE = 0x10,
		SHOW_SAVE_CHECK_BOX = 0x40,
		ALWAYS_SHOW_UI = 0x80,
		REQUIRE_SMARTCARD = 0x100,
		PASSWORD_ONLY_OK = 0x200,
		VALIDATE_USERNAME = 0x400,
		COMPLETE_USERNAME = 0x800,
		PERSIST = 0x1000,
		SERVER_CREDENTIAL = 0x4000,
		EXPECT_CONFIRMATION = 0x20000,
		GENERIC_CREDENTIALS = 0x40000,
		USERNAME_TARGET_CREDENTIALS = 0x80000,
		KEEP_USERNAME = 0x100000
	}

	[Flags]
	public enum CredUIWinFlags
	{
		Generic = 1,
		Checkbox = 2,
		AutoPackageOnly = 0x10,
		InCredOnly = 0x20,
		EnumerateAdmins = 0x100,
		EnumerateCurrentUser = 0x200,
		SecurePrompt = 0x1000,
		Pack32Wow = 0x10000000
	}

	internal enum CredUIReturnCodes
	{
		NO_ERROR = 0,
		ERROR_CANCELLED = 1223,
		ERROR_NO_SUCH_LOGON_SESSION = 1312,
		ERROR_NOT_FOUND = 1168,
		ERROR_INVALID_ACCOUNT_NAME = 1315,
		ERROR_INSUFFICIENT_BUFFER = 122,
		ERROR_INVALID_PARAMETER = 87,
		ERROR_INVALID_FLAGS = 1004
	}

	internal enum CredTypes
	{
		CRED_TYPE_GENERIC = 1,
		CRED_TYPE_DOMAIN_PASSWORD,
		CRED_TYPE_DOMAIN_CERTIFICATE,
		CRED_TYPE_DOMAIN_VISIBLE_PASSWORD
	}

	internal enum CredPersist
	{
		Session = 1,
		LocalMachine,
		Enterprise
	}

	internal struct CREDUI_INFO
	{
		public int cbSize;

		public IntPtr hwndParent;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszMessageText;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszCaptionText;

		public IntPtr hbmBanner;
	}

	public struct CREDENTIAL
	{
		public int Flags;

		public CredTypes Type;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string TargetName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Comment;

		public long LastWritten;

		public uint CredentialBlobSize;

		public IntPtr CredentialBlob;

		[MarshalAs(UnmanagedType.U4)]
		public CredPersist Persist;

		public int AttributeCount;

		public IntPtr Attributes;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string TargetAlias;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string UserName;
	}

	public enum FolderBrowserDialogMessage
	{
		Initialized = 1,
		SelChanged = 2,
		ValidateFailedA = 3,
		ValidateFailedW = 4,
		EnableOk = 1125,
		SetSelection = 1127
	}

	public delegate int BrowseCallbackProc(IntPtr hwnd, FolderBrowserDialogMessage msg, IntPtr lParam, IntPtr wParam);

	[Flags]
	public enum BrowseInfoFlags
	{
		ReturnOnlyFsDirs = 1,
		DontGoBelowDomain = 2,
		StatusText = 4,
		ReturnFsAncestors = 8,
		EditBox = 0x10,
		Validate = 0x20,
		NewDialogStyle = 0x40,
		UseNewUI = 0x50,
		BrowseIncludeUrls = 0x80,
		UaHint = 0x100,
		NoNewFolderButton = 0x200,
		NoTranslateTargets = 0x400,
		BrowseForComputer = 0x1000,
		BrowseForPrinter = 0x2000,
		BrowseIncludeFiles = 0x4000,
		Shareable = 0x8000,
		BrowseFileJunctions = 0x10000
	}

	public struct BROWSEINFO
	{
		public IntPtr hwndOwner;

		public IntPtr pidlRoot;

		public string pszDisplayName;

		public string lpszTitle;

		public BrowseInfoFlags ulFlags;

		public BrowseCallbackProc lpfn;

		public IntPtr lParam;

		public int iImage;
	}

	public const int ErrorFileNotFound = 2;

	public const int WM_USER = 1024;

	public const int WM_GETICON = 127;

	public const int WM_SETICON = 128;

	public const int ICON_SMALL = 0;

	public const int ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 4;

	internal const int CREDUI_MAX_USERNAME_LENGTH = 513;

	internal const int CREDUI_MAX_PASSWORD_LENGTH = 256;

	public static bool IsWindowsVistaOrLater
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				return Environment.OSVersion.Version >= new Version(6, 0, 6000);
			}
			return false;
		}
	}

	public static bool IsWindowsXPOrLater
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				return Environment.OSVersion.Version >= new Version(5, 1, 2600);
			}
			return false;
		}
	}

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern SafeModuleHandle LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

	[DllImport("kernel32", SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GetCurrentThreadId();

	[DllImport("comctl32.dll", PreserveSig = false)]
	public static extern void TaskDialogIndirect([In] ref TASKDIALOGCONFIG pTaskConfig, out int pnButton, out int pnRadioButton, [MarshalAs(UnmanagedType.Bool)] out bool pfVerificationFlagChecked);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

	[DllImport("Kernel32.dll", SetLastError = true)]
	public static extern ActivationContextSafeHandle CreateActCtx(ref ACTCTX actctx);

	[DllImport("kernel32.dll")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static extern void ReleaseActCtx(IntPtr hActCtx);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ActivateActCtx(ActivationContextSafeHandle hActCtx, out IntPtr lpCookie);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeactivateActCtx(uint dwFlags, IntPtr lpCookie);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	public static IShellItem CreateItemFromParsingName(string path)
	{
		Guid riid = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe");
		object ppv;
		int num = SHCreateItemFromParsingName(path, IntPtr.Zero, ref riid, out ppv);
		if (num != 0)
		{
			throw new Win32Exception(num);
		}
		return (IShellItem)ppv;
	}

	[DllImport("user32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	public static extern int LoadString(SafeModuleHandle hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern uint FormatMessage([MarshalAs(UnmanagedType.U4)] FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, string[] Arguments);

	[DllImport("credui.dll", CharSet = CharSet.Unicode)]
	internal static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO pUiInfo, string targetName, IntPtr Reserved, int dwAuthError, StringBuilder pszUserName, uint ulUserNameMaxChars, StringBuilder pszPassword, uint ulPaswordMaxChars, [In][Out][MarshalAs(UnmanagedType.Bool)] ref bool pfSave, CREDUI_FLAGS dwFlags);

	[DllImport("credui.dll", CharSet = CharSet.Unicode)]
	public static extern CredUIReturnCodes CredUIPromptForWindowsCredentials(ref CREDUI_INFO pUiInfo, uint dwAuthError, ref uint pulAuthPackage, IntPtr pvInAuthBuffer, uint ulInAuthBufferSize, out IntPtr ppvOutAuthBuffer, out uint pulOutAuthBufferSize, [MarshalAs(UnmanagedType.Bool)] ref bool pfSave, CredUIWinFlags dwFlags);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredReadW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CredRead(string TargetName, CredTypes Type, int Flags, out IntPtr Credential);

	[DllImport("advapi32.dll")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal static extern void CredFree(IntPtr Buffer);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredDeleteW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CredDelete(string TargetName, CredTypes Type, int Flags);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredWriteW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CredWrite(ref CREDENTIAL Credential, int Flags);

	[DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CredPackAuthenticationBuffer(uint dwFlags, string pszUserName, string pszPassword, IntPtr pPackedCredentials, ref uint pcbPackedCredentials);

	[DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CredUnPackAuthenticationBuffer(uint dwFlags, IntPtr pAuthBuffer, uint cbAuthBuffer, StringBuilder pszUserName, ref uint pcchMaxUserName, StringBuilder pszDomainName, ref uint pcchMaxDomainName, StringBuilder pszPassword, ref uint pcchMaxPassword);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

	[DllImport("shell32.dll", SetLastError = true)]
	public static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, Environment.SpecialFolder nFolder, ref IntPtr ppidl);

	[DllImport("shell32.dll", PreserveSig = false)]
	public static extern IMalloc SHGetMalloc();

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr SendMessage(IntPtr hWnd, FolderBrowserDialogMessage msg, IntPtr wParam, string lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr SendMessage(IntPtr hWnd, FolderBrowserDialogMessage msg, IntPtr wParam, IntPtr lParam);
}
