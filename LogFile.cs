using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MigrateData3to4
{
	static class LogFile
	{

		private enum FileType
		{
			Log,
			ExtraLog,
			AirLink
		}

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
		}


		private static void DoLogFiles()
		{
			// Get a list of the files using a regex
			var reg = new Regex(@"[a-zA-Z]+[0-9]{2}log\.txt");
			var monFiles = Directory.GetFiles("data", "*log.txt").Where(path => reg.IsMatch(path)).ToArray();
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"LogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, FileType.Log);
		}

		private static void DoExtraLogFiles()
		{
			// Get a list of the files
			var monFiles = Directory.GetFiles("data", "Extra*.txt");
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"ExtraLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, FileType.ExtraLog);
		}

		private static void DoAirLinkLogFiles()
		{
			// Get a list of the files
			var monFiles = Directory.GetFiles("data", "AirLink*log.txt");
			Console.WriteLine($"Found {monFiles.Length} monthly log files to process");
			Utils.LogMessage($"AirLinkLogFile: Found {monFiles.Length} monthly log files to process");

			DoFiles(monFiles, FileType.AirLink);
		}

		private static void DoFiles(string[] files, FileType type)
		{
			foreach (var inFile in files)
			{
				Console.Write("  Processing file " + inFile + "... ");
				Utils.LogMessage("Processing file " + inFile);

				try
				{
					// read the first line to determine format
					var line1 = File.ReadLines(inFile).First();
					char sepInp = Utils.GetLogFileSeparator(line1, ',');
					Utils.LogMessage($"File {inFile} is using the separator: {sepInp}");

					Utils.TryDetectNewLine(inFile, out string endOfLine);
					Utils.LogMessage($"File {inFile} is using the line ending: {(endOfLine == "\n" ? "\\n" : "\\r\\n")}");

					// Split the line
					var fields = line1.Split(sepInp);

					// Get the date time so we can create the output filename
					var date = Utils.DdmmyyhhmmStrToDate(fields[0], fields[1]);

					var outFilename = Program.Dest + "/" + GetFilename(date, type);

					var cnt = WriteFileContents(inFile, outFilename, sepInp, endOfLine);

					Console.WriteLine("done.");
					Utils.LogMessage($"Finished processing file {inFile}, lines processed = {cnt}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing log file - {ex.Message}\n");
				}
			}

		}

		private static string GetFilename(DateTime date, FileType type)
		{
			if (type == FileType.Log)
				return Utils.GetLogFileName(date);
			else if (type == FileType.ExtraLog)
				return Utils.GetExtraLogFileName(date);
			else
				return Utils.GetAirLinkLogFileName(date);
		}


		/// <summary>
		/// All new log files have the same internal format for the first two fields, this copies input to output
		/// </summary>
		/// <param name="date"></param>
		/// <param name="inpFile"></param>
		/// <param name="outFile"></param>
		/// <param name="sep"></param>
		/// <param name="eol"></param>
		/// <returns>Count of the lines processed</returns>
		private static int WriteFileContents(string inpFile, string outFile, char sep, string eol)
		{
			var lineNum = 0;
			string inpLine;
			StringBuilder outLine = new(512);
			bool inDST;
			DateTime previousDate = DateTime.MinValue;

			try
			{
				using (var sr = new StreamReader(inpFile))
				using (var sw = new StreamWriter(outFile))
				{
					sw.NewLine = eol;

					while ((inpLine = sr.ReadLine()) != null)
					{
						// Reset the string builder
						outLine.Length = 0;

						var fields = inpLine.Split(sep);

						// get the date and time
						var date = Utils.DdmmyyhhmmStrToDate(fields[0], fields[1]);

						// check if we are in standard time or DST
						inDST = !TimeZoneInfo.Local.IsDaylightSavingTime(date);

						if (inDST && date < previousDate)
						{
							inDST = false;
						}
						previousDate= date;

						var utcDate = Utils.ResolveAmbiguousTime(date, inDST);

						outLine.Append(date.ToString("dd/MM/yy HH:mm", CultureInfo.InvariantCulture.DateTimeFormat));
						outLine.Append(',');
						outLine.Append(Utils.ToUnixTime(utcDate));
						outLine.Append(',');

						// do the rest of the fields, converting comma decimals to dot
						foreach (var field in fields.Skip(2))
						{
							outLine.Append(field.Replace(',', '.'));
							outLine.Append(',');
						}

						// remove the last ','
						outLine.Length--;

						// Write the output
						sw.WriteLine(outLine);

						lineNum++;
					};
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error at line {lineNum + 1} - {ex.Message}\n");
				Console.ResetColor();
			}
			return lineNum;
		}
	}
}
