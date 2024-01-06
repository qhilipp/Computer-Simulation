namespace ComputerSimulation {
    public class And : Chip{

        public And() : base("And") {
            Inputs.Add(new Signal(true, this));
            Inputs.Add(new Signal(true, this));
            Outputs.Add(new Signal(false, this));
            Calculate();
        }

        public override void Calculate() {
            if(Outputs.Count == 1 && Inputs.Count == 2) {
                Outputs[0].On = Inputs[0].On && Inputs[1].On;
                Send();
            }
        }

        public override Chip CreateNew() {
            return new And() { Color = Color };
        }

    }
}