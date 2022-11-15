using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MigrateData3to4
{
	static class DayFile
	{

		private static readonly string inpFilename = "data/dayfile.txt";
		private static readonly string outFilename = Program.Dst + "/dayfile-v4.txt";


		static internal void Convert()
		{
			Console.WriteLine($"Migrating dayfile.txt to {outFilename}");

			if (File.Exists(inpFilename))
			{
				Utils.LogMessage($"Dayfile: Found input file: {inpFilename}");
			}
			else
			{
				Console.WriteLine("Dayfile: Warning! Input dayfile.txt not found");
				Utils.LogMessage("Dayfile: Warning! Input dayfile.txt not found");
				return;
			}

			if (File.Exists(outFilename + ".sav"))
			{
				Console.WriteLine("Dayfile: The dayfile.txt backup file dayfile-v4.txt.sav already exists, aborting to prevent overwriting the original data.");
				Utils.LogMessage("Dayfile: The dayfile.txt backup file dayfile-v4.txt.sav already exists, aborting to prevent overwriting the original data.");
				Console.WriteLine("Press any key to exit");
				Console.ReadKey(true);
				Console.WriteLine("Exiting...");
				Environment.Exit(1);
			}

			var lineNum = 1;
			string inpLine;
			StringBuilder outLine = new(512);

			try
			{

				// read the first line to determine format
				var line1 = File.ReadLines(inpFilename).First();
				char sepInp = Utils.GetLogFileSeparator(line1, ',');
				Utils.LogMessage($"LogFile: File {inpFilename} is using the separator: {sepInp}");

				Utils.TryDetectNewLine(inpFilename, out string endOfLine);
				Utils.LogMessage($"LogFile: File {inpFilename} is using the line ending: {(endOfLine == "\n" ? "\\n" : "\\r\\n")}");

				using var sr = new StreamReader(inpFilename);
				using var sw = new StreamWriter(outFilename);
				sw.NewLine = endOfLine;

				while ((inpLine = sr.ReadLine()) != null)
				{
					// Reset the string builder
					outLine.Length = 0;

					var fields = inpLine.Split(sepInp);

					// Do the date
					outLine.Append(Utils.DdmmyyStrToStr(fields[0]));
					outLine.Append(',');
					// Do the Unix timestamp
					outLine.Append(Utils.ToUnixTime(Utils.DdmmyyStrToDate(fields[0])));
					outLine.Append(',');
					// do the rest of the fields, converting comma decimals to dot
					foreach (var field in fields.Skip(1))
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
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error at line {lineNum} - {ex.Message}\n" );
				Console.ResetColor();
			}
		}
	}
}
