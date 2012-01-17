using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace NeuralNetworks2.API.Model
{
    [Serializable]
    public class Person : BaseModel, ISerializable
    {
        private const string FirstNameSerName = "FirstName";
        private const string SurNameSerName = "SurName";
        private const string ImageSerName = "Image";
        private const string TrainWavesPathsSerName = "TrainWavesPaths";
        private const string TestWavesPathsSerName = "TestWavesPaths";

        private string firstName;
        private string surName;
        private string image;
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
                OnPropertyChanged<Person>(x => x.FullName);
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
                OnPropertyChanged<Person>(x => x.FullName);
            }
        }

        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", FirstName, SurName);
            }
        }

        /// <summary>
        /// Obraz.
        /// </summary>
        public string Image
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


        public Person()
        {
        }

        public Person(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                return;
            }

            firstName = info.GetString(FirstNameSerName);
            surName = info.GetString(SurNameSerName);
            image = info.GetString(ImageSerName);
            trainWavesPaths = new ObservableCollection<string>(
                (List<string>) info.GetValue(TrainWavesPathsSerName, typeof (List<string>)));
            testWavesPaths = new ObservableCollection<string>(
                (List<string>)info.GetValue(TestWavesPathsSerName, typeof(List<string>)));
        }


        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
            {
                return;
            }

            info.AddValue(FirstNameSerName, firstName);
            info.AddValue(SurNameSerName, surName);
            info.AddValue(ImageSerName, image);
            info.AddValue(TrainWavesPathsSerName, trainWavesPaths.ToList());
            info.AddValue(TestWavesPathsSerName, testWavesPaths.ToList());
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}