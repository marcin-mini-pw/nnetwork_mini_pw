using NeuralNetworks2.UI.ViewModels;

namespace NeuralNetworks2.UI
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


        private void AlgorithmInitWindowViewModel_RequestClose(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
