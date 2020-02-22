using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

class AutoImageCompresser {
	const string APPLICATION_NAME = "AutoImageCompresser";

	[DllImport("kernel32.dll")]
	static extern int GetPrivateProfileString(
		string lpApplicationname,
		string lpKeyName,
		string lpDefault,
		StringBuilder lpReturnedstring,
		int nSize,
		string lpFileName
	);

	[DllImport("kernel32.dll")]
	static extern int GetPrivateProfileSectionNames(
		IntPtr lpszReturnBuffer,
		uint nSize,
		string lpFileName
	);

	static string getExeDir() {
		return Path.GetDirectoryName(
			Assembly.GetExecutingAssembly().Location
		);
	}

	static bool getBoolByString(string str, bool defaultValue) {
		return (str == (!defaultValue ? "true" : "false")) ? !defaultValue : defaultValue;
	}

	static void createShortcut(string configName) {
		var t = Type.GetTypeFromCLSID(
			new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")
		);

		dynamic shell = Activator.CreateInstance(t);

		var shortcut = shell.CreateShortcut(
			Path.Combine(
				getExeDir(),
				string.Format("{0} - {1}.lnk", APPLICATION_NAME, configName)
			)
		);

		shortcut.TargetPath = Assembly.GetExecutingAssembly().Location;
		shortcut.Arguments = "\"" + configName + "\"";
		shortcut.Save();

		Marshal.FinalReleaseComObject(shortcut);
		Marshal.FinalReleaseComObject(shell);
	}

	static string getIniValue(
		string path,
		string section,
		string key,
		string defaultValue = ""
	) {
		var sb = new StringBuilder(1024);

		GetPrivateProfileString(
			section,
			key,
			defaultValue,
			sb,
			sb.Capacity,
			path
		);

		return sb.ToString();
	}

	static string[] getIniSections(
		string path
	) {
		var sectionNamesOrig = new string('\0', 1024);
		IntPtr sectionNames = Marshal.StringToHGlobalAnsi(sectionNamesOrig);

		int length = GetPrivateProfileSectionNames(
			sectionNames,
			(uint) sectionNamesOrig.Length,
			path
		);

		if (length <= 0)
			return new string[0];

		return Marshal.PtrToStringAnsi(sectionNames, length).TrimEnd('\0').Split('\0');
	}

	static void Main(string[] Args) {
		var iniPath = Path.Combine(getExeDir(), "configure.ini");


		if (Args.Length == 0) {
			var sections = getIniSections(iniPath);
			foreach (var section in sections)
				createShortcut(section);
		} else if (Args.Length == 1) {
			MessageBox.Show("Observe...");

		} else {
			MessageBox.Show("Too many arguments");
			
		}

	}
}

