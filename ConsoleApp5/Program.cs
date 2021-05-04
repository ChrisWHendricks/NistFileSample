using System;
using System.IO;
using System.Text;

namespace ConsoleApp5
{
	class Program
	{
		static void Main(string[] args)
		{
			var nist = new NISTFileGenerator();

			// Add 1.01 Tag with all zeros, this is the record length and will be calculated
			nist.Add("1.01", "00");
			nist.Add("1.02", "0200");

			/******************************
			 * Tag 1.03 has several parts
			 * Category Code which seems to always be 1
			 * Record Count: Total number of records that are not type 1
			 * Record Type plus IDC
			 * For example is there was one type 2 record and one type 10 record
			 * tag 1.03 would look like this:
			 * 1[US]02[RS]2[US]00[RS]10[US]01[GS]			 * 
			 * **************************************************/
			var cnt = new StringBuilder();
			cnt.Append($"1{nist.scUnitStop}");
			cnt.Append($"02{nist.scRecordStop}");
			cnt.Append($"2{nist.scUnitStop}00{nist.scRecordStop}");
			cnt.Append($"10{nist.scUnitStop}01");
			nist.Add("1.03", cnt.ToString());
			nist.Add("1.04", "CRM");
			nist.Add("1.05", "20210430");
			nist.Add("1.06", "2");
			nist.Add("1.07", "CA0100100");
			nist.Add("1.08", "CA0100100");

			// Add 2.001 Tag with all zeros, this is the record length and will be calculated
			nist.Add("2.001", "0000000");
			nist.Add("2.002", "00");
			nist.Add("2.005", "RET");
			nist.Add("2.009", "202100000002");
			nist.Add("2.016", "222336666");
			nist.Add("2.024", "M");
			nist.Add("2.025", "W");
			nist.Add("2.031", "BLU");
			// Example of name need to specify unit stops between fields
			nist.Add("2.050", "BadGuy" + nist.scUnitStop + "Joe" + nist.scUnitStop + "W" + nist.scUnitStop + "JR");
			nist.Add("2.051", "CustomData");

			// Type 10
			var photoData = File.ReadAllBytes("WileyMug.jpg");
			var photoString = Convert.ToBase64String(photoData, Base64FormattingOptions.None);

			nist.Add("10.001", "00000");
			nist.Add("10.002", "01");
			nist.Add("10.003", "FACE");
			nist.Add("10.004", "CA0100100");
			nist.Add("10.005", "20210427");
			nist.Add("10.006", "451");
			nist.Add("10.007", "474");
			nist.Add("10.008", "1");
			nist.Add("10.009", "500");
			nist.Add("10.010", "500");
			nist.Add("10.011", "JPEGB");
			nist.Add("10.012", "YCC");
			nist.Add("10.020", "F");
			nist.Add("10.999", photoString);

			var type12 = nist.Type1AndType2RecordsToString();
			var type10NoData = nist.Type10RecordStringsNoData();

			using (var fs = new FileStream("C:\\Test.nst", FileMode.Create, FileAccess.ReadWrite))
			{
				using (var sw = new StreamWriter(fs))
				{
					sw.AutoFlush = true;
					sw.Write(type12);

					var bw = new BinaryWriter(fs);
					var photoCounter = 0;
					foreach (var type10 in type10NoData)
					{
						sw.Write(type10);
						bw.Write(nist.Type10RecordDataBytes(photoCounter));
						photoCounter++;
						sw.Write(nist.scFileStop);
					}
				}
			}
		}
	}
}
