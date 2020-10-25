namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GsaFile : AsmFile<GsaLine>
    {
        public GsaFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Gsa")
        { }

        public int GetCount()
        {
            return int.Parse(this.Content[0].TrimEnd());
        }

        public override IEnumerator<GsaLine> GetEnumerator()
        {
            return this.Content.Skip(1).Take(this.GetCount() + 1).Select(GsaLine.Parse).GetEnumerator();
        }

        public override bool IsWellStructured()
        {
            try
            {
                var count = this.GetCount();
                
                for (var i = 1; i < this.Content.Count; i++)
                {
                    var line = this.Content[i];

                    if (i <= count + 1)
                    {
                        GsaLine.Parse(line);
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}