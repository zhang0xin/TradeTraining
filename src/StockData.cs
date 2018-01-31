namespace TradeTraining
{
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
		public override string ToString()
		{
			return string.Format(
				"ticker: {0}, time: {1}, open: {2}, high: {3}, low: {4}, close: {5}, vol: {6}",
				Ticker, Time, Open, High, Low, Close, Vol);
		}
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