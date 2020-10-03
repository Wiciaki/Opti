namespace Opti
{
    public class MicFile : AsmFile
    {
        public MicFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Mic")
        {

        }

        public override bool IsWellStructured()
        {
            return true;
        }
    }
}