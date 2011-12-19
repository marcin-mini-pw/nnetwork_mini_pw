using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.Logic
{
    public class FileIOLogic : IFileIOLogic
    {
        public string DefaultPeopleFileExtension
        {
            get
            {
                return ".nnp";
            }
        }

        public string DefaultParamsFileExtension
        {
            get
            {
                return ".nna";
            }
        }

        public string DefaultAlgorithmsLogicFileExtension
        {
            get
            {
                return ".nnl";
            }
        }


        public void SavePeople(IList<Person> people, string filePath)
        {
            SaveObject(people, filePath);
        }

        public void SaveAlgorithmParameters(AlgorithmParams algorithmParams, string filePath)
        {
            SaveObject(algorithmParams, filePath);
        }

        public IList<Person> ReadPeople(string filePath)
        {
            return ReadObject<IList<Person>>(filePath);
        }

        public AlgorithmParams ReadAlgorithmParameters(string filePath)
        {
            return ReadObject<AlgorithmParams>(filePath);
        }

        public void SaveAlgorithmsLogic(IAlgorithmsLogic algorithmsLogic, string filePath)
        {
            if (!(algorithmsLogic is AlgorithmsLogic))
            {
                throw new ArgumentException("algorithmsLogic");
            }

            SaveObject(algorithmsLogic, filePath);
        }

        public IAlgorithmsLogic ReadAlgorithmsLogic(string filePath)
        {
            return ReadObject<AlgorithmsLogic>(filePath);
        }


        private static void SaveObject(object obj, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, obj);
            }
        }

        private static T ReadObject<T>(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                return (T)bf.Deserialize(fs);
            }
        }
    }
}