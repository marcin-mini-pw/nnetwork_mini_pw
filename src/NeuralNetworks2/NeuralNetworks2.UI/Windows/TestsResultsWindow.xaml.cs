using System;
using NeuralNetworks2.UI.ViewModels;

namespace NeuralNetworks2.UI.Windows
{
    public partial class TestsResultsWindow
    {
        public TestsResultsWindowViewModel TestsResultsWindowViewModel
        {
            get
            {
                return TestsResultsWindowViewModel.Instance;
            }
        }


        public TestsResultsWindow()
        {
            InitializeComponent();

            DataContext = TestsResultsWindowViewModel;
            TestsResultsWindowViewModel.RequestClose += TestsResultsWindowViewModel_RequestClose;
        }


        private void TestsResultsWindowViewModel_RequestClose(object sender, EventArgs e)
        {
            TestsResultsWindowViewModel.RequestClose -= TestsResultsWindowViewModel_RequestClose;
            Close();
        }
    }
}
