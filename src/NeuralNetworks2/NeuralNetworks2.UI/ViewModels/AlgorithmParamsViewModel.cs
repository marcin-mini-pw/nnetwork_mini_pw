using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;
using NeuralNetworks2.Logic;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// ViewModel dla widoku z parametrami sieci neuronowych.
    /// </summary>
    public class AlgorithmParamsViewModel : BaseViewModel
    {
        private readonly AlgorithmParams algorithmParams = new AlgorithmParams();

        private RelayCommand saveToFileCommand;
        private RelayCommand getFromFileCommand;

        private IFileIOLogic FileIOLogic
        {
            get
            {
                return LogicProvider.Instance.FileIOLogic;
            }
        }


        public AlgorithmParams AlgorithmParams
        {
            get
            {
                return algorithmParams;
            }
        }

        public ICommand SaveToFileCommand
        {
            get
            {
                return saveToFileCommand ??
                       (saveToFileCommand = new RelayCommand(param => SaveToFile()));
            }
        }

        public ICommand GetFromFileCommand
        {
            get
            {
                return getFromFileCommand ??
                       (getFromFileCommand = new RelayCommand(param => GetFromFile()));
            }
        }


        private void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog();
            SetGetAlgParamsFileDialogExtensionAndFilter(saveFileDialog);
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FileIOLogic.SaveAlgorithmParameters(AlgorithmParams, saveFileDialog.FileName);
            }
        }

        private void GetFromFile()
        {
            var openFileDialog = new OpenFileDialog();
            SetGetAlgParamsFileDialogExtensionAndFilter(openFileDialog);
            bool? result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            AlgorithmParams readAlgorithmParameters;
            try
            {
                readAlgorithmParameters = FileIOLogic.ReadAlgorithmParameters(openFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd wczytywania danych z pliku!", "Błąd");
                return;
            }

            AlgorithmParams.CopyFrom(readAlgorithmParameters);
        }

        private void SetGetAlgParamsFileDialogExtensionAndFilter(FileDialog fileDialog)
        {
            fileDialog.DefaultExt = FileIOLogic.DefaultParamsFileExtension;

            fileDialog.Filter =
                String.Format("NeuralNetworks2 Algorithm Parameters File (*{0})|*{0}|All files (*.*)|*.*",
                FileIOLogic.DefaultParamsFileExtension);
        }
    }
}
