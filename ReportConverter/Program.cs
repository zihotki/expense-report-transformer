using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

			using (var reader = File.OpenText("c:\\Users\\cube\\Desktop\\transaction.json"))
			{
				entries = ReadAllTransactionEntries(reader);
			}

			AnalyzeTransactionTypes(entries);

			entries.Reverse();

			using (var writer = File.CreateText("c:\\Users\\cube\\Desktop\\transaction log old.txt"))
			{
				var csv = new CsvWriter(writer);
				csv.Configuration.AutoMap<TransactionDetail>();
				csv.WriteRecords(entries);

				writer.Flush();
			}
		}

		private static void AnalyzeTransactionTypes(List<TransactionDetail> entries)
		{
			foreach (var transaction in entries)
			{
				var title = transaction.Title;

				if (title.StartsWith("albert ", StringComparison.InvariantCultureIgnoreCase)
				    || title.StartsWith("ah", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("jumbo", StringComparison.InvariantCultureIgnoreCase))
				{
					transaction.Type = "Food";
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
					transaction.Type = "Utilities";
					continue;
				}

				if (title.StartsWith("Naam: NS GROEP", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Ns-", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("Naam: TLS BV", StringComparison.InvariantCultureIgnoreCase))
				{
					transaction.Type = "Transport";
					continue;
				}

				if (title.StartsWith("Etos", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("ikea", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("hema", StringComparison.InvariantCultureIgnoreCase)
					|| title.StartsWith("xenos", StringComparison.InvariantCultureIgnoreCase))
				{
					transaction.Type = "Groceries";
					continue;
				}
			}
		}

        private static List<TransactionDetail> ReadAllTransactionEntries(StreamReader reader)
        {
            var result = new List<TransactionDetail>();

            JObject allDetails = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            IList<JToken> transactions = allDetails["transactions"].Children().ToList();

            foreach (JToken r in transactions)
            {
                Transaction transactionResult = r.ToObject<Transaction>();
                var transactionDetail = new TransactionDetail()
                {
                    Date = DateTime.ParseExact(transactionResult.Date, "dd-MM-yyyy", DateTimeFormatInfo.CurrentInfo),
                    Amount = Math.Abs(Decimal.Parse(transactionResult.Amount)),
                    Direction = Decimal.Parse(transactionResult.Amount) > 0 ? Direction.In : Direction.Out,                   
                    Title = transactionResult.StatementLines[0],
                    Type = ""
                };
                result.Add(transactionDetail);
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
