using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeTraining
{
	class MainForm : Form
	{
		Panel canvas = new Panel();
		public MainForm()
		{
			canvas.BackColor = Color.White;
			canvas.Paint += delegate(object sender, PaintEventArgs e)
			{
				e.Graphics.DrawLine(new Pen(Color.Black, 3), 0, 0, 90, 90);
			};
			Controls.Add(canvas);
		}
	}
}