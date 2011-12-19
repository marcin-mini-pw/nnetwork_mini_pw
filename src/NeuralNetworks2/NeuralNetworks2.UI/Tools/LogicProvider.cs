using NeuralNetworks2.API.Logic;
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


        public IFileIOLogic FileIOLogic { get; private set; }
        public IAlgorithmsLogic AlgorithmsLogic { get; set; }


        private LogicProvider()
        {
            FileIOLogic = new FileIOLogic();
            AlgorithmsLogic = new AlgorithmsLogic();
        }
    }
}