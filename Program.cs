using System;
using System.Diagnostics;
using System.IO;

namespace MigrateData3to4
{
	class Program
	{
		public static string Dest = "datav4";

		static void Main()
		{
			TextWriterTraceListener myTextListener = new($"MXdiags{Path.DirectorySeparatorChar}MigrateData-{DateTime.Now:yyyyMMdd-HHmmss}.txt", "MDlog");
			Trace.Listeners.Add(myTextListener);
			Trace.AutoFlush = true;

			var fullVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			var version = $"{fullVer.Major}.{fullVer.Minor}.{fullVer.Build}";
			Utils.LogMessage("MigrateData v." + version);
			Console.WriteLine("MigrateData v." + version);

			var pwd = Directory.GetCurrentDirectory();
			var color = Console.ForegroundColor;
			var sep = Path.DirectorySeparatorChar.ToString();
			Console.WriteLine("\nThis will convert your log files from Cumulus MX v3 to Cumulus MX v4 format");
			Console.Write($"  Files will be read from  : ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"{pwd + sep}data");
			Console.ForegroundColor = color;
			Console.Write($"  New files will created in: ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(pwd + sep + Dest);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("\n  Any existing files in the output folder will be overwritten");
			Console.ForegroundColor = color;
			Console.WriteLine("\nPress a key to continue, or Ctrl-C to exit");
			Console.ReadKey(true);

			Utils.LogMessage("Processing started");
			Console.WriteLine($"\nProcessing started: {DateTime.Now:U}\n");

			if (!Directory.Exists("data"))
			{
				Utils.LogMessage("Error: Source folder 'data' does not exist. Aborting");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error: Source folder 'data' does not exist. Aborting");
				Console.ForegroundColor = ConsoleColor.White;
				Environment.Exit(1);
			}

			if (!Directory.Exists(Dest))
			{
				Utils.LogMessage("Target folder data2 does not exist, creating it...");
				Console.WriteLine("Target folder data2 does not exist, creating it...");

				Directory.CreateDirectory(Dest);
			}

			// Do the dayfile
			DayFile.Convert();

			// Do the monthly log files
			LogFile.Convert();

			Utils.LogMessage("Processing completed");
			Console.WriteLine($"\nProcessing completed: {DateTime.Now:U}\n");

			Console.WriteLine("Press any key to exit");
			Console.ReadKey(true);

		}
	}
}
