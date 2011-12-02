using System.Collections.Generic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.API.Logic
{
    public interface IAlgorithmsLogic
    {
        /// <summary>
        /// Inicjalizuje logikę (w praktyce: buduje sieci neuronowe).
        /// </summary>
        /// <param name="peopleToBeRecognized">Osoby, których rozpoznawania będziemy uczyć sieć.</param>
        /// <param name="algParams">Parametry algorytmu.</param>
        void Init(IList<Person> peopleToBeRecognized, AlgorithmParams algParams);

        /// <summary>
        /// Resetule logikę (usuwa zbudowane sieci neuronowe itp.). Potem jest potrzebna nowa inicjalizacja.
        /// </summary>
        void Reset();

        /// <summary>
        /// Uczy wszystkie zbudowane sieci rozpoznawania przypisanych do nich osób.
        /// </summary>
        /// <returns>Słownik [Osoba, błąd sieci odpowiadającej tej osobie na zbiorze testowym].</returns>
        IDictionary<Person, double> Train();

        /// <summary>
        /// Rozpoczyna nagrywanie dźwięku z mikrofonu.
        /// </summary>
        void StartRecordingVoice();

        /// <summary>
        /// Kończy nagrywanie dźwięku z mikrofonu.
        /// </summary>
        /// <returns>Rezultaty zwrócone przez sieci odpowiadające poszczególnym osobom.</returns>
        IDictionary<Person, double> StopRecordingAndGetResults();
    }
}