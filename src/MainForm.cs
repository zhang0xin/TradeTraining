using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeTraining
{
	class MainForm : Form
	{
		Panel canvas = new Panel();
		StockDataAdapter adapter = new StockDataAdapter("data/usdjpy.txt");
		public MainForm()
		{
			StockData[] datas = adapter.GetData(20010102230500L, 5);
			canvas.BackColor = Color.White;
			canvas.Dock = DockStyle.Fill;
			canvas.Paint += delegate(object sender, PaintEventArgs e)
			{
				e.Graphics.DrawLine(new Pen(Color.Black, 3), 0, 0, 90, 90);
			};
			Controls.Add(canvas);
		}
	}
}