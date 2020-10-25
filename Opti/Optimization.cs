namespace Opti
{
    public abstract class Optimization
    {
        private readonly AsmCoordinator coordinator;

        protected GsaFile Gsa => this.coordinator.Gsa;

        protected TxtFile Txt => this.coordinator.Txt;

        protected MicFile Mic => this.coordinator.Mic;

        protected Optimization(AsmCoordinator coordinator) => this.coordinator = coordinator;
    }
}