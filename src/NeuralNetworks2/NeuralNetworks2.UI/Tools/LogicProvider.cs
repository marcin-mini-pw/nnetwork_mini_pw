using NeuralNetworks2.API;
using NeuralNetworks2.Logic;

namespace NeuralNetworks2.UI.Tools
{
    public sealed class LogicProvider
    {
        private static volatile LogicProvider instance = null;
        private static readonly object instanceLock = new object();

        public static LogicProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new LogicProvider();
                        }
                    }
                }
                return instance;
            }
        }


        public IAudioLogic AudioLogic { get; private set; }
        //NOTE: tutaj dodać kolejne


        private LogicProvider()
        {
            AudioLogic = new AudioLogic();
            //TODO: tutaj dodać kolejne
        }
    }
}
