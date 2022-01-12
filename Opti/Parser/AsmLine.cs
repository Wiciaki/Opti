namespace Opti.Parser
{
    public abstract class AsmLine
    {
        public string Instruction { get; }

        public abstract override string ToString();

        protected AsmLine(string instruction)
        {
            this.Instruction = instruction;
        }
    }
}