using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SQLite;
using KlerksSoft;

namespace TradeTraining
{
	class StockDataAdapter
	{
		int start;
		int size;
		long startTime;
		int step;

		Encoding encoding;
		string file;

		public StockDataAdapter(string dataFile)
		{
			file = dataFile;	
			Init(dataFile);
			SQLiteConnection connection = new SQLiteConnection("data source = data.db");
		}
		void Init(string file)
		{
			encoding = TextFileEncodingDetector.DetectTextFileEncoding(file, Encoding.Default);
			using(FileStream stream  = File.OpenRead(file))
			{
				byte[] data = new byte[256];
				stream.Read(data, 0, data.Length);
				string datastr = encoding.GetString(data);
				string[] strs = datastr.Split('\n');
				string title = strs[0];
				string line1 = strs[1];
				string line2 = strs[2];
				start = encoding.GetBytes(title+'\n').Length;
				size = encoding.GetBytes(line1+'\n').Length;
				var data1 = new StockData(line1);
				var data2 = new StockData(line2);
				step = (int)(data2.Time - data1.Time);
				startTime = data1.Time;
			}
		}
		public StockData[] GetData(long time, int count, int minuteCount = 1)
		{
			StockData[] datas = new StockData[count];
			int fileStart = start + size * ((int)(time - startTime) / step);
			int dataLength = size * count * minuteCount;	
			using(FileStream stream  = File.OpenRead(file))
			{
				stream.Seek(fileStart, SeekOrigin.Begin);
				for(int i=0; i<count; i++)
				{
					byte[] lineData = new byte[size];
					stream.Read(lineData, 0, lineData.Length);
					string line = encoding.GetString(lineData);
					datas[i] = new StockData(line);
				}
			}			
			return datas;
		}
	}
	class StockData
	{
		public StockData(string textData)
		{
			string[] strs = textData.Split(',');
			Ticker = strs[0];
			Time = long.Parse(strs[1]+strs[2]);
			Open = float.Parse(strs[3]);
			High = float.Parse(strs[4]);
			Low = float.Parse(strs[5]);
			Close = float.Parse(strs[6]);
			Volume = float.Parse(strs[7]);
		}
		public string Ticker {get; set;}
		public long Time {get; set;}
		public float Open {get; set;}
		public float High {get; set;}
		public float Low {get; set;}
		public float Close {get; set;}
		public float Volume {get; set;}
	}
}

/*
<TICKER>,<DTYYYYMMDD>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>
USDJPY,20010102,230300,114.43,114.43,114.43,114.43,4
USDJPY,20010102,230400,114.44,114.44,114.44,114.44,4
USDJPY,20010102,230500,114.44,114.44,114.44,114.44,4
USDJPY,20010102,230700,114.44,114.44,114.44,114.44,4
USDJPY,20010102,230800,114.44,114.44,114.44,114.44,4
USDJPY,20010102,230900,114.44,114.44,114.44,114.44,4
USDJPY,20010102,231100,114.44,114.45,114.44,114.45,4
USDJPY,20010102,231200,114.45,114.45,114.45,114.45,4
USDJPY,20010102,231300,114.45,114.45,114.43,114.43,4
USDJPY,20010102,231400,114.42,114.42,114.41,114.41,4
*/
