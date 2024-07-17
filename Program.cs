using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MigrateData3to4
{
	static class Program
	{
		public const string Src = "datav3";
		public const string Dst = "data";
		public static char sepField;
		public static char sepTime;

		static void Main(string[] args)
		{

			for (int i = 0; i < args.Length; i++)
			{
				try
				{
					switch (args[i])
					{
						case "-lang" when args.Length >= i:
							{
								var lang = args[++i];
								// some people enter the code as eg en_GB, it should use dash en-GB
								lang = lang.Replace('_', '-');

								CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(lang);
								CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(lang);
								Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
								CultureInfo.CurrentCulture = new CultureInfo(lang);
								CultureInfo.CurrentUICulture = new CultureInfo(lang);
								break;
							}
					}
				}
				catch
				{
					Usage();
				}
			}

			TextWriterTraceListener myTextListener = new($"MXdiags{Path.DirectorySeparatorChar}MigrateData-{DateTime.Now:yyyyMMdd-HHmmss}.txt", "MDlog");
			Trace.Listeners.Add(myTextListener);
			Trace.AutoFlush = true;

			var fullVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			var version = $"{fullVer.Major}.{fullVer.Minor}.{fullVer.Build}";
			Utils.LogMessage("MigrateData v." + version);
			Console.WriteLine("MigrateData v." + version);

			var custLogs = new CustLogs();

			var pwd = Directory.GetCurrentDirectory();
			var color = Console.ForegroundColor;
			var sep = Path.DirectorySeparatorChar;
			Console.WriteLine("\nThis will convert your log files from Cumulus MX v3 to Cumulus MX v4 format");
			Console.Write($"  Files will be read from  : ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(pwd + sep + Src);
			Console.ForegroundColor = color;
			Console.Write($"  New files will created in: ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(pwd + sep + Dst);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("\n  Any existing files in the output folder will be overwritten");
			Console.ForegroundColor = color;
			Console.WriteLine("\nPress a Enter to continue, or Ctrl-C to exit");
			Console.ReadKey(true);

			Utils.LogMessage("Processing started");
			Console.WriteLine($"\nProcessing started: {DateTime.Now:U}\n");

			if (!Directory.Exists(Src))
			{
				Utils.LogMessage($"Error: Source folder '{Src}' does not exist. Aborting");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error: Source folder '{Src}' does not exist. Aborting");
				Console.ForegroundColor = ConsoleColor.White;
				Environment.Exit(1);
			}


			if (!Directory.Exists(Dst))
			{
				Utils.LogMessage($"Target folder '{Dst}' does not exist, creating it...");
				Console.WriteLine($"Target folder '{Dst}' does not exist, creating it...");

				Directory.CreateDirectory(Dst);
			}

			// Do the dayfile
			DayFile.Convert();

			// Do the monthly log files
			LogFile.Convert(custLogs);

			// Do the static other files
			OtherFiles.Copy();

			// Do the monthly ini files
			OtherFiles.CopyMonthIni();

			Utils.LogMessage("Processing completed");
			Console.WriteLine($"\nProcessing completed: {DateTime.Now:U}\n");

			Console.WriteLine("Press Enter to exit");
			Console.ReadKey(true);

		}

		private static void Usage()
		{
			Console.WriteLine();
			Console.WriteLine("Valid arguments are:");
			Console.WriteLine(" -lang <culture_name> - Sets the Language MigrateData3to4 will use (defaults to current user language)");
			Console.WriteLine("\nMigrateData3to4 terminating");
			Environment.Exit(1);
		}
	}
}
