using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace NeuralNetworks1 {
    /// <summary>
    /// Klasa odpowiada za wczytanie z pliku danych do 
    /// nauki sieci neuronowej.
    /// </summary>
    class InputDataSet {
        private List<double[]> _inputDataSet;
        private List<double[]> _outputDataSet;

        private enum TokenType {
            OpenPar,
            ClosePar,
            Separator,
            Number,
            Invalid
        }

        private class Token {
            public double Value { get; private set; }
            public TokenType Type { get; private set; }

            /// <summary>
            /// Numer linii w ktorej wystapil dany token
            /// </summary>
            public int Line { get; private set; }
            /// <summary>
            /// Number kolumny w ktorej wystapil dany token
            /// </summary>
            public int Column { get; private set; }

            public Token(TokenType type, int line, int column)
                : this(0.0, type, line, column) {
                    Debug.Assert(type != TokenType.Number);
            }

            public Token(double value, int line, int column) 
                : this(value, TokenType.Number, line, column) { }

            private Token(double value, TokenType type, int line = 0, int column = 0) {
                Value = value;
                Type = type;

                Line = line;
                Column = column;
            }

            public override string ToString() {
                return String.Format("{0} ({1}, {2})", (Type != TokenType.Number) ? (object) Type : (object) Value, Line, Column);
            }
        }

        /// <summary>
        /// Dane wejsciowe sieci
        /// </summary>
        public double[][] InputSet {
            get { return _inputDataSet.ToArray(); }
        }

        /// <summary>
        /// Rozmiar danych wejsciowych sieci (drugi indeks tablicy)
        /// </summary>
        public int InputDataSize { get; private set; }

        /// <summary>
        /// Ilosc wektorow wejsciowych (pierwszy indeks tablicy)
        /// </summary>
        public int InputDataCount {
            get { return _inputDataSet.Count; }
        }

        /// <summary>
        /// Dane wyjsciowe sieci
        /// </summary>
        public double[][] OutputSet {
            get { return _outputDataSet.ToArray(); }
        }

        /// <summary>
        /// Rozmiar danych wyjsciowych sieci (drugi indeks tablicy)
        /// </summary>
        public int OutputDataSize { get; private set; }

        /// <summary>
        /// Ilosc danych wyjsciowych sieci (pierwszy indeks tablicy)
        /// </summary>
        public int OutputDataCount {
            get { return _outputDataSet.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Nazwa pliku z danymi</param>
        /// <param name="inputDataSize">Rozmiar danych wejsciowych</param>
        /// <param name="outputDataSize">Rozmiar danych wyjsciowych (0 - dla sieci Kohonena)</param>
        public InputDataSet(string fileName, int inputDataSize, int outputDataSize = 0) {
            if (inputDataSize < 1 || outputDataSize < 0)
                throw new InputDataSetException("Invalid constructor parameters");
            
            _inputDataSet = new List<double[]>();
            _outputDataSet = new List<double[]>();

            InputDataSize = inputDataSize;
            OutputDataSize = outputDataSize;

            string fileData = ReadFileData(fileName);
            List<Token> tokens = GenerateTokens(fileData);
            ParseTokens(tokens);
        }

        private void ParseTokens(List<Token> tokens) {
            int tokenIndex = 0;
            
            Func<Token> getCurrentToken = () => {
                if (tokenIndex >= tokens.Count)
                    return new Token(TokenType.Invalid, -1, -1);
                return tokens[tokenIndex];
            };

            Action<TokenType> matchToken = (TokenType type) => {
                if (getCurrentToken().Type == type)
                    tokenIndex++;
                else
                    throw new InputDataSetException(
                        String.Format("Invalid data structure, expecting: {0} but {1}", type, getCurrentToken())
                        );
            };

            Func<int, double[]> readVector = (int vectorSize) => {
                List<double> vectorItems = new List<double>();

                if (vectorSize > 0) {
                    matchToken(TokenType.OpenPar);
                    for (int i = 0; i < vectorSize; i++) {
                        vectorItems.Add(getCurrentToken().Value);
                        matchToken(TokenType.Number);

                        if (i < vectorSize - 1)
                            matchToken(TokenType.Separator);
                    }
                    matchToken(TokenType.ClosePar);
                }

                return vectorItems.ToArray();
            };

            while(getCurrentToken().Type != TokenType.Invalid) {
                _inputDataSet.Add(readVector(InputDataSize));
                _outputDataSet.Add(readVector(OutputDataSize));
            }
        }

        private string ReadFileData(string fileName) {
            try {
                using (TextReader reader = new StreamReader(fileName)) {
                    return reader.ReadToEnd();
                }
            }
            catch (IOException error) {
                throw new InputDataSetException(
                    "File IO Error: " + error.Message
                    );
            }
        }

        private List<Token> GenerateTokens(string input) {
            List<Token> inputTokens = new List<Token>();

            bool inNumber = false; 
            StringBuilder tmp = new StringBuilder();

            int line = 1;
            int column = 1;

            Func<char, bool> generateToken = (char c) => {
                if(!inNumber) {
                    if (Char.IsWhiteSpace(c))
                        return true;

                    switch (c) {
                        case '[':
                            inputTokens.Add(new Token(TokenType.OpenPar, line, column));
                            return true;
                        case ']':
                            inputTokens.Add(new Token(TokenType.ClosePar, line, column));
                            return true;
                        case ';':
                            inputTokens.Add(new Token(TokenType.Separator, line, column));
                            return true;
                    }

                    if (Char.IsNumber(c) || "+-.".Contains(c)) {
                        inNumber = true;
                        tmp.Clear();
                    }
                    else 
                        throw new InputDataSetException(
                            String.Format("Invalid input character '{0}' at ({1}, {2})", c, line, column)
                            );
                }

                if (inNumber) {
                    if (Char.IsNumber(c) || "+-.e".Contains(c)) {
                        tmp.Append(c);
                    }
                    else {
                        inNumber = false;
                        double value;
                        if (!Double.TryParse(tmp.ToString(), out value))
                            throw new InputDataSetException(
                                String.Format("Invalid number format '{0}' at ({1}, {2})", tmp.ToString(), line, column)
                                );
                        inputTokens.Add(new Token(value, line, column - tmp.Length));
                        return false;
                    }
                }

                return true;
            };

            foreach (char c in input) {
                while (!generateToken(c))
                    ;

                if (c == '\n') {
                    line++;
                    column = 0;
                }
                column++;
            }
            generateToken('\n');

            return inputTokens;
        }

    }
}
