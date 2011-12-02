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
        /// Zapisuje ludzi do pliku.
        /// </summary>
        /// <param name="people">Osoby, których rozpoznawania będziemy uczyć sieć.</param>
        void SavePeople(IList<Person> people);

        /// <summary>
        /// Zapisuje parametry algorytmu do pliku.
        /// </summary>
        /// <param name="algorithmParams">Parametry algorytmu.</param>
        void SaveAlgorithmParameters(AlgorithmParams algorithmParams);

        /// <summary>
        /// Wczytuje ludzi z pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku z zapisanymi ludźmi.</param>
        IList<Person> ReadPeople(string filePath);

        /// <summary>
        /// Wczytuje parametry algorytmu z pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku z zapisanymi parametrami.</param>
        AlgorithmParams ReadAlgorithmParameters(string filePath);
    }
}