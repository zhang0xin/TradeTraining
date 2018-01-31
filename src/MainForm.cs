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
			Width=800;
			Height=600;
			// dm.ImportFromCsv(Path.Combine(exedir, "USDJPY2001.txt"));
			StockData[] dataArr = dm.GetData("usdjpy2001", 20010103000000L, 200);
			StockData totaldata = dm.CalculateData("usdjpy2001", 20010103000000L, 200);
			canvas.BackColor = Color.White;
			canvas.Dock = DockStyle.Fill;
			canvas.Paint += delegate(object sender, PaintEventArgs e)
			{
				//e.Graphics.DrawLine(new Pen(Color.Black, 3), 0, 0, 90, 90);
				SizeF unit = new SizeF(canvas.Width/dataArr.Length, 
					canvas.Height/Math.Abs(totaldata.High - totaldata.Low));
				DrawStock(e.Graphics, unit, totaldata, dataArr);
			};
			Controls.Add(canvas);
		}
		void DrawStock(Graphics graphics, SizeF unit, StockData totalData, StockData[] dataArr)
		{
			Pen pen = new Pen(Color.Black, 1);
			float startY = totalData.Low;
			for(int i=0; i<dataArr.Length; i++)
			{
				StockData data = dataArr[i];

				graphics.DrawRectangle(pen, 
					i*unit.Width, (data.Open-startY)*unit.Height, 
					unit.Width, unit.Height*Math.Abs(data.Open-data.Close)); 
				Console.WriteLine("t:" + totalData.High+ "," + totalData.Low + "," + Math.Abs(totalData.High - totalData.Low));
				Console.WriteLine(data.Open);

				graphics.DrawLine(pen, i*unit.Width+unit.Width/2, (data.High-startY)*unit.Height, 
					i*unit.Width+unit.Width/2, unit.Height*(data.Low-startY)); 
				//Console.WriteLine(unit);
			}
		}
	}
}