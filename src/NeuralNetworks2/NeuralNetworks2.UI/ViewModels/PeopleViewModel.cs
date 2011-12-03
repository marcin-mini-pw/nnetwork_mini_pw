using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// ViewModel dla widoku z edycją listy osób, których rozpoznawania nauczymy aplikację.
    /// </summary>
    public class PeopleViewModel: BaseViewModel
    {
        private readonly ObservableCollection<Person> people;

        private RelayCommand addNewPersonCommand;
        private RelayCommand deletePersonCommand;
        private RelayCommand saveToFileCommand;
        private RelayCommand getFromFileCommand;
        private RelayCommand changeImageCommand;
        private RelayCommand addNewWaveFileCommand;
        private RelayCommand removeWaveFileCommand;

        private IFileIOLogic FileIOLogic
        {
            get
            {
                return LogicProvider.Instance.FileIOLogic;
            }
        }

        public ICollectionView People { get; set; }

        public IList<Person> PeopleList
        {
            get
            {
                return people;
            }
        }

        public ICommand AddNewPersonCommand
        {
            get
            {
                return addNewPersonCommand ??
                       (addNewPersonCommand = new RelayCommand(param => AddNewPerson()));
            }
        }

        public ICommand DeletePersonCommand
        {
            get
            {
                return deletePersonCommand ??
                       (deletePersonCommand = new RelayCommand(param => DeletePerson(), param => CanDeletePerson()));
            }
        }

        public ICommand SaveToFileCommand
        {
            get
            {
                return saveToFileCommand ??
                       (saveToFileCommand = new RelayCommand(param => SaveToFile(), param => CanSaveToFile()));
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
        
        public ICommand ChangeImageCommand
        {
            get
            {
                return changeImageCommand ??
                       (changeImageCommand = new RelayCommand(param => ChangeImage(), param => CanChangeImage()));
            }
        }

        public ICommand AddNewWaveFileCommand
        {
            get
            {
                return addNewWaveFileCommand ??
                       (addNewWaveFileCommand = new RelayCommand(
                           param => AddNewWaveFile((bool)param), param => CanAddNewWaveFile((bool)param)));
            }
        }

        public ICommand RemoveWaveFileCommand
        {
            get
            {
                return removeWaveFileCommand ??
                       (removeWaveFileCommand = new RelayCommand(
                           param => RemoveWaveFile((bool)param), param => CanRemoveWaveFile((bool)param)));
            }
        }


        public PeopleViewModel()
        {
            people = new ObservableCollection<Person>();


            //TODO: usunąć
            //***************************
            //people.Add(new Person {FirstName = "Albert", SurName = "Skłodowski"});
            //people.Add(new Person {FirstName = "Marcin", SurName = "Chwedczuk"});
            //***************************

            People = CollectionViewSource.GetDefaultView(people);
        }


        private void AddNewPerson()
        {
            people.Add(new Person {FirstName = "Neural", SurName = "Network"});
        }

        private void DeletePerson()
        {
            int currentIndex = People.CurrentPosition;
            people.RemoveAt(currentIndex);
        }

        private bool CanDeletePerson()
        {
            return People.CurrentItem != null;
        }

        private void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog();
            SetGetPeopleFileDialogExtensionAndFilter(saveFileDialog);
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FileIOLogic.SavePeople(people, saveFileDialog.FileName);
            }
        }

        private bool CanSaveToFile()
        {
            return people.Any();
        }

        private void GetFromFile()
        {
            var openFileDialog = new OpenFileDialog();
            SetGetPeopleFileDialogExtensionAndFilter(openFileDialog);
            bool? result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            IList<Person> readPeople;
            try
            {
                readPeople = FileIOLogic.ReadPeople(openFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd wczytywania danych z pliku!", "Błąd");
                return;
            }

            people.Clear();
            foreach (var person in readPeople)
            {
                people.Add(person);
            }
        }

        private void ChangeImage()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.gif;*.bmp;*.png;*.jpeg|All Files|*.*";
            bool? result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            var person = People.CurrentItem as Person;
            person.Image = openFileDialog.FileName;
        }

        private bool CanChangeImage()
        {
            return People.CurrentItem != null;
        }

        private void AddNewWaveFile(bool train)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter =
                String.Format("Wave files (*wav)|*wav|All files (*.*)|*.*");
            bool? result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            var person = People.CurrentItem as Person;
            var waveFiles = train ? person.TrainWavesPaths : person.TestWavesPaths;
            waveFiles.Add(openFileDialog.FileName);
        }

        private bool CanAddNewWaveFile(bool train)
        {
            return People.CurrentItem != null; //TODO: może ustawić jakieś maksimum?
        }

        private void RemoveWaveFile(bool train)
        {
            var person = People.CurrentItem as Person;
            var waveFiles = train ? person.TrainWavesPaths : person.TestWavesPaths;
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(waveFiles);
            waveFiles.RemoveAt(collectionView.CurrentPosition);
        }

        private bool CanRemoveWaveFile(bool train)
        {
            var person = People.CurrentItem as Person;
            if(person == null)
            {
                return false;
            }
            var waveFiles = train ? person.TrainWavesPaths : person.TestWavesPaths;
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(waveFiles);
            return collectionView.CurrentItem != null;
        }

        private void SetGetPeopleFileDialogExtensionAndFilter(FileDialog fileDialog)
        {
            fileDialog.DefaultExt = FileIOLogic.DefaultPeopleFileExtension;

            fileDialog.Filter =
                String.Format("NeuralNetworks2 People File (*{0})|*{0}|All files (*.*)|*.*",
                FileIOLogic.DefaultPeopleFileExtension);
        }
    }
}
