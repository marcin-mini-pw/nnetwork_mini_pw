using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.Logic {
    /// <summary>
    /// Obsluga gnuplota
    /// </summary>
    class GnuPlot {
        private const string GNU_PLOT_PATH = @"D:\TOOLS\gnuplot\binary\gnuplot.exe";

        private string _plotTitle;
        private string _plotPath;
        private string _tmpFileName;
        private bool _normYRange;
        private double _minYRange, _maxYRange;
        private StreamWriter _plotDataFile;

        public GnuPlot(string plotTitle, string plotFilePath) {
            _plotTitle = plotTitle;
            _plotPath = plotFilePath;
            _normYRange = false;
        }

        public void SetYRange(double min, double max) {
            _normYRange = true;
            _minYRange = min;
            _maxYRange = max;
        }

        public void BeginPlot() {
            _tmpFileName = Path.GetTempFileName();
            _plotDataFile = new StreamWriter(_tmpFileName);
        }

        /// <summary>
        /// Dodaje punkt (x,y) do wykresu
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddDataPoint(double x, double y) {
            if (_plotDataFile == null) {
                throw new InvalidOperationException("First call BeginPlot()");
            }

            _plotDataFile.WriteLine(
                String.Format(CultureInfo.InvariantCulture, "{0}\t{1}", x, y)
                );
        }

        public void AddAlgorithmParamsToTitle(AlgorithmParams algParams) {
            string paramsString = String.Format(
                " [e: {0:F3}, m: {1:F3}, mfcc: {2}, sigfr: {3}, tc: {4:F3}]",
                algParams.LearningRate,
                algParams.Momentum,
                algParams.MfccCount,
                algParams.SignalFramesCount,
                algParams.TCoef);
            _plotTitle += paramsString;
        }

        public void EndPlot() {
            _plotDataFile.Close();

            ProcessStartInfo psi = new ProcessStartInfo(
                GNU_PLOT_PATH,
                String.Format(
                    @"-e ""set terminal png;set output '{0}';set title '{1}';{3}plot '{2}' notitle with lines""",
                    _plotPath, _plotTitle, _tmpFileName,
                    (_normYRange) ? String.Format(CultureInfo.InvariantCulture, "set yrange [{0}:{1}];", _minYRange, _maxYRange) : ""
                    )
                );

            psi.UseShellExecute = false;

            Process proc = Process.Start(psi);
            proc.WaitForExit(8000);

            File.Delete(_tmpFileName);
        }

        /// <summary>
        /// Tworzy wykres w formacie png na podstawie danych z pliku dataFileName.
        /// Zapisuje wykres pod nazwa plotFileName.
        /// </summary>
        /// <param name="dataFileName"></param>
        /// <param name="plotFileName"></param>
        public static void Plot(string dataFileName, string plotFileName) {
            
        }
    }
}
