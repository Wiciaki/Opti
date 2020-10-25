namespace Opti
{
    public abstract class AsmLine
    {
        public string Name { get; }

        protected AsmLine(string name)
        {
            this.Name = name;
        }
    }
}
