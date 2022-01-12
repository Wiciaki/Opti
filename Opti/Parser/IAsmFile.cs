namespace Opti.Parser
{
    using System.Collections;

    public interface IAsmFile : IEnumerable, IVerifiable
    {
        string Name { get; }

        string Extension { get; }

        string[] Input { get; }

        string[] GetContent();
    }
}