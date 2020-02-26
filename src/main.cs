using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program {
	public const string APPLICATION_NAME = "AutoImageCompresser";

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
		return (
			str == (!defaultValue ? "true" : "false")
		) ? !defaultValue : defaultValue;
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

		return Marshal.PtrToStringAnsi(
			sectionNames,
			length
		).TrimEnd('\0').Split('\0');
	}

	static void createSectionsShortcut(string iniPath) {
		var createdCount = 0;

		foreach (var section in getIniSections(iniPath)) {
			createShortcut(section);
			++ createdCount;
		}

		MessageBox.Show(
			string.Format("{0} shortcut(s) created", createdCount),
			Program.APPLICATION_NAME,
			MessageBoxButtons.OK,
			MessageBoxIcon.Information
		);

	}

	static ImageCodecInfo getImageCodecInfo(Guid formatId) {
		foreach (var ici in ImageCodecInfo.GetImageEncoders())
			if (ici.FormatID == formatId)
				return ici;

		return null;
	}

	static void observeSection(string iniPath, string section) {
		var uintRegex = new Regex(
			@"\A\d+\z",
			RegexOptions.Compiled
		);

		if (!getIniSections(iniPath).Contains(section)) {
			MessageBox.Show(
				string.Format("This sections is deleted: {0}", section),
				Program.APPLICATION_NAME,
				MessageBoxButtons.OK,
				MessageBoxIcon.Hand
			);

			return;
		}

		var pattern = getIniValue(iniPath, section, "pattern", "*");

		var inDir = getIniValue(iniPath, section, "input_dir", string.Empty);

		if (inDir == string.Empty) {
			MessageBox.Show(
				string.Format(
					"input_dir is not defined (section: {0})",
					section
				),
				Program.APPLICATION_NAME,
				MessageBoxButtons.OK,
				MessageBoxIcon.Hand
			);

			return;
		}

		inDir = Environment.ExpandEnvironmentVariables(inDir);

		if (!Path.IsPathRooted(inDir))
			inDir = Path.Combine(getExeDir(), inDir);

		if (!Directory.Exists(inDir)) {
			MessageBox.Show(
				string.Format(
					"input_dir is defined but not exists:\n{0}",
					inDir
				),
				Program.APPLICATION_NAME,
				MessageBoxButtons.OK,
				MessageBoxIcon.Hand
			);
		}

		var outDir = getIniValue(iniPath, section, "output_dir", string.Empty);

		if (outDir == string.Empty) {
			MessageBox.Show(
				string.Format(
					"output_dir is not defined (section: {0})",
					section
				),
				Program.APPLICATION_NAME,
				MessageBoxButtons.OK,
				MessageBoxIcon.Hand
			);

			return;
		}

		outDir = Environment.ExpandEnvironmentVariables(outDir);

		if (!Path.IsPathRooted(outDir))
			outDir = Path.Combine(getExeDir(), outDir);

		if (!Directory.Exists(outDir)) {
			if (
				getBoolByString(
					getIniValue(
						iniPath,
						section,
						"create_output_dir",
						string.Empty
					).ToLower(),
					false
				)
			) {
				Directory.CreateDirectory(outDir);
			} else {
				MessageBox.Show(
					string.Format(
						"output_dir is defined but not exists:\n{0}",
						inDir
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);
			}
		}

		ImageCodecInfo imageCodecInfo = null;
		EncoderParameters encoderParameters = null;
		var extension = string.Empty;

		switch (
			getIniValue(
				iniPath,
				section,
				"type",
				string.Empty
			).ToLower()
		) {
			case "jpeg":
			case "jpg":
				var qualityStr = getIniValue(
					iniPath,
					section,
					"quality",
					string.Empty
				);

				if (qualityStr == string.Empty) {
					MessageBox.Show(
						string.Format(
							"quality is not defined (section: {0})",
							section
						),
						Program.APPLICATION_NAME,
						MessageBoxButtons.OK,
						MessageBoxIcon.Hand
					);

					return;
				}

				if (!uintRegex.IsMatch(qualityStr)) {
					MessageBox.Show(
						string.Format(
							"invalid quality (section: {0})",
							section
						),
						Program.APPLICATION_NAME,
						MessageBoxButtons.OK,
						MessageBoxIcon.Hand
					);

					return;
				}

				var quality = Int32.Parse(qualityStr);

				if (100 < quality) {
					MessageBox.Show(
						string.Format(
							"quality is out of range (0-100) (section: {0})",
							section
						),
						Program.APPLICATION_NAME,
						MessageBoxButtons.OK,
						MessageBoxIcon.Hand
					);

					return;
				}

				imageCodecInfo = getImageCodecInfo(ImageFormat.Jpeg.Guid);
				encoderParameters = new EncoderParameters(1);
				encoderParameters.Param[0] = new EncoderParameter(
					System.Drawing.Imaging.Encoder.Quality,
					quality
				);

				extension = "jpg";

				break;

			case "png":
				imageCodecInfo = getImageCodecInfo(ImageFormat.Png.Guid);

				extension = "png";

				break;

			case "": // string.Empty
				MessageBox.Show(
					string.Format(
						"type is not defined (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;

			default:
				MessageBox.Show(
					string.Format(
						"type is not valid (only jpg or png) (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;
		}

		var maxLongSide = 0;
		var maxWidth = 0;
		var maxHeight = 0;

		{
			var maxLongSideStr = getIniValue(
				iniPath,
				section,
				"max_long_side",
				"0"
			);

			if (!uintRegex.IsMatch(maxLongSideStr)) {
				MessageBox.Show(
					string.Format(
						"invalid max_long_side value (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;
			}

			maxLongSide = Int32.Parse(maxLongSideStr);

			var maxWidthStr = getIniValue(
				iniPath,
				section,
				"max_width",
				"0"
			);

			if (!uintRegex.IsMatch(maxWidthStr)) {
				MessageBox.Show(
					string.Format(
						"invalid max_width value (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;
			}

			maxWidth = Int32.Parse(maxWidthStr);

			var maxHeightStr = getIniValue(
				iniPath,
				section,
				"max_height",
				"0"
			);

			if (!uintRegex.IsMatch(maxHeightStr)) {
				MessageBox.Show(
					string.Format(
						"invalid max_height value (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;
			}

			maxHeight = Int32.Parse(maxHeightStr);
		}


		// nolimitation
		if (maxLongSide == 0)
			maxLongSide = Int32.MaxValue;

		if (maxWidth == 0)
			maxWidth = Int32.MaxValue;

		if (maxHeight == 0)
			maxHeight = Int32.MaxValue;

		var observe = false;
		var observeFileReadDelayMs = 0;
		var oneshot = false;

		switch (
			getIniValue(
				iniPath,
				section,
				"mode",
				string.Empty
			).ToLower()
		) {

			case "oneshot":
				oneshot = true;
				break;

			case "both":
				oneshot = true;
				goto case "observe";

			case "observe":
				observe = true;

				var observeFileReadDelayMsStr = getIniValue(
					iniPath,
					section,
					"observe_file_read_delay_ms",
					"1000"
				);

				if (!uintRegex.IsMatch(observeFileReadDelayMsStr)) {
					MessageBox.Show(
						string.Format(
							"invalid observe_file_read_delay_ms value (section: {0})",
							section
						),
						Program.APPLICATION_NAME,
						MessageBoxButtons.OK,
						MessageBoxIcon.Hand
					);

					return;
				}

				observeFileReadDelayMs = Int32.Parse(observeFileReadDelayMsStr);

				break;

			case "": // string.Empty
				MessageBox.Show(
					string.Format(
						"mode is not defined (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;

			default:
				MessageBox.Show(
					string.Format(
						"mode is not valid (section: {0})",
						section
					),
					Program.APPLICATION_NAME,
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand
				);

				return;
		}

		if (oneshot) {
			var pOpt = new ParallelOptions();
			pOpt.MaxDegreeOfParallelism = Environment.ProcessorCount;

			Console.WriteLine(string.Format(
				"OneShot ({0} threads): Process files...",
				pOpt.MaxDegreeOfParallelism
			));

			Parallel.ForEach(Directory.GetFiles(inDir, pattern), pOpt, f => {
				var newF = getDestFileName(f, outDir, extension);

				if (File.Exists(newF))
					return;

				resizeImage(
					f,
					newF,
					maxWidth,
					maxHeight,
					maxLongSide,
					imageCodecInfo,
					encoderParameters
				);
			});
		}

		if (observe) {
			var watcher = new FileSystemWatcher();

			watcher.Path = inDir;
			watcher.Filter = pattern;
			watcher.NotifyFilter = NotifyFilters.FileName;

			watcher.Created += new FileSystemEventHandler(
				async (source, e) => {
					Console.WriteLine(
						string.Format(
							"Convert Scheduled: {0}",
							Path.GetFileName(e.FullPath)
						)
					);


					await Task.Delay(observeFileReadDelayMs);

					resizeImage(
						e.FullPath,
						getDestFileName(e.FullPath, outDir, extension),
						maxWidth,
						maxHeight,
						maxLongSide,
						imageCodecInfo,
						encoderParameters
					);
				}
			);

			Console.WriteLine("Observing... (Press enter key to exit)");
			watcher.EnableRaisingEvents = true;

			Console.ReadLine();
		}
	}

	static string getDestFileName(string input, string outDir, string extension) {
		return Path.Combine(
			outDir,
			Path.GetFileNameWithoutExtension(input) + "." + extension
		);
	}

	static void resizeImage(
		string input,
		string output,
		int maxWidth,
		int maxHeight,
		int maxLongSide,
		ImageCodecInfo imageCodecInfo,
		EncoderParameters encParam
	) {
		try {
			using (var i = Image.FromFile(input)) {
				if (i.Width == 0 || i.Height == 0) {
					return;
				}

				var s = resizeKeepAspect(
					new Size(i.Width, i.Height),
					maxWidth,
					maxHeight,
					maxLongSide
				);

				if (s.Width == 0)
					s.Width = 1;

				if (s.Height == 0)
					s.Height = 1;

				using (var b = new Bitmap(s.Width, s.Height))
				using (var g = Graphics.FromImage(b)) {
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawImage(i, 0, 0, s.Width, s.Height);
					b.Save(output, imageCodecInfo, encParam);
				}

				Console.WriteLine(
					string.Format("Converted: {0}", Path.GetFileName(input))
				);
			}
		} catch (OutOfMemoryException) {
			Console.Error.WriteLine(
				string.Format(
					"Error file ({0}): Image file is invalid.",
					input
				)
			);
		}
	}

	static Size resizeKeepAspect(
		Size s,
		int maxWidth,
		int maxHeight,
		int maxLongSide
	) {
		if (
			s.Width <= maxWidth && s.Height <= maxHeight
			&&
			s.Width <= maxLongSide && s.Height <= maxLongSide
		) {
			return s;
		}

		double[] scales = {
			(double) maxWidth / s.Width,
			(double) maxHeight / s.Height,
			(double) maxLongSide / s.Width,
			(double) maxLongSide / s.Height,
		};

		return new Size(
			(int)(s.Width * scales.Min()),
			(int)(s.Height * scales.Min())
		);
	}

	static void Main(string[] Args) {
		var iniPath = Path.Combine(getExeDir(), "configure.ini");

		if (Args.Length == 0) {
			createSectionsShortcut(iniPath);

		} else if (Args.Length == 1) {
			observeSection(iniPath, Args[0]);

		} else {
			MessageBox.Show(
				"Too many arguments",
				Program.APPLICATION_NAME,
				MessageBoxButtons.OK,
				MessageBoxIcon.Hand
			);
		}

	}
}

