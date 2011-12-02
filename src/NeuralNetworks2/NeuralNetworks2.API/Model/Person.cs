using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace NeuralNetworks2.API.Model
{
    [Serializable]
    public class Person : BaseModel
    {
        private string firstName;
        private string surName;
        private BitmapImage image;
        private readonly ObservableCollection<string> trainWavesPaths = new ObservableCollection<string>();
        private readonly ObservableCollection<string> testWavesPaths = new ObservableCollection<string>();

        /// <summary>
        /// Imię.
        /// </summary>
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                if (value == firstName)
                {
                    return;
                }

                firstName = value;
                OnPropertyChanged<Person>(x => x.FirstName);
            }
        }

        /// <summary>
        /// Nazwisko.
        /// </summary>
        public string SurName
        {
            get
            {
                return surName;
            }
            set
            {
                if (value == surName)
                {
                    return;
                }

                surName = value;
                OnPropertyChanged<Person>(x => x.SurName);
            }
        }

        /// <summary>
        /// Obraz.
        /// </summary>
        public BitmapImage Image
        {
            get
            {
                return image;
            }
            set
            {
                if (value == image)
                {
                    return;
                }

                image = value;
                OnPropertyChanged<Person>(x => x.Image);
            }
        }

        /// <summary>
        /// Ścieżki do plików *.wav z nagraniami głosu danej osoby, służącymi do uczenia sieci.
        /// </summary>
        public ObservableCollection<string> TrainWavesPaths
        {
            get
            {
                return trainWavesPaths;
            }
        }

        /// <summary>
        /// Ścieżki do plików *.wav z nagraniami głosu danej osoby, służącymi do testowania sieci.
        /// </summary>
        public ObservableCollection<string> TestWavesPaths
        {
            get
            {
                return testWavesPaths;
            }
        }
    }
}