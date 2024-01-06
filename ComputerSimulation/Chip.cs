using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ComputerSimulation {
    public class Chip {

        private Color color;
        public Color Color {
            get {
                return color;
            }
            set {
                color = value;
            }
        }

        private string name;
        public string Name {
            get {
                return name;
            }
            set {
                name = value;
                double sum = 0;
                foreach(char c in value) {
                    sum += c;
                }
                int max = value.Length * 128;
                sum /= max;
                sum *= 16581375;
                string html = ((int) sum).ToString("X");
                color = ColorTranslator.FromHtml("#" + html);
            }
        }

        public List<Signal> Inputs = new List<Signal>();
        public List<Signal> Outputs = new List<Signal>();

        public List<Chip> Chips = new List<Chip>();

        public bool[,] table;

        public Chip(string name) {
            Name = name;
        }

        public virtual void Calculate() {
            if(table == null) return;
            int index = 0;
            for(int i = 0; i < Inputs.Count; i++)
                if(Inputs[Inputs.Count - i - 1].On) index += (int) Math.Pow(2, i);

            for(int i = 0; i < Outputs.Count; i++)
                Outputs[i].On = table[index, i];

            Send();
        }

        public void Send() {
            foreach(Signal o in Outputs)
                foreach(Signal s in o.Others)
                    s.On = o.On;
        }

        public virtual Chip CreateNew() {
            Chip c = new Chip(name);
            c.Color = color;
            for(int i = 0; i < Inputs.Count; i++) {
                c.Inputs.Add(new Signal(true, c));
            }
            for(int i = 0; i < Outputs.Count; i++) {
                c.Outputs.Add(new Signal(false, c));
            }
            if(table != null) {
                c.table = new bool[table.GetLength(0), table.GetLength(1)];
                for(int y = 0; y < table.GetLength(1); y++) {
                    for(int x = 0; x < table.GetLength(0); x++) {
                        c.table[x, y] = table[x, y];
                    }
                }
            }
            return c;
        }

        public int Safe() {
            StreamWriter sw = null;
            string path = Path.Combine(Form1.Dir, name + ".txt");
            try {
                sw = new StreamWriter(path);
            }catch(Exception e) {
                return -1;
            }
            for(int j = 0; j < table.GetLength(0); j++) {
                for(int i = 0; i < table.GetLength(1); i++) {
                    sw.Write(table[j, i] ? 1 : 0);
                }
                sw.WriteLine();
            }
            sw.Close();
            return 0;
        }

    }
}