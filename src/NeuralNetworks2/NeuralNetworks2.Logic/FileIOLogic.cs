using System;
using System.Collections.Generic;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.Logic
{
    public class FileIOLogic : IFileIOLogic
    {
        public void SavePeople(IList<Person> people)
        {
            throw new NotImplementedException();
        }

        public void SaveAlgorithmParameters(AlgorithmParams algorithmParams)
        {
            throw new NotImplementedException();
        }

        public IList<Person> ReadPeople(string filePath)
        {
            throw new NotImplementedException();
        }

        public AlgorithmParams ReadAlgorithmParameters(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}