using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp5
{
	public class NISTFileGenerator
	{
		#region Special Characters
		/// <summary>Exposed for testing only.</summary>
		public string scFileStop;
		/// <summary>Exposed for testing only.</summary>
		public string scGroupStop;
		/// <summary>Exposed for testing only.</summary>
		public string scRecordStop;
		public string scUnitStop;
		#endregion

		#region Properties
		public SortedDictionary<string, List<string>> _nistRecords;

		private SortedDictionary<string, string> _extraData;

		public SortedDictionary<string, string> ExtraData
		{
			get { return _extraData; }
		}

		private Guid _guid;
		public Guid Guid
		{
			get { return _guid; }
			set { _guid = value; }
		}

		/// <summary>Returns the number of items in the file generator.</summary>
		public int Count
		{
			get { return _nistRecords.Count; }
		}
		#endregion

		public NISTFileGenerator()
		{
			_guid = new Guid();

			_extraData = new SortedDictionary<string, string>();
			_nistRecords = new SortedDictionary<string, List<string>>();
			scFileStop = Convert.ToChar(28).ToString();
			scGroupStop = Convert.ToChar(29).ToString();
			scRecordStop = Convert.ToChar(30).ToString();
			scUnitStop = Convert.ToChar(31).ToString();
		}

		/// <summary>This constructor is strictly for DEBUGGING.</summary>
		public NISTFileGenerator(string FileStop, string GroupStop, string RecordStop, string UnitStop)
		{
			_guid = new Guid();

			_nistRecords = new SortedDictionary<string, List<string>>();
			scFileStop = FileStop;
			scGroupStop = GroupStop;
			scRecordStop = RecordStop;
			scUnitStop = UnitStop;
		}

		public void Import(string File)
		{
			_nistRecords = new SortedDictionary<string, List<string>>();

			while (!string.IsNullOrEmpty(File))
			{
				string recordType = string.Empty;
				if (File.Contains(":"))
				{
					recordType = File.Substring(0, File.IndexOf(':', 0));

					
					File = File.Substring(recordType.Length + 1);
				}
				else
					throw new Exception(string.Format("({0}) File does not match NIST format. No Record Types in file.", _guid));

				string curFile = string.Empty;
				if (File.Contains(scFileStop))
				{
					curFile = File.Substring(0, File.IndexOf(scFileStop));
					File = File.Substring(curFile.Length + scFileStop.Length);
				}
				else
					throw new Exception(
						string.Format(
							"({0}) File does not match NIST format. No File Stops in file. Found after RecordType={1}",
							_guid.ToString(),
							recordType));

				while (!string.IsNullOrEmpty(curFile))
				{
					if(recordType.StartsWith("10.9"))
					{
						
					}
					string curGroup = string.Empty;
					if (curFile.Contains(scGroupStop))
					{
						curGroup = curFile.Substring(0, curFile.IndexOf(scGroupStop));
						curFile = curFile.Substring(curGroup.Length + scGroupStop.Length);
					}
					else
					{
						curGroup = curFile;
						curFile = string.Empty;
					}

					while (!string.IsNullOrEmpty(curGroup))
					{
						if (curGroup.Contains(scRecordStop))
						{
							string curRecord = curGroup.Substring(0, curGroup.IndexOf(scRecordStop));
							curGroup = curGroup.Substring(curRecord.Length + scRecordStop.Length);

							Add(recordType, curRecord);
						}
						else
						{
							Add(recordType, curGroup);
							curGroup = string.Empty;
						}
					}
					if (curFile.Contains(":"))
					{
						recordType = curFile.Substring(0, curFile.IndexOf(':', 0));
						curFile = curFile.Substring(recordType.Length + 1);
					}
				}
			}
		}

		/// <summary></summary>
		/// <param name="RecordType"></param>
		/// <param name="Values">If a Unit Stop (Chr(31)) is required separating different 
		/// parts of a record, be sure to include that in Value.</param>
		public void Add(string RecordType, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				List<string> values = new List<string>();
				values.Add(Value);
				Add(RecordType, values);
			}
		}

		/// <summary></summary>
		/// <param name="RecordType"></param>
		/// <param name="Values">If a Unit Stop (Chr(31)) is required separating different 
		/// parts of a record, be sure to include that in Value.</param>
		public void Add(string RecordType, string Value, int MaxLength)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				if (Value.Length > MaxLength)
					Value = Value.Substring(0, MaxLength);
				Add(RecordType, Value);
			}
		}

		/// <summary></summary>
		/// <param name="RecordType"></param>
		/// <param name="Values">If a Unit Stop (Chr(31) or this.scUnitStop) is required separating
		/// different parts of a record, be sure to include that in each Value in Values.</param>
		public void Add(string RecordType, List<string> Values)
		{
			if (_nistRecords.ContainsKey(RecordType))
			{
				//-----
				// If our RecordType already exists in the Dictionary,
				// append the value to the current list of values only
				// if they are actual values.
				//-----
				foreach (string s in Values)
					if (!string.IsNullOrEmpty(s))
						_nistRecords[RecordType].Add(s);

				//-----
				// If our RecordType has no values, remove it
				//-----
				if (_nistRecords[RecordType].Count == 0)
					Remove(RecordType);
			}
			else
			{
				//-----
				// Add only actual values.
				//-----
				List<string> newVals = new List<string>();
				foreach (string s in Values)
					if (!string.IsNullOrEmpty(s))
						newVals.Add(s);

				//-----
				// If no actual values were found, do not add
				// the RecordType to the Dictionary.
				//-----
				if (newVals.Count > 0)
					_nistRecords.Add(RecordType, newVals);
			}
		}

		/// <summary>Removes the Record Type and all values associated with it.</summary>
		public void Remove(string RecordType)
		{
			if (_nistRecords.ContainsKey(RecordType))
				_nistRecords.Remove(RecordType);
			else
				throw new MissingMemberException(
					string.Format("({0}) Record Type, {1}, does not exists within the given context.",
						_guid.ToString(),
						RecordType));
		}

		/// <summary>Removes only the specified value from a given Record Type. 
		/// If there are no values left in the given Record Type, remove the Record Type as well.</summary>
		public void Remove(string RecordType, string Value)
		{
			if (_nistRecords.ContainsKey(RecordType))
			{
				if (!_nistRecords[RecordType].Remove(Value))
					throw new MissingMemberException(
						string.Format("({0}) Value, {1}, does not exists within the given context.",
							_guid.ToString(),
							Value));
				if (_nistRecords[RecordType].Count == 0)
					_nistRecords.Remove(RecordType);
			}
			else
				throw new MissingMemberException(
					string.Format("({0}) Record Type, {1}, does not exists within the given context.",
						_guid.ToString(),
						RecordType));
		}

		public void Update(string RecordType, string Value)
		{
			List<string> values = new List<string>();
			values.Add(Value);
			Update(RecordType, values);
		}

		public void Update(string RecordType, List<string> Values)
		{
			_nistRecords[RecordType] = Values;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="RecordType"></param>
		/// <returns>
		/// The value of the record for a given RecordType.
		/// If the record is a group or does not exist,
		/// this will throw an AccessViolationException
		/// </returns>
		public string Value(string RecordType)
		{
			string retVal = string.Empty;
			if (_nistRecords.ContainsKey(RecordType))
			{
				if (_nistRecords[RecordType].Count == 0)
					throw new AccessViolationException(string.Format("RecordType={0} does not exist.", RecordType));
				else if (_nistRecords[RecordType].Count == 1)
					retVal = _nistRecords[RecordType][0];
				else if (_nistRecords[RecordType].Count > 1)
					throw new AccessViolationException(
						string.Format("RecordType={0} has a list of Values associated with it. Try .ValueList(\"{0}\")", RecordType));
			}
			else
				throw new AccessViolationException(string.Format("RecordType={0} does not exist.", RecordType));
			return retVal;
		}

		public List<string> ValueList(string RecordType)
		{
			if (_nistRecords.ContainsKey(RecordType))
				return _nistRecords[RecordType];
			else
				return null;
		}

		public bool ContainsKey(string RecordType)
		{
			return _nistRecords.ContainsKey(RecordType);
		}

		public override string ToString()
		{
			return this.Type1AndType2RecordsToString();
		}

		public string Type1AndType2RecordsToString()
		{
			//-----
			// This is item will let us know
			// when to exit the builder.
			//-----
			string exitString = "9999.999";
			Add(exitString, "999");

			StringBuilder outBuilder = new StringBuilder();
			StringBuilder recordBuilder = new StringBuilder();

			string curRecordType = "";
			foreach (KeyValuePair<string, List<string>> pair in _nistRecords)
			{
				//-----
				// Set the default stop character
				//-----
				string stopChar = scGroupStop;

				//-----
				// Set up the first record
				//-----
				if (string.IsNullOrEmpty(curRecordType))
				{
					stopChar = "";
					curRecordType = pair.Key.Substring(0, pair.Key.IndexOf(".") + 1);
				}

				//-----
				// Found the end of a record
				//-----
				if (!pair.Key.StartsWith("10.")) // Don't deal with Type-10 records
				{
					if (!pair.Key.StartsWith(curRecordType))
					{
						recordBuilder.Append(scFileStop);
						int recordLength = recordBuilder.Length;

						string recordSubType = "";
						if (_nistRecords.ContainsKey(curRecordType + "01"))
							recordSubType = "01";
						else if (_nistRecords.ContainsKey(curRecordType + "001"))
							recordSubType = "001";
						else
							throw new Exception(string.Format("({0}) No empty length record included " +
															  "(example: {1}01:0000000000 or {1}001:0000000000)",
															  _guid.ToString(),
															  curRecordType));

						//-----
						// Adjust the length field to hold the correct data.
						//-----
						int lenLength = _nistRecords[curRecordType + recordSubType].ToArray()[0].Length;
						string zeroes = "";
						zeroes = zeroes.PadLeft(lenLength, '0');
						curRecordType = curRecordType.TrimStart(new char[] { '0' });
						string recordString = recordBuilder.ToString().Replace(curRecordType + recordSubType + ":" + zeroes,
																			   curRecordType + recordSubType + ":" + recordLength.ToString().PadLeft(lenLength, '0'));
						outBuilder.Append(recordString);

						//-----
						// Found exit string
						//-----
						if (pair.Key == exitString)
							break;

						//-----
						// Set up the next record
						//-----
						curRecordType = pair.Key.Substring(0, pair.Key.IndexOf(".") + 1);
						recordBuilder = new StringBuilder();
						stopChar = "";
					}

					//-----
					// Handle most records
					//-----

					recordBuilder.Append(stopChar + pair.Key.TrimStart(new char[] { '0' }) + ":");
					stopChar = string.Empty;
					foreach (string s in pair.Value)
					{
						recordBuilder.Append(stopChar + s);
						stopChar = scRecordStop;
					}
				}
			}

			Remove(exitString);

			// .ToUpper() is very important here, as most 3rd parties are on older systems
			// that require upper case data to come over.
			return outBuilder.ToString().ToUpper();
		}

		public bool ContainsType10Records()
		{
			return _nistRecords.Keys.Where(x => x.StartsWith("10.")).Any();
		}

		public string[] Type10RecordStringsNoData()
		{
			string[] type10RecordStringsNoData = null;

			IEnumerable<string> type10Records = _nistRecords.Keys.Where(x => x.StartsWith("10."));
			if (type10Records.Any())
			{
				// New up the correct number
				type10RecordStringsNoData = new string[_nistRecords[type10Records.First()].Count()];
				string zeroedLengthString = string.Empty;

				// Form each Type-10 string, without the Data Bytes post-"10.999"
				foreach (string type10Key in type10Records)
				{
					if (type10Key == "10.001")
					{
						zeroedLengthString = _nistRecords[type10Key].First();
					}

					for (int j = 0; j < _nistRecords[type10Key].Count(); ++j)
					{
						if ("10.999" != type10Key)
						{
							type10RecordStringsNoData[j] =
								string.Format("{0}{1}:{2}{3}",
												type10RecordStringsNoData[j],
												type10Key,
												_nistRecords[type10Key][j],
												scGroupStop);
						}
						else // If we're at the "10.999" key, put the beginning of the string, but not the data bytes
						{
							type10RecordStringsNoData[j] = string.Format("{0}10.999:", type10RecordStringsNoData[j]);
						}
					}
				}

				// Update each Type-10 string with the correct length
				for (int j = 0; j < type10RecordStringsNoData.Length; ++j)
				{
					int imageBytesLength = Convert.FromBase64String(_nistRecords["10.999"][j]).Length;
					int type10StringLength = type10RecordStringsNoData[j].Length + imageBytesLength + 1; // LengthOfNon-Byte-Data + NumberOfBytes + FileStop character

					string updatedLengthString = type10StringLength.ToString(zeroedLengthString); // Zero padded, formatted length

					// Type-10 record string, post "10.001" (length) section
					//
					// From index = "'10.001:'" + Length + GroupStop character", to the end
					string endOfType10String = type10RecordStringsNoData[j].Substring("10.001:".Length + zeroedLengthString.Length + 1);

					type10RecordStringsNoData[j] =
						string.Format("10.001:{0}{1}{2}", updatedLengthString, scGroupStop, endOfType10String);
				}
			}

			return type10RecordStringsNoData;
		}

		public byte[] Type10RecordDataBytes(int index)
		{
			byte[] retVal = null;

			if (_nistRecords.Keys.Any(x => x == "10.999") && _nistRecords["10.999"].Count() > index)
			{
				retVal = Convert.FromBase64String(_nistRecords["10.999"][index]);
			}

			return retVal;
		}
	}
}
