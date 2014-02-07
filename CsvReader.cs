#region License

/*-----------------------------------------------------------------------------
Version: 1.0.0.0
Blog: http://oninoiori.blog72.fc2.com/
DL Site: http://www48.tok2.com/home/oninonando/
Author: Ao-Oni <ao-oni@mail.goo.ne.jp>
Licensed under The MIT License
Redistributions of files must retain the above copyright notice.
-----------------------------------------------------------------------------*/

#endregion

#region CsvReader

#region using

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

#endregion

namespace AoOni
{
	#region CsvReader

	public class CsvReader : IDisposable
	{
		#region Definition
		
		private TextReader _reader;
		public bool EndOfData { private set; get; }
		private long _lineNumber = 0;
		public long LineNumber { get { return _lineNumber; } }
		private string _delimiter = ",";
		public string Delimiter
		{
			set
			{
				if(value.Length == 0)
					throw new Exception("invalid delimiter");

				if (value.Contains(CON_DOUBLE_QUOTE))
					throw new Exception("invalid delimiter");

				_delimiter = value;
			}
			get { return _delimiter; }
		}
		private bool _leaveOpen = false;
		public bool LeaveOpen
		{
			set { _leaveOpen = value; }
			get { return _leaveOpen; }
		}
		private bool _readerIsClosed = false;

		private const string CON_DOUBLE_QUOTE_DOUBLE = "\"\"";
		private const string CON_DOUBLE_QUOTE = "\"";

		#endregion

		#region Constructor

		public CsvReader(string path) : this(path, Encoding.UTF8) { }
		public CsvReader(string path, Encoding encoding)
		{
			_reader = new StreamReader(path, encoding);
		}
		public CsvReader(Stream stream) : this(stream, Encoding.UTF8) { }
		public CsvReader(Stream stream, Encoding encoding)
		{
			_reader = new StreamReader(stream, encoding);
		}
		public CsvReader(TextReader reader)
		{
			_reader = reader;
		}

		#endregion

		#region Public Methods

		#region Read Fields

		public string[] ReadFields()
		{
			var line = _reader.ReadLine();
			if (line == null)
			{
				EndOfData = true;
				return null;
			}

			_lineNumber++;

			return SplitLine(line);
		}

		#endregion

		#region Close

		public void Close()
		{
			if (_reader != null)
				_reader.Close();

			_readerIsClosed = true;
		}

		public void Dispose()
		{
			if(!_leaveOpen)
				if (!_readerIsClosed && _reader != null)
					_reader.Close();
		}

		#endregion

		#endregion

		#region Private Method

		#region SplitLine

		private string[] SplitLine(string line)
		{
			int indexOf;
			int seekPos = 0;
			var cols = new List<string>();

			while (seekPos < line.Length)
			{
				if (line.Substring(seekPos, 1) == CON_DOUBLE_QUOTE)
				{
					var seekPosTemp = seekPos;
					while (seekPosTemp < line.Length)
					{
						indexOf = line.IndexOf(CON_DOUBLE_QUOTE, seekPosTemp + 1);
						if (indexOf == -1)
							throw new Exception("closing quotation mark is not found");

						if (indexOf == (line.Length - 1))
						{
							cols.Add(line.Substring(seekPos + 1, (indexOf - seekPos - 1)).Replace(CON_DOUBLE_QUOTE_DOUBLE, CON_DOUBLE_QUOTE));
							seekPos = line.Length;
							break;
						}

						if (line.Substring(indexOf + 1, 1) != CON_DOUBLE_QUOTE)
						{
							if (line.Substring(indexOf + 1, _delimiter.Length) != _delimiter)
								throw new Exception("invalid closing quotation");

							cols.Add(line.Substring(seekPos + 1, (indexOf - seekPos - 1)).Replace(CON_DOUBLE_QUOTE_DOUBLE, CON_DOUBLE_QUOTE));
							seekPos = indexOf + _delimiter.Length + 1;
							break;
						}

						seekPosTemp = indexOf + 1;
					}
				}
				else if (line.Substring(seekPos, _delimiter.Length) == _delimiter)
				{
					cols.Add(string.Empty);
					seekPos = seekPos + _delimiter.Length;
				}
				else
				{
					indexOf = line.IndexOf(_delimiter, seekPos);
					if (indexOf == -1)
					{
						cols.Add(line.Substring(seekPos));
						seekPos = line.Length;
					}
					else
					{
						cols.Add(line.Substring(seekPos, (indexOf - seekPos)));
						seekPos = indexOf + 1;
					}
				}
			}

			return cols.ToArray();
		}

		#endregion

		#endregion
	}

	#endregion
}

#endregion
