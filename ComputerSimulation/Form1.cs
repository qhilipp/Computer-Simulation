using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ComputerSimulation {
    public partial class Form1 : Form {

        private TextBox name;
        private Button add;
        private Label display;
        private Panel controls;

        private Chip chip;

        private Chip dragging = null;
        private Label draggingLabel = null;
        private Signal first = null;
        private Signal start = null;

        private bool cntrDown = false;
        private bool shiftDown = false;
        private bool altDown = false;

        private Graphics g;

        private List<List<Signal>> binarys = new List<List<Signal>>();

        public static string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ComputerSimulationSafe");

        public Form1() {

            #region UI Stuff
            FormBorderStyle = FormBorderStyle.None;
            Text = "Computer Simulation";
            Size = new Size(1920, 1080);
            BackColor = Color.FromArgb(15, 15, 15);
            KeyPreview = true;
            CenterToScreen();

            name = new TextBox();
            name.PlaceholderText = "Name";
            name.BorderStyle = BorderStyle.None;
            name.BackColor = Color.FromArgb(30, 30, 30);
            name.ForeColor = Color.White;
            name.Width = Width - 155;
            name.Font = new Font("Arial", 25);
            name.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            Controls.Add(name);

            add = new Button();
            add.Text = "Add";
            add.FlatStyle = FlatStyle.Flat;
            add.FlatAppearance.BorderSize = 0;
            add.BackColor = name.BackColor;
            add.ForeColor = Color.White;
            add.Font = name.Font;
            add.Width = 150;
            add.Height = name.Height;
            add.Left = Width - 150;
            add.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            add.Click += AddClicked;
            Controls.Add(add);

            display = new Label();
            display.BackColor = Color.FromArgb(19, 19, 19);
            display.Top = name.Height;
            display.Width = Width - 100;
            display.Height = Height - name.Height - 100;
            display.Left = (Width - display.Width) / 2;
            display.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            display.MouseDown += MousePressed;
            display.Paint += (Object sender, PaintEventArgs e) => {
                Timer t = new Timer();
                t.Interval = 50;
                t.Tick += (Object sender, EventArgs e) => {
                    DrawConnections();
                    t.Stop();
                };
                t.Start();
            };
            Controls.Add(display);
            
            controls = new Panel();
            controls.BackColor = name.BackColor;
            controls.Width = Width;
            controls.Height = 100;
            controls.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            controls.Top = name.Height + display.Height;
            controls.VerticalScroll.Maximum = 0;
            controls.AutoScroll = true;
            Controls.Add(controls);
            #endregion

            g = display.CreateGraphics();

            AddChip(new Not());
            AddChip(new And());
            if(!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            else {
                string[] files = Directory.GetFiles(Dir);
                foreach(string file in files) {
                    Chip c = new Chip(Path.GetFileNameWithoutExtension(file));
                    StreamReader sr = new StreamReader(file);
                    List<string> lines = new List<string>();
                    string s = sr.ReadLine();
                    while(s != null) {
                        lines.Add(s);
                        s = sr.ReadLine();
                    }
                    c.table = new bool[lines.Count, lines[0].Length];
                    for(int i = 0; i < c.table.GetLength(1); i++) {
                        for(int j = 0; j < c.table.GetLength(0); j++) {
                            c.table[j, i] = lines[j][i] == '1';
                        }
                    }
                    for(int i = 0; i < lines[0].Length; i++) c.Outputs.Add(new Signal(false, c));
                    for(int i = 0; i < Math.Log2(lines.Count); i++) c.Inputs.Add(new Signal(true, c));
                    sr.Close();
                    AddChip(c);
                }
            }
            Init();

            Timer t = new Timer();
            t.Interval = 30;
            t.Tick += (Object sender, EventArgs e) => {
                DrawConnections();
                if(draggingLabel != null) {
                    draggingLabel.Location = Cursor.Position - new Size(display.Left + Left, display.Top + Top - 5);
                }
            };
            t.Start();

        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if(e.KeyCode == Keys.ControlKey) cntrDown = true;
            if(e.KeyCode == Keys.ShiftKey) shiftDown = true;
            if(e.KeyCode == Keys.Menu) altDown = true;
            if(e.KeyCode == Keys.Up) {
                if(Cursor.Position.X < 300 + Left)
                    InOutPut(chip.Inputs);
                else if(Cursor.Position.X > Left + Width - 300)
                    InOutPut(chip.Outputs);
            }
        }
        protected override void OnKeyUp(KeyEventArgs e) {
            if(e.KeyCode == Keys.ControlKey) cntrDown = false;
            if(e.KeyCode == Keys.ShiftKey) shiftDown = false;
            if(e.KeyCode == Keys.Menu) altDown = false;
        }
        private void MousePressed(Object sender, MouseEventArgs e) {
            if(dragging != null) {
                Chip c = dragging.CreateNew();
                c.Calculate();
                chip.Chips.Add(c);
                dragging = null;
            }
            if(draggingLabel != null) draggingLabel = null;
            g.Clear(display.BackColor);
        }

        private void Connect(Signal s1, Signal s2) {
            if(s1.Others.Contains(s2)) {
                s1.Others.Remove(s2);
                s2.Others.Remove(s1);
                s1.On = false;
                s2.On = false;
                s1.Chip.Calculate();
                s2.Chip.Calculate();
                return;
            }
            if(s1.IsInput == s2.IsInput && s1.Chip == chip && s2.Chip == chip) 
                return;
            if(s1 == s2) 
                return;
            if(s1.IsInput == s2.IsInput && s1.Chip == s2.Chip && s1.Chip != chip) 
                return;            
            if(s1.IsInput && s2.IsInput && s1.Chip != chip && s2.Chip != chip) 
                return;          
            if(s1.Chip == s2.Chip && s1.Chip != chip) 
                return;            
            if(s1.IsInput && s1.Chip != chip && s1.Others.Count == 1) 
                return;
            if(s2.IsInput && s2.Chip != chip && s2.Others.Count == 1) 
                return;
            if(!s1.IsInput && s1.Chip != chip && !s2.IsInput && s2.Chip != chip) 
                return;
            s1.Others.Add(s2);
            s2.Others.Add(s1);
            if(s1.On) s2.On = true;
            if(s2.On) s1.On = true;
        }
        private void SignalConnect(Signal s) {
            if(first == null) {
                first = s;
                return;
            }
            display.CreateGraphics().Clear(display.BackColor);
            Connect(first, s);
            first = null;
        }

        private void DrawConnection(Signal s1, Signal s2) {
            Point pos1 = s1.Location + new Size(s1.Height / 2, s1.Height / 2);
            if(s1.Parent != display && s1.Parent != null) pos1 += new Size(s1.Parent.Left, s1.Parent.Top);
            Point pos2 = s2.Location + new Size(s1.Height / 2, s1.Height / 2);
            if(s2.Parent != display && s2.Parent != null) pos2 += new Size(s2.Parent.Left, s2.Parent.Top);
            g.DrawLine(new Pen(Color.White, 3), pos1, pos2);
        }
        private void DrawConnections() {
            Pen p = new Pen(Color.White, 3);
            if(first != null || (dragging == null && draggingLabel != null))
                g.Clear(display.BackColor);
            if(first != null) {
                Point a = first.Location + new Size(first.Height / 2, first.Height / 2);
                Point b = Cursor.Position - new Size(display.Left + Left, display.Top + Top);
                if(first.Parent != display) a += new Size(first.Parent.Left, first.Parent.Top);
                g.DrawLine(p, a, b);
            }
            for(int i = 0; i < display.Controls.Count; i++) {
                if(display.Controls[i].GetType() == typeof(ChipLabel)) {
                    ChipLabel cl = (ChipLabel) display.Controls[i];
                    foreach(Signal s in cl.Chip.Inputs) {
                        foreach(Signal s0 in s.Others) {
                            DrawConnection(s, s0);
                        }
                    }
                    foreach(Signal s in cl.Chip.Outputs) {
                        foreach(Signal s0 in s.Others) {
                            DrawConnection(s, s0);
                        }
                    }
                }
            }
            foreach(Signal s in chip.Inputs) {
                foreach(Signal s0 in s.Others) {
                    DrawConnection(s, s0);
                }
            }
        }
        private void DrawBinaryConnections() {
            CreateGraphics().Clear(BackColor);
            foreach(List<Signal> bin in binarys) {
                int val = 0;
                for(int i = 1; i < bin.Count; i++) {
                    if(bin[i].On) val += (int) Math.Pow(2, bin.Count - i - 1);
                }
                if(bin.Count > 0) {
                    if(bin[0].On) val -= (int) Math.Pow(2, bin.Count - 1);
                    Point a = new Point(bin[0].IsInput ? 40 : Width - 40, bin[0].Top + display.Top + 5);
                    Point b = new Point(bin[0].IsInput ? 40 : Width - 40, bin.Last().Top + display.Top + bin[0].Height - 5);
                    Graphics g = CreateGraphics();
                    g.DrawLine(new Pen(Color.White, 5), a, b);
                    g.DrawString(val + "", new Font("Arial", 15), new SolidBrush(Color.White), new Point(bin[0].IsInput ? 0 : Width - 40, a.Y + (b.Y - a.Y - 15) / 2));
                }
            }
        }

        private void AddClicked(Object sender, EventArgs e) {
            foreach(Label l in controls.Controls) {
                if(l.Text == name.Text) {
                    MessageBox.Show("This Name already exists.");
                    return;
                }
            }
            foreach(char z in name.Text) {
                if(z > 128) {
                    MessageBox.Show("This string is not valid");
                    return;
                }
            }
            chip.Name = name.Text;
            bool[,] table = new bool[(int) Math.Pow(2, chip.Inputs.Count), chip.Outputs.Count];
            for(int i = 0; i < table.GetLength(0); i++) {
                string binary = Convert.ToString(i, 2);
                while(binary.Length < chip.Inputs.Count)
                    binary = binary.Insert(0, "0");
                for(int j = 0; j < chip.Inputs.Count; j++) {
                    chip.Inputs[j].On = binary[j] == '1';
                    foreach(Signal temp in chip.Inputs[j].Others) temp.On = chip.Inputs[j].On;
                }
                for(int j = 0; j < chip.Outputs.Count; j++) {
                    table[i, j] = chip.Outputs[j].On;
                }
            }
            Chip c = chip.CreateNew();
            c.table = table;
            if(c.Safe() == -1) {
                MessageBox.Show("This name is invalid");
                return;
            }
            AddChip(c);
            Reset();
        }
        private void AddChip(Chip chip) {
            Label l = new Label();
            l.Text = chip.Name;
            l.BackColor = chip.Color;
            l.Size = new Size(100, 70);
            l.TextAlign = ContentAlignment.MiddleCenter;
            l.Font = new Font("Arial", 16);
            l.Top = (controls.Height - l.Height) / 2;
            l.Left = controls.Controls.Count * (l.Width + l.Top) + l.Top;
            l.Click += (Object sender, EventArgs e) => {
                Chip c = chip.CreateNew();
                ChipLabel cl = new ChipLabel(c);
                cl.Connect += (Signal lSender) => {
                    SignalConnect(lSender);
                };
                cl.Click += (Object sender, EventArgs e) => {
                    draggingLabel = cl;
                    Cursor.Position = new Point(cl.Left + display.Left + Left, cl.Top - 5 + display.Top + Top);
                };
                display.Controls.Add(cl);
                draggingLabel = cl;
                dragging = chip;
                c.Calculate();
            };
            controls.Controls.Add(l);
        }

        private bool[] GetTable(Signal s) {
            bool[] table = new bool[(int)Math.Pow(2, chip.Inputs.Count)];
            for(int i = 0; i < table.GetLength(0); i++) {
                string binary = Convert.ToString(i, 2);
                while(binary.Length < chip.Inputs.Count)
                    binary = binary.Insert(0, "0");
                for(int j = 0; j < chip.Inputs.Count; j++) {
                    chip.Inputs[j].On = binary[j] == '1';
                    foreach(Signal temp in chip.Inputs[j].Others) temp.On = chip.Inputs[j].On;
                }
                table[i] = s.On;
            }
            foreach(Signal l in chip.Inputs) {
                l.On = false;
                foreach(Signal temp in l.Others) {
                    temp.On = l.On;
                }
            }
            return table;
        }

        private void InOutPut(List<Signal> list) {
            Signal l = new Signal(list == chip.Inputs, null);
            l.Changed += (bool on) => {
                DrawBinaryConnections();
            };
            l.MouseDown += (Object sender, MouseEventArgs e) => {
                Signal l = (Signal) sender;
                if(e.Button != MouseButtons.Left) return;
                if(altDown && !l.IsInput) {
                    bool[] current = new bool[chip.Inputs.Count];
                    for(int i = 0; i < current.Length; i++) {
                        current[i] = chip.Inputs[i].On;
                    }
                    Table t = new Table(chip.Inputs.Count, GetTable(l));
                    for(int i = 0; i < current.Length; i++) {
                        chip.Inputs[i].On = current[i];
                    }
                    t.Show();
                    altDown = false;
                }
                else if(shiftDown) {
                    if(list.Count < 2 || l.Others.Count != 0) return;
                    foreach(List<Signal> bin in binarys)
                        if(bin.Contains(l)) return;
                    list.Remove(l);
                    display.Controls.Remove(l);
                    for(int i = 0; i < list.Count; i++) {
                        list[i].Left = list == chip.Inputs ? 0 : display.Width - l.Height;
                        list[i].Top = i * 40 + ((display.Height - (list.Count * 40 - 20)) / 2);
                    }
                    display.CreateGraphics().Clear(display.BackColor);
                    DrawBinaryConnections();
                }
                else if(cntrDown) {
                    if(start == null) {
                        start = l;
                        binarys.Add(new List<Signal>());
                    }
                    else {
                        if(start.Top > l.Top) {
                            Signal temp = start;
                            start = l;
                            l = temp;
                        }
                        foreach(List<Signal> bin in binarys)
                        if(bin.Contains(start) && bin.Contains(l)) {
                            binarys.Remove(binarys.Last());
                            binarys.Remove(bin);
                            DrawBinaryConnections();
                            start = null;
                            return;
                        }
                        bool found = false;
                        foreach(Signal s in list) {
                            if(s == start) found = true;
                            if(found) {
                                binarys.Last().Add(s);
                            }
                            if(s == l) found = false;
                        }
                        start = null;
                        DrawBinaryConnections();
                    }
                }else
                    SignalConnect(l);
            };
            l.Anchor = list == chip.Inputs ? AnchorStyles.Left : AnchorStyles.Right;
            l.Chip = chip;
            if(list == chip.Inputs) 
            l.MouseDown += (Object sender, MouseEventArgs e) => {
                if(e.Button == MouseButtons.Right) {
                    l.On = !l.On;
                    foreach(Signal temp in l.Others) {
                        temp.On = l.On;
                    }
                }
            };
            display.Controls.Add(l);
            list.Add(l);
            for(int i = 0; i < list.Count; i++) {
                list[i].Left = list == chip.Inputs ? 0 : display.Width - l.Height;
                list[i].Top = i * 40 + ((display.Height - (list.Count * 40 - 10)) / 2);
            }
            DrawBinaryConnections();
            display.Invalidate();
        }

        private void Init() {
            chip = new Chip("Temp");
            InOutPut(chip.Inputs);
            InOutPut(chip.Inputs);
            InOutPut(chip.Outputs);
        }
        private void Reset() {
            display.Controls.Clear();
            name.Text = "";
            binarys.Clear();
            Init();
        }

    }
}