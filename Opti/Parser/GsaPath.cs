namespace Opti.Parser
{
    using System;
    using System.Collections.Generic;

    public class GsaPath
    {
        public int Source { get; }

        public List<GsaLine> Path { get; }

        public GsaPath(int source)
        {
            this.Source = source;
            this.Path = new List<GsaLine>();
        }

        public override string ToString()
        {
            return @$"GsaPath: [ Source={this.Source}
{string.Join(Environment.NewLine, this.Path)} ]";
        }
    }
}