using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MigrateData3to4
{
	class Program
	{
		public static string Src = "datav3";
		public static string Dst = "data";
		public static List<string> custDaily = [];

		static void Main(string[] args)
		{
			TextWriterTraceListener myTextListener = new($"MXdiags{Path.DirectorySeparatorChar}MigrateData-{DateTime.Now:yyyyMMdd-HHmmss}.txt", "MDlog");
			Trace.Listeners.Add(myTextListener);
			Trace.AutoFlush = true;

			var fullVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			var version = $"{fullVer.Major}.{fullVer.Minor}.{fullVer.Build}";
			Utils.LogMessage("MigrateData v." + version);
			Console.WriteLine("MigrateData v." + version);

			// Have any custom daily files been supplied

			for (int i = 0; i < args.Length; i++)
			{
				custDaily.Add(args[i]);
			}


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
			LogFile.Convert();

			// Finally the custom daily log files
			if (custDaily.Count == 0)
			{
				Console.WriteLine("\nNo Custom daily log files supplied");
				Utils.LogMessage("No Custom daily log files supplied");
			}
			else
			{

				Console.WriteLine("\nMigrating Custom daily log files");
				Utils.LogMessage("Migrating Custom daily log files");

				LogFile.DoFiles(custDaily.ToArray(), LogFile.FileType.Dayfile);
			}

			// Do the static other files
			OtherFiles.Copy();

			// Do the monthly ini files
			OtherFiles.CopyMonthIni();

			Utils.LogMessage("Processing completed");
			Console.WriteLine($"\nProcessing completed: {DateTime.Now:U}\n");

			Console.WriteLine("Press Enter to exit");
			Console.ReadKey(true);

		}
	}
}
