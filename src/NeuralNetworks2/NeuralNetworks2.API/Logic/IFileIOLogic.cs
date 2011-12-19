using System.Collections.Generic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.API.Logic
{
    /// <summary>
    /// Zarządza operacjami zapisu do / odczytu z plików.
    /// </summary>
    public interface IFileIOLogic
    {
        /// <summary>
        /// Domyślne rozszerzenie plików z zapisanymi ludźmi.
        /// </summary>
        string DefaultPeopleFileExtension { get; }

        /// <summary>
        /// Domyślne rozszerzenie plików z zapisanymi parametrami algorytmu.
        /// </summary>
        string DefaultParamsFileExtension { get; }

        /// <summary>
        /// Domyśle rozszerzenie plików z zapisaną logiką.
        /// </summary>
        string DefaultAlgorithmsLogicFileExtension { get; }


        /// <summary>
        /// Zapisuje ludzi do pliku.
        /// </summary>
        /// <param name="people">Osoby, których rozpoznawania będziemy uczyć sieć.</param>
        /// <param name="filePath">Ścieżka do pliku, w którym chcemy zapisać ludzi.</param>
        void SavePeople(IList<Person> people, string filePath);

        /// <summary>
        /// Zapisuje parametry algorytmu do pliku.
        /// </summary>
        /// <param name="algorithmParams">Parametry algorytmu.</param>
        /// <param name="filePath">Ścieżka do pliku, w którym chcemy zapisać parametry algorytmu.</param>
        void SaveAlgorithmParameters(AlgorithmParams algorithmParams, string filePath);

        /// <summary>
        /// Zapisuje logikę do pliku.
        /// </summary>
        /// <param name="algorithmsLogic"></param>
        /// <param name="filePath">Ścieżka do pliku, w którym chcemy zapisać logikę.</param>
        void SaveAlgorithmsLogic(IAlgorithmsLogic algorithmsLogic, string filePath);

        /// <summary>
        /// Wczytuje ludzi z pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku z zapisanymi ludźmi.</param>
        /// <returns></returns>
        IList<Person> ReadPeople(string filePath);

        /// <summary>
        /// Wczytuje parametry algorytmu z pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku z zapisanymi parametrami.</param>
        /// <returns></returns>
        AlgorithmParams ReadAlgorithmParameters(string filePath);

        /// <summary>
        /// Wczytuje logikę z pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku z zapisanymi parametrami.</param>
        /// <returns></returns>
        IAlgorithmsLogic ReadAlgorithmsLogic(string filePath);
    }
}