using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NeuralNetworks2.API.Model;
using System.Collections.Generic;

namespace NeuralNetworks2.Logic {
    /// <summary>
    /// Obsluga gnuplota
    /// </summary>
    class GnuPlot {
        private const string GNU_PLOT_PATH = @"d:\tools\gnuplot\binary\gnuplot.exe";

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

        /// <summary>
        /// Dodaje punkty (x,y1) i (x,y2) do wykresu
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddDataPoint(double x, double yLearnSet, double yTestSet) {
            if (_plotDataFile == null) {
                throw new InvalidOperationException("First call BeginPlot()");
            }

            _plotDataFile.WriteLine(
                String.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}", x, yLearnSet, yTestSet)
                );
        }

        public void AddAlgorithmParamsToTitle(AlgorithmParams algParams) {
            string paramsString = String.Format(
                CultureInfo.InvariantCulture,
                " [e: {0:F3}, m: {1:F3}, mfcc: {2}, sigfr: {3}, tc: {4:F3}]",
                algParams.LearningRate,
                algParams.Momentum,
                algParams.MfccCount,
                algParams.SignalFramesCount,
                algParams.TCoef);
            _plotTitle += paramsString;
        }

        private void ExecuteGnuPlotCommands(string commands) {
            ProcessStartInfo psi = new ProcessStartInfo(
                GNU_PLOT_PATH,
                String.Format(@"-e ""{0}""", commands)
                );

            psi.UseShellExecute = false;

            Process proc = Process.Start(psi);
            proc.WaitForExit(8000);
        }

        public void EndPlot() {
            _plotDataFile.Close();

            string commandString = String.Format(
                @"set terminal png;set output '{0}';set grid;set title '{1}';{3}plot '{2}' notitle with lines",
                _plotPath, _plotTitle, _tmpFileName,
                (_normYRange) ? String.Format(CultureInfo.InvariantCulture, "set yrange [{0}:{1}];", _minYRange, _maxYRange) : ""
                );
            ExecuteGnuPlotCommands(commandString);

            File.Delete(_tmpFileName);
        }

        /// <summary>
        /// Tworzy 2 wykres (2 wyresy na 1dnym)
        /// </summary>
        public void End2Plot() {
            _plotDataFile.Close();

            string commandString = String.Format(
                @"set terminal png;set output '{0}';set title '{1}';set grid;{3}plot '{2}' using 1:2 title 'Zbior uczacy' with lines lt rgb 'red', '{2}' using 1:3 title 'Zbior testowy' with lines lt rgb 'blue'",
                _plotPath, _plotTitle, _tmpFileName,
                (_normYRange) ? String.Format(CultureInfo.InvariantCulture, "set yrange [{0}:{1}];", _minYRange, _maxYRange) : ""
                );
            ExecuteGnuPlotCommands(commandString);

            File.Delete(_tmpFileName);
        }
    }
}
