using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ComputerSimulation {
    public class Signal : Label{

        private bool on;
        public bool On {
            get {
                return on;
            }
            set {
                on = value;
                Changed?.Invoke(on);
                BackColor = on ? Color.White : Color.FromArgb(30, 30, 30);
                if(Chip != null && IsInput) Chip.Calculate();
            }
        }

        private bool isInput;
        public bool IsInput {
            get {
                return isInput;
            }
        }

        public List<Signal> Others = new List<Signal>();

        public delegate void ChangedDelegate(bool on);
        public ChangedDelegate Changed;

        public Chip Chip;

        public Signal(bool isInput, Chip Chip) {
            this.isInput = isInput;
            this.Chip = Chip;
            On = false;

            Size = new Size(30, 30);
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, Width, Height);
            Region = new Region(gp);
            Invalidate();
        }


    }
}