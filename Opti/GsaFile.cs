namespace Opti
{
    public class GsaFile : AsmFile
    {
        public GsaFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Gsa")
        {

        }

        public override bool IsWellStructured()
        {

            return true;
        }
    }
}