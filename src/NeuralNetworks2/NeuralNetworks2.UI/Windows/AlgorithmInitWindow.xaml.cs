using System;
using NeuralNetworks2.UI.ViewModels;

namespace NeuralNetworks2.UI.Windows
{
    public partial class AlgorithmInitWindow
    {
        public AlgorithmInitWindowViewModel AlgorithmInitWindowViewModel
        {
            get
            {
                return AlgorithmInitWindowViewModel.Instance;
            }
        }


        public AlgorithmInitWindow()
        {
            InitializeComponent();

            DataContext = AlgorithmInitWindowViewModel;
            AlgorithmInitWindowViewModel.RequestClose += AlgorithmInitWindowViewModel_RequestClose;
        }


        private void AlgorithmInitWindowViewModel_RequestClose(object sender, EventArgs e)
        {
            DialogResult = true;
            AlgorithmInitWindowViewModel.RequestClose -= AlgorithmInitWindowViewModel_RequestClose;
            Close();
        }
    }
}
