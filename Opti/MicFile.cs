namespace Opti
{
    using System.Collections.Generic;

    public class MicFile : AsmFile<InstructionLine>
    {
        public MicFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Mic")
        { }

        public override IEnumerator<InstructionLine> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsWellStructured()
        {
            try
            {
                if (this.Content[0].TrimEnd() != "Y0")
                {
                    return false;
                }

                for (var i = 1; i < this.Content.Count; i++)
                {
                    var line = this.Content[i];

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        InstructionLine.ParseMic(line);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}