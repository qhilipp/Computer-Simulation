namespace ComputerSimulation {
    public class Not : Chip {

        public Not() : base("Not"){
            Inputs.Add(new Signal(true, this));
            Outputs.Add(new Signal(false, this));
            Calculate();
        }

        public override void Calculate() {
            if(Outputs.Count == 1 && Inputs.Count == 1) {
                Outputs[0].On = !Inputs[0].On;
                Send();
            }
        }

        public override Chip CreateNew() {
            return new Not() { Color = Color };
        }

    }
}