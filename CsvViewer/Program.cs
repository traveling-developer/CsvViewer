using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvViewer
{
	internal class Program
	{
		private static string[] Convert_line_to_record_fields(string line, string delimiter)
		{
			return Convert_line_to_record_fields(line, delimiter, new List<string>()).ToArray();
		}

		private static List<string> Convert_line_to_record_fields(string line, string delimiter, List<string> fields)
		{
			if (line == "")
			{
				return fields;
			}

			if (line.StartsWith("\""))
			{
				line = line.Substring(1);
				int iApo = line.IndexOf("\"");
				fields.Add(line.Substring(0, iApo).Trim());

				line = line.Substring(iApo + 1);
				int iDelim = line.IndexOf(delimiter);
				if (iDelim >= 0)
				{
					line = line.Substring(iDelim + 1);
				}
				else
				{
					line = "";
				}
			}
			else
			{
				int iDelim = line.IndexOf(delimiter);
				if (iDelim >= 0)
				{
					fields.Add(line.Substring(0, iDelim).Trim());
					line = line.Substring(iDelim + 1);
				}
				else
				{
					fields.Add(line.Trim());
					line = "";
				}
			}

			return Convert_line_to_record_fields(line, delimiter, fields);
		}

		private static string Create_disply_line_for_record(string[] recordFields, int[] colWidths)
		{
			return string.Join("|", recordFields.Select((f, i) => f.PadRight(colWidths[i])));
		}

		private static void Main(string[] args)
		{
			string[] rawLines = File.ReadAllLines(args[0]);

			int pageLen = 5;
			if (args.Length > 1)
			{
				pageLen = int.Parse(args[1]);
			}

			IEnumerable<string> pageLines = rawLines.Take(pageLen + 1);
			int iFirstLineOfLastPage = 1;

			while (true)
			{
				IEnumerable<string[]> records = pageLines.Select(l => Convert_line_to_record_fields(l, ","));

				int[] colWidths = Enumerable.Range(0, records.First().Count())
					.Select(i => records.Select(r => r[i].Length).Max())
					.ToArray();
				string headline = Create_disply_line_for_record(records.First(), colWidths);

				IEnumerable<string> underlineRecord =
					Enumerable.Range(0, colWidths.Length).Select(i => new string('-', colWidths[i]));
				string underline = string.Join("+", underlineRecord);

				IEnumerable<string> displayLines =
					new[] {headline, underline}.Concat(
						records.Where((r, i) => i > 0).Select(r => Create_disply_line_for_record(r, colWidths)));
				Console.WriteLine(string.Join("\n", displayLines));

				Console.Write("F(irst, L(ast, N(ext, P(rev, eX(it: ");
				char cmd = char.ToLower(Console.ReadKey().KeyChar);
				Console.WriteLine("\n");

				switch (cmd)
				{
					case 'x':
						return;

					case 'f':
						pageLines = rawLines.Take(pageLen + 1);
						iFirstLineOfLastPage = 1;
						break;

					case 'l':
						iFirstLineOfLastPage = rawLines.Length - (rawLines.Length - 1)%pageLen;
						pageLines = new[] {rawLines[0]}.Concat(rawLines.Where((l, i) => i > 0 && i >= iFirstLineOfLastPage));
						break;

					case 'n':
						iFirstLineOfLastPage += pageLen;
						if (iFirstLineOfLastPage >= rawLines.Length)
						{
							iFirstLineOfLastPage = rawLines.Length - (rawLines.Length - 1)%pageLen;
						}
						pageLines =
							new[] {rawLines[0]}.Concat(
								rawLines.Where((l, i) => i > 0 && i >= iFirstLineOfLastPage && i < (iFirstLineOfLastPage + pageLen)));
						break;

					case 'p':
						iFirstLineOfLastPage -= pageLen;
						if (iFirstLineOfLastPage < 1)
						{
							iFirstLineOfLastPage = 1;
						}
						pageLines =
							new[] {rawLines[0]}.Concat(
								rawLines.Where((l, i) => i > 0 && i >= iFirstLineOfLastPage && i < (iFirstLineOfLastPage + pageLen)));
						break;
				}
			}
		}
	}
}