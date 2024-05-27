using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace MigrateData3to4
{
	static partial class LogFile
	{
		const int NumLogFileFields = 29;
		const int NumExtraLogFileFields = 92;
		const int NumAirLinkLogFileFields = 56;
		const int DayfileFields = 55;


		static internal void Convert(CustLogs custLogs)
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

			DoCustomMonthlyFiles(custLogs);

			// Now the Custom Monthly log files
			Console.WriteLine("\nMigrating Custom daily log files");
			Utils.LogMessage("Migrating Custom daily log files");

			DoCustomDailyFiles(custLogs);
		}


		private static void DoLogFiles()
		{
			// Get a list of the files using a regex
			var reg = MonthlyLogFilesRegex();
			var monFiles = Directory.GetFiles(Program.Src, "*log.txt");
			List<string> logFiles = [];

			foreach (string file in monFiles)
			{
				if (reg.IsMatch(Path.GetFileName(file)))
				{
					logFiles.Add(file);
				}
			}
			Console.WriteLine($"Found {logFiles.Count} monthly log files to process");
			Utils.LogMessage($"LogFile: Found {logFiles.Count} monthly log files to process");

			DoFiles(logFiles.ToArray(), FileType.Monthly);
		}

		private static void DoExtraLogFiles()
		{
			// Get a list of the files
			var reg = ExtraMonthlyLogFilesRegex();
			var monFiles = Directory.GetFiles(Program.Src, "ExtraLog*.txt").Where(path => reg.IsMatch(path)).ToArray();
			Console.WriteLine($"Found {monFiles.Length} monthly Extra log files to process");
			Utils.LogMessage($"ExtraLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, FileType.Extra);
		}

		private static void DoAirLinkLogFiles()
		{
			// Get a list of the files
			var monFiles = Directory.GetFiles(Program.Src, "AirLink*log.txt");
			Console.WriteLine($"Found {monFiles.Length} monthly AirLink log files to process");
			Utils.LogMessage($"AirLinkLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, FileType.AirLink);
		}

		private static void DoCustomMonthlyFiles(CustLogs custLogs)
		{
			if (custLogs.IntvLogs.Count == 0)
			{
				Console.WriteLine("\nNo Custom interval log files defined in Cumulus.ini");
				Utils.LogMessage("No Custom interval log files defined in Cumulus.ini");
			}
			else
			{
				// Interval log file names are templates, we need to filnd all the files
				foreach (var file in custLogs.IntvLogs)
				{
					var reg = new Regex(file + @"-20[0-9]{4}\.txt");
					var monFiles = Directory.GetFiles(Program.Src, "*.txt").Where(path => reg.IsMatch(path)).ToArray();

					Console.WriteLine($" Found {monFiles.Length} custom monthly log files matching \"{file}\" to process");
					Utils.LogMessage($"CustomMonthly: Found {monFiles.Length} custom monthly log files matching \"{file}\" to process");

					DoFiles(monFiles, FileType.CustomIntv);
				}
			}
		}

		private static void DoCustomDailyFiles(CustLogs custLogs)
		{
			if (custLogs.DailyLogs.Count == 0)
			{
				Console.WriteLine("\nNo Custom daily log files defined in Cumulus.ini");
				Utils.LogMessage("No Custom daily log files defined in Cumulus.ini");
			}
			else
			{
				DoFiles(custLogs.DailyLogs.ToArray(), FileType.CustomDaily);
			}
		}


		public static void DoFiles(string[] files, FileType fileType)
		{
			foreach (var inFile in files)
			{
				Console.Write("  Processing file " + inFile + "... ");
				Utils.LogMessage("Processing file " + inFile);

				try
				{
					if (File.Exists(inFile))
					{
						var cnt = WriteFileContents(inFile, fileType);

						Console.WriteLine("done.");
						Utils.LogMessage($"Finished writing to file, lines processed = {cnt}");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("File not found.");
						Utils.LogMessage($"File not found {inFile}");
						Console.ResetColor();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing log file - {ex.Message}\n");
					Utils.LogMessage($"Error processing log {inFile} file - {ex.Message}\n");
				}
			}

		}


		/// <summary>
		/// All new log files have the same internal format for the first two fields, this copies input to output
		/// </summary>
		/// <param name="date"></param>
		/// <param name="inpFile"></param>
		/// <param name="fileType"></param>
		/// <returns>Count of the lines processed</returns>
		private static int WriteFileContents(string inpFile, FileType fileType)
		{
			var lineNum = 1;
			string outFile;

			var fieldCount = fileType switch
			{
				FileType.Monthly => NumLogFileFields,
				FileType.Extra => NumExtraLogFileFields,
				FileType.AirLink => NumAirLinkLogFileFields,
				FileType.Dayfile => DayfileFields,
				_ => -1,
			};

			try
			{
				// Custom daily files are just the bare filename
				if (fileType == FileType.CustomDaily)
				{
					inpFile = Program.Src + Path.DirectorySeparatorChar + inpFile;
				}

				// read the first line to determine format
				var lines = File.ReadLines(inpFile).ToArray();
				Program.sepField = Utils.GetLogFileSeparator(lines[0], ',');
				Utils.LogMessage($"LogFile: File is using the field separator: {Program.sepField}");

				if (fileType != FileType.CustomDaily)
				{
					Program.sepTime = Utils.GetLogFileTimeSeparator(lines[0], ':');
				}
				Utils.LogMessage($"LogFile: File is using the time separator: {Program.sepTime}");

				if (Program.sepTime != ':')
				{
					for (var i = 0; i < lines.Length; i++)
					{
						lines[i] = lines[i].Replace(Program.sepTime, ':');
					}
				}


				if (fileType == FileType.Monthly)
				{
					outFile = Program.Dst + Path.DirectorySeparatorChar + Utils.DdmmyyStrToDate(lines[0].Split(Program.sepField)[0]).ToString("yyyyMM") + "log.txt";
				}
				else
				{
					outFile = Program.Dst + Path.DirectorySeparatorChar + inpFile.Split(Path.DirectorySeparatorChar)[^1];
				}

				Utils.LogMessage($"LogFile: File {inpFile} will be written to {outFile}");
				Utils.TryDetectNewLine(inpFile, out string endOfLine);
				Utils.LogMessage($"LogFile: File {inpFile} is using the line ending: {(endOfLine == "\n" ? "\\n" : "\\r\\n")}");


				using var sw = new StreamWriter(outFile) { NewLine = endOfLine };

				for (var l =0; l < lines.Length; l++)
				{
					var line = lines[l];

					if (fileType != FileType.CustomIntv && fileType != FileType.CustomDaily && line[0] < 32)
					{
						var repLine = RepairLine(line, Program.sepField, fieldCount);
						if (repLine == null)
						{
							Console.WriteLine($"  deleted corrupt line {l + 1}");
							Utils.LogMessage($"LogFile: File {inpFile} deleted corrupt line {l +1}");
							continue;
						}
						else
						{
							Console.WriteLine($"  repaired corrupt line {l + 1}");
							Utils.LogMessage($"LogFile: File {inpFile} repaired corrupt line {l + 1}");
							line = repLine;
						}
					}

					var fields = line.Split(Program.sepField);

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
			catch (FileNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"File not found: {inpFile}");
				Console.WriteLine($"File not found: {inpFile}\n");

				Console.ResetColor();
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

		private static string RepairLine(string line, char sep, int recCount)
		{
			if (recCount < 0)
				return null;

			try
			{
				line = new string((from c in line
								   where char.IsLetterOrDigit(c) || char.IsPunctuation(c)
								   select c
						).ToArray());

				// test if it is now valid by spliting into fields and counting them
				if (line.Split(sep).Length == recCount)
				{
					// all good, return the modded line
				}
				else
				{
					line = null;
				}
			}
			catch
			{
				// it failed somewhere, just delete the line
				line = null;
			}

			return line;
		}

		public enum FileType : ushort
		{
			Monthly = 0,
			Extra = 1,
			AirLink = 2,
			Dayfile = 3,
			CustomIntv = 4,
			CustomDaily = 5
		}

		[GeneratedRegex(@"^\D+[\d]{2}log\.txt")]
		private static partial Regex MonthlyLogFilesRegex();

		[GeneratedRegex(@"ExtraLog20[\d]{4}\.txt")]
		private static partial Regex ExtraMonthlyLogFilesRegex();
	}
}
