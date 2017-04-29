using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace ReportConverter
{
	class Program
	{
		private static readonly NumberFormatInfo SumFormat = new NumberFormatInfo();
		
		static void Main(string[] args)
		{
			SumFormat.NumberDecimalSeparator = ",";
			SumFormat.NumberGroupSeparator = ".";

			List<TransactionDetail> entries;

			using (var reader = File.OpenText("c:\\transaction log.txt"))
			{
				entries = ReadAllTransactionEntries(reader);
			}

			AnalyzeTransactionTypes(entries);

			entries.Reverse();

			using (var writer = File.CreateText("c:\\transaction log.csv"))
			{
				var csv = new CsvWriter(writer);
				csv.Configuration.AutoMap<TransactionDetail>();
				csv.WriteRecords(entries);

				writer.Flush();
			}
		}

		private static void AnalyzeTransactionTypes(List<TransactionDetail> entries)
		{
			foreach (var transactionDetail in entries)
			{
				var title = transactionDetail.Title;

				if (title.StartsWith("albert ", StringComparison.InvariantCultureIgnoreCase)
				    || title.StartsWith("ah", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("jumbo", StringComparison.InvariantCultureIgnoreCase))
				{
					transactionDetail.Type = "Food";
					continue;
				}

				if (title.StartsWith("Naam: R. van", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: VIDA ", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: SPOTIFY", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: HUISMERK ", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: VITENS", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: ZIGGO ", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: STG ", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: Stichting Derdengelden Buckaroo", StringComparison.InvariantCultureIgnoreCase))
				{
					transactionDetail.Type = "Utilities";
					continue;
				}

				if (title.StartsWith("Naam: NS GROEP", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Ns-", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: TLS BV", StringComparison.InvariantCultureIgnoreCase))
				{
					transactionDetail.Type = "Transport";
					continue;
				}

				if (title.StartsWith("Etos", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("ikea", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("hema", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("xenos", StringComparison.InvariantCultureIgnoreCase))
				{
					transactionDetail.Type = "Groceries";
					continue;
				}
			}
		}

		private static List<TransactionDetail> ReadAllTransactionEntries(StreamReader reader)
		{
			var result = new List<TransactionDetail>();

			var hasData = true;
			while (hasData)
			{
				var transactionDetailStrings = new List<string>();
				string line;
				do
				{
					line = reader.ReadLine();

					if (line == null)
					{
						hasData = false;
						break;
					}

					transactionDetailStrings.Add(line);
				} while (IsNotLastTransactionLine(line));

				if (transactionDetailStrings.Count > 0)
				{
					var transactionDetail = ParseTransaction(transactionDetailStrings);
					result.Add(transactionDetail);
				}
			}

			return result;
		}

		private static TransactionDetail ParseTransaction(List<string> transactionDetailStrings)
		{
			var transactionDetail = new TransactionDetail();
			var firstLine = transactionDetailStrings[0];

			var date = firstLine.Substring(0, 10);
			transactionDetail.Date = DateTime.ParseExact(date, "dd-MM-yyyy", DateTimeFormatInfo.CurrentInfo);

			firstLine = firstLine.Substring(11);

			var sumIndex = firstLine.LastIndexOf(" ");
			var sumString = firstLine.Substring(sumIndex + 1);
			transactionDetail.Amount = decimal.Parse(sumString, SumFormat);


			firstLine = firstLine.Substring(0, sumIndex);

			var bankTypeIndex = firstLine.LastIndexOf(" ");
			var bankType = firstLine.Substring(bankTypeIndex + 1);

			transactionDetail.Title = firstLine.Substring(0, bankTypeIndex);

			var lastLine = transactionDetailStrings[transactionDetailStrings.Count - 1];
			transactionDetail.Direction = lastLine.Equals("af", StringComparison.InvariantCultureIgnoreCase)
				? Direction.Out
				: Direction.In;

			return transactionDetail;
		}

		private static bool IsNotLastTransactionLine(string line)
		{
			return !(line.Equals("af", StringComparison.InvariantCultureIgnoreCase) || line.Equals("bij", StringComparison.InvariantCultureIgnoreCase));
		}
	}

	internal class TransactionDetail
	{
		public Direction Direction { get; set; }
		public DateTime Date { get; set; }
		public string Title { get; set; }
		public decimal Amount { get; set; }
		public string Type { get; set; }
	}

	internal enum Direction
	{
		In,
		Out
	}
}
