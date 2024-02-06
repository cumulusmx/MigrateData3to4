using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MigrateData3to4
{
	static partial class LogFile
	{
		static internal void Convert()
		{
			// First do the Monthly log files
			Console.WriteLine("\nMigrating monthly log files");
			Utils.LogMessage("Migrating monthly log files");

			DoLogFiles();

			// Now the extra Monthly log files
			Console.WriteLine("\nMigrating Extra monthly log files");
			Utils.LogMessage("Migrating Extra monthly log files");

			DoExtraLogFiles();

			// Now the AirLink Monthly log files
			Console.WriteLine("\nMigrating AirLink monthly log files");
			Utils.LogMessage("Migrating AirLink monthly log files");

			DoAirLinkLogFiles();

			// Now the Custom Monthly log files
			Console.WriteLine("\nMigrating Custom monthly log files");
			Utils.LogMessage("Migrating Custom monthly log files");

			DoCustomMonthlyFiles();
		}


		private static void DoLogFiles()
		{
			// Get a list of the files using a regex
			var reg = MonthlyLogFilesRegex();
			var monFiles = Directory.GetFiles(Program.Src, "*log.txt").Where(path => reg.IsMatch(path)).ToArray();
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"LogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, true);
		}

		private static void DoExtraLogFiles()
		{
			// Get a list of the files
			var reg = ExtraMonthlyLogFilesRegex();
			var monFiles = Directory.GetFiles(Program.Src, "ExtraLog*.txt").Where(path => reg.IsMatch(path)).ToArray();
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"ExtraLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles);
		}

		private static void DoAirLinkLogFiles()
		{
			// Get a list of the files
			var monFiles = Directory.GetFiles(Program.Src, "AirLink*log.txt");
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"AirLinkLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles);
		}

		private static void DoCustomMonthlyFiles()
		{
			// Get a list of the files
			var reg = CustomLogFilesRegex();
			var monFiles = Directory.GetFiles(Program.Src, "*.txt").Where(path => reg.IsMatch(path)).ToArray();
			Console.WriteLine($"Found {monFiles.Length} custom monthly log files to process");
			Utils.LogMessage($"CustomMonthly: Found {monFiles.Length} custom monthly log files to process");

			DoFiles(monFiles);
		}

		public static void DoFiles(string[] files, bool monthly = false)
		{
			foreach (var inFile in files)
			{
				Console.Write("  Processing file " + inFile + "... ");
				Utils.LogMessage("Processing file " + inFile);

				try
				{
					var cnt = WriteFileContents(inFile, monthly);

					Console.WriteLine("done.");
					Utils.LogMessage($"Finished writing to file, lines processed = {cnt}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing log file - {ex.Message}\n");
				}
			}

		}


		/// <summary>
		/// All new log files have the same internal format for the first two fields, this copies input to output
		/// </summary>
		/// <param name="date"></param>
		/// <param name="inpFile"></param>
		/// <param name="monthly"></param>
		/// <returns>Count of the lines processed</returns>
		private static int WriteFileContents(string inpFile, bool monthly=false)
		{
			var lineNum = 1;
			string outFile;

			try
			{
				// read the first line to determine format
				var lines = File.ReadLines(inpFile).ToArray();
				char sepInp = Utils.GetLogFileSeparator(lines[0], ',');
				Utils.LogMessage($"LogFile: File is using the separator: {sepInp}");

				Utils.TryDetectNewLine(inpFile, out string endOfLine);
				Utils.LogMessage($"LogFile: File {inpFile} is using the line ending: {(endOfLine == "\n" ? "\\n" : "\\r\\n")}");

				if (monthly)
				{
					outFile = Program.Dst + Path.DirectorySeparatorChar + Utils.DdmmyyStrToDate(lines[0].Split(sepInp)[0]).ToString("yyyyMM") + "log.txt";
				}
				else
				{
					outFile = Program.Dst + Path.DirectorySeparatorChar + inpFile.Split(Path.DirectorySeparatorChar).Last();
				}

				using var sw = new StreamWriter(outFile) { NewLine = endOfLine };

				foreach (var inpLine in lines)
				{
					var fields = inpLine.Split(sepInp);

					// Do the date
					fields[0] = Utils.DdmmyyStrToStr(fields[0]);

					// do the rest of the fields, converting comma decimals to dot
					for (var i = 1; i < fields.Length; i++)
					{
						fields[i] = fields[i].Replace(',', '.');
					}

					// Write the output
					sw.WriteLine(string.Join(',', fields));

					lineNum++;
				}
				sw.Flush();
				sw.Close();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error at line {lineNum} - {ex.Message}");
				Console.WriteLine($"Please fix the file {inpFile} at line {lineNum} and try the migration again\n");

				Console.ResetColor();
			}
			return lineNum - 1;
		}

		[GeneratedRegex(@"\w{3}[0-9]{2}log\.txt")]
		private static partial Regex MonthlyLogFilesRegex();

		[GeneratedRegex(@"ExtraLog20[0-9]{4}\.txt")]
		private static partial Regex ExtraMonthlyLogFilesRegex();

		[GeneratedRegex(@".+-20[0-9]{4}\.txt")]
		private static partial Regex CustomLogFilesRegex();
	}
}
