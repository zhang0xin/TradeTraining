using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace TradeTraining
{
	class MainForm : Form
	{
		static string exedir = Path.GetDirectoryName(
			System.Reflection.Assembly.GetExecutingAssembly().Location);
		Panel canvas = new Panel();
		DataManager dm = new DataManager(Path.Combine(exedir, "data.db"));	

		public MainForm()
		{
			//dm.ImportFromCsv(Path.Combine(exedir, "USDJPY2001.txt"));
			StockData[] dataArr = dm.GetData("usdjpy2001", 20010103160600L, 10);
			foreach(StockData data in dataArr)
			{
				Console.WriteLine(data);
			}
			StockData totaldata = dm.CalculateData("usdjpy2001", 20010103160600L, 10);
			Console.WriteLine("total: "); 
			Console.WriteLine(totaldata);
			canvas.BackColor = Color.White;
			canvas.Dock = DockStyle.Fill;
			canvas.Paint += delegate(object sender, PaintEventArgs e)
			{
				//e.Graphics.DrawLine(new Pen(Color.Black, 3), 0, 0, 90, 90);
			};
			Controls.Add(canvas);
		}
		void DrawStock(Graphics graphics, StockData[] dataArr)
		{

		}
	}
}