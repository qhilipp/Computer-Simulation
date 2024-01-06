using System;
using System.Windows.Forms;
using System.Drawing;

namespace ComputerSimulation {
    public class Table : Form{

        Font f = new Font("Arial", 20);

        public Table(int inputs, bool[] vals) {

            Text = "Truthtable";
            MaximumSize = new Size(400, 700);
            Size = new Size((inputs + 1) * f.Height * 2 + 40, (int) ((Math.Pow(2, inputs) + 1) * f.Height * 2) + 40);
            BackColor = Color.FromArgb(19, 19, 19);
            TopMost = true;
            AutoScroll = true;
            CenterToParent();
            
            Panel display = new Panel();
            display.Paint += (Object sender, PaintEventArgs e) => {
                for(int i = 0; i < inputs; i++) {
                    e.Graphics.DrawString((char) (65 + i) + "", f, new SolidBrush(Color.DarkGray), i * f.Height * 2 + f.Height / 2, 0);
                    e.Graphics.DrawLine(new Pen(Color.FromArgb(50, 50, 50), 2), new Point((i + 1) * f.Height * 2, 5), new Point((i + 1) * f.Height * 2, display.Height - 5));
                }
                e.Graphics.DrawLine(new Pen(Color.FromArgb(50, 50, 50), 2), new Point(5, (int) (f.Height * 1.5)), new Point(display.Width - 5, (int) (f.Height * 1.5)));
                for(int i = 0; i < Math.Pow(2, inputs); i++) {
                    string binary = Convert.ToString(i, 2);
                    while(binary.Length < inputs)
                        binary = binary.Insert(0, "0");
                    for(int j = 0; j < binary.Length; j++) {
                        e.Graphics.DrawString(binary[j] + "", f, new SolidBrush(Color.White), j * f.Height * 2 + f.Height / 2, (i + 1) * f.Height * 2);
                    }
                }
                e.Graphics.DrawString("O", f, new SolidBrush(Color.DarkGray), inputs * f.Height * 2 + f.Height / 2, 0);
                for(int i = 0; i < vals.Length; i++) {
                    e.Graphics.DrawString(vals[i] ? "1" : "0", f, new SolidBrush(Color.White), inputs * f.Height * 2 + f.Height / 2, (i + 1) * f.Height * 2);
                }
            };
            display.Size = new Size((inputs + 1) * f.Height * 2, (int) ((Math.Pow(2, inputs) + 1) * f.Height * 2));
            Controls.Add(display);

        }

    }
}