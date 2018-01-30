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
		static string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
			LoadFromCsv(dataFile);
		}
		void LoadFromCsv(string csvFile)
		{
			string tableName = Path.GetFileNameWithoutExtension(csvFile);
			SQLiteConnection connection = new SQLiteConnection("data source = "+exedir+"//data.db");
			SQLiteCommand cmdCreateTable = connection.CreateCommand();
			cmdCreateTable.CommandText = 
				"create table if not exists t_"+tableName+
				"(time bigint primary key not null, "+
				"ticker nvarchar(30), open decimal(10,4), high decimal(10,4), "+
				"low decimal(10,4), close decimal(10,4), vol decimal(10,4))";
			connection.Open();
			cmdCreateTable.ExecuteNonQuery();
			connection.Close();
			

			connection.Open();
			string sqlInsert = "insert into t_" + tableName + " values (" + 
				":time, :ticker, :open, :high, :low, :close, :vol)";
			SQLiteTransaction trans = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			try{
				using(var reader = new StreamReader(csvFile))
				{
					string line;
					while((line = reader.ReadLine()) != null)
					{
						StockData data = StockData.Parse(line);
						if (data == null) continue;
						SQLiteCommand cmdInsert = connection.CreateCommand();
						cmdInsert.CommandText = sqlInsert;
						cmdInsert.Parameters.Add(new SQLiteParameter("time", data.Time));
						cmdInsert.Parameters.Add(new SQLiteParameter("ticker", data.Ticker));
						cmdInsert.Parameters.Add(new SQLiteParameter("open", data.Open));
						cmdInsert.Parameters.Add(new SQLiteParameter("high", data.High));
						cmdInsert.Parameters.Add(new SQLiteParameter("low", data.Low));
						cmdInsert.Parameters.Add(new SQLiteParameter("close", data.Close));
						cmdInsert.Parameters.Add(new SQLiteParameter("vol", data.Vol));
						cmdInsert.ExecuteNonQuery();
					}
				}
				trans.Commit();
			} 
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				trans.Rollback();
			}
			connection.Close();
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
		public string Ticker {get; set;}
		public long Time {get; set;}
		public float Open {get; set;}
		public float High {get; set;}
		public float Low {get; set;}
		public float Close {get; set;}
		public float Vol {get; set;}

		public StockData(string textData)
		{
			Parse(this, textData);
		}
		public StockData()
		{}
		public static bool CanParse(string line)
		{
			return Parse(line) != null;	
		}
		public static StockData Parse(string line)
		{
			StockData data = new StockData();
			try
			{
				Parse(data, line);
			}
			catch
			{
				return null;
			}
			return data;
		}
		public static void Parse(StockData data, string line)
		{
			string[] strs = line.Split(',');
			data.Ticker = strs[0];
			data.Time = long.Parse(strs[1]+strs[2]);
			data.Open = float.Parse(strs[3]);
			data.High = float.Parse(strs[4]);
			data.Low = float.Parse(strs[5]);
			data.Close = float.Parse(strs[6]);
			data.Vol = float.Parse(strs[7]);
		}
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
