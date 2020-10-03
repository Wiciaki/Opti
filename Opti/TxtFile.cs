namespace Opti
{
    public class TxtFile : AsmFile
    {
        public TxtFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Txt")
        {
            
        }

        public override bool IsWellStructured()
        {
            return true;
        }
    }
}