using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace ComputerSimulation {
    public class ChipLabel : Label{

        List<Signal> inputs = new List<Signal>();
        List<Signal> outputs = new List<Signal>();

        public delegate void ClickedDelegate(Signal sender);
        public ClickedDelegate Connect;

        public Chip Chip;

        public ChipLabel(Chip chip) {

            Chip = chip;
            int max = Math.Max(chip.Inputs.Count, chip.Outputs.Count) * 40;
            Size = new Size((int) (chip.Name.Length * Font.Height + 60), max < 60 ? 60 : max);
            Text = chip.Name;
            BackColor = chip.Color;
            Font = new Font("Arial", 16);
            TextAlign = ContentAlignment.MiddleCenter;
            Anchor = AnchorStyles.None;

            for(int i = 0; i < chip.Inputs.Count; i++) 
                InOutPut(inputs, true, chip.Inputs[i]);
            for(int i = 0; i < chip.Outputs.Count; i++)
                InOutPut(outputs, true, chip.Outputs[i]);

        }

        private void InOutPut(List<Signal> list, bool add, Signal s) {
            if(!add) {
                if(list.Count < 2) return;
                Controls.Remove(list[0]);
                list.RemoveAt(0);
            }
            else {
                s.Click += (Object sender, EventArgs e) => {
                    Connect?.Invoke((Signal) sender);
                };
                s.Anchor = list == inputs ? AnchorStyles.Left : AnchorStyles.Right;
                Controls.Add(s);
                list.Add(s);
            }
            for(int i = 0; i < list.Count; i++) {
                list[i].Left = list == inputs ? 0 : Width - s.Height;
                list[i].Top = i * 40 + ((Height - (list.Count * 40 - 10)) / 2);
            }
        }


    }
}