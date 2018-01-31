using System;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace TradeTraining
{
	class DataManager
	{
		string dbfile;
		SQLiteConnection connection;
		public SQLiteConnection Connection
		{
			get 
			{
				if (connection == null) 
					connection = new SQLiteConnection("data source = "+dbfile);
				return connection;
			}
		}
		public DataManager(string dbfile)
		{
			this.dbfile = dbfile;
		}
		public StockData CalculateData(string ticker, long time, int count)
		{
			SQLiteCommand cmd = Connection.CreateCommand();
			string sql = 
@"
select ts.ticker, max(time) as time, ts.open, round(max(high), 4) as high, 
round(min(low), 4) as low, te.close, sum(vol) as vol from (
  select * from {0} where time >= :time order by time limit :count
) 
cross join (
  select ticker, open from {0} where time = :time
) ts
cross join (
  select close from {0} where time >= :time order by time limit 1 offset (:count-1)
) te";
			cmd.CommandText = string.Format(sql, GetTableName(ticker));
			cmd.Parameters.Add(new SQLiteParameter("time", time));
			cmd.Parameters.Add(new SQLiteParameter("count", count));
			Connection.Open();
			SQLiteDataReader reader = cmd.ExecuteReader();
			reader.Read();
			StockData data = CreateStockData(reader);
			Connection.Close();
			return data;
		}
		public StockData[] GetData(string ticker, long time, int count)
		{
			StockData[] dataArr = new StockData[count];
			SQLiteCommand cmd = Connection.CreateCommand();
			cmd.CommandText = "select * from " + GetTableName(ticker) + 
				" where time >= :time order by time limit :count";
			cmd.Parameters.Add(new SQLiteParameter("time", time));
			cmd.Parameters.Add(new SQLiteParameter("count", count));
			connection.Open();
			SQLiteDataReader reader = cmd.ExecuteReader();
			int i = 0;
			while(reader.Read())	
			{
				dataArr[i++] = CreateStockData(reader);
			}
			connection.Close();	
			return dataArr;
		}
		StockData CreateStockData(SQLiteDataReader reader)
		{
			StockData data = new StockData();
			data.Ticker = reader.GetString(reader.GetOrdinal("ticker"));
			data.Time = reader.GetInt64(reader.GetOrdinal("time"));
			data.Open = reader.GetFloat(reader.GetOrdinal("open"));
			data.High = reader.GetFloat(reader.GetOrdinal("high"));
			data.Low = reader.GetFloat(reader.GetOrdinal("Low"));
			data.Close = reader.GetFloat(reader.GetOrdinal("Close"));
			data.Vol = reader.GetFloat(reader.GetOrdinal("vol"));
			return data;
		}
		public void ImportFromCsv(string csvFile)
		{
			string tableName = GetTableName(Path.GetFileNameWithoutExtension(csvFile));
			SQLiteCommand cmdCreateTable = Connection.CreateCommand();
			cmdCreateTable.CommandText = 
				"create table if not exists "+tableName+
				"(time bigint primary key not null, "+
				"ticker nvarchar(30), open decimal(10,4), high decimal(10,4), "+
				"low decimal(10,4), close decimal(10,4), vol decimal(10,4))";
			Connection.Open();
			cmdCreateTable.ExecuteNonQuery();
			Connection.Close();
			

			Connection.Open();
			string sqlInsert = "insert into " + tableName + " values (" + 
				":time, :ticker, :open, :high, :low, :close, :vol)";
			SQLiteTransaction trans = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
			try{
				using(var reader = new StreamReader(csvFile))
				{
					string line;
					while((line = reader.ReadLine()) != null)
					{
						StockData data = StockData.Parse(line);
						if (data == null) continue;
						SQLiteCommand cmdInsert = Connection.CreateCommand();
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
			Connection.Close();
		}
		public string GetTableName(string ticker)
		{
			return "t_"+ticker+"_1m";
		}
	}
}
