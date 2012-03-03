using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace ProcView
{
    [DataContract(Name = "Kernel", Namespace = "http://www.facebook.com/joshua.niehus")]

    public class Kernel
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Kernel()
        {
        }

        /// <summary>
        /// Preset constructor
        /// </summary>
        /// <param name="preset">Enter name of implemented filter (Example: "Lowpass" or "Weighted Lowpass").</param>
        public Kernel(string preset)
        {
            int[,] matrix;
            switch (preset)
            {
                case "Lowpass":  //basic smoothing kernel, makes the image blurry
                    {
                        matrix = new int[7, 7] { 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }, 
                        { 1, 1, 1, 1, 1, 1, 1 }
                        };
                        _name = "Lowpass";
                        break;
                    }
                case "Weighted Lowpass":  //smoothing kernel where surrounding pixels dont count as much the farther away they are from the target pixel
                    {
                        matrix = new int[3, 3] { 
                        { 1, 2, 1 }, 
                        { 2, 4, 2 }, 
                        { 1, 2, 1 } 
                        };
                        _name = "Weighted Lowpass";
                        break;
                    }
                case "High Pass":  //edge detection kernel
                    {
                        matrix = new int[3, 3] { 
                        { -1, -1, -1 }, 
                        { -1, 8, -1 }, 
                        { -1, -1, -1 } 
                        };
                        _name = "High Pass";
                        break;
                    }
                case "Negative":  //applies negative value to target pixel ( where negative value = 255 - targetPixel'sComponent)
                    {
                        matrix = new int[3, 3] { 
                        { 0, 0, 0 }, 
                        { 0, -1, 0 }, 
                        { 0, 0, 0 } 
                        };
                        _name = "Negative";
                        break;
                    }
                case "Normalized Sobel X":  //edge detection kernel
                    {
                        matrix = new int[3, 3] { 
                        { 1, 2, 1 }, 
                        { 0, 0, 0 }, 
                        { -1, -2, -1 } 
                        };
                        _name = "NormalizedSobelX";
                        break;
                    }
                case "Normalized Sobel Y":  //edge detection kernel
                    {
                        matrix = new int[3, 3] { 
                        { 1, 0, -1 }, 
                        { 2, 0, -2 }, 
                        { 1, 0, -1 } 
                        };
                        _name = "NormalizedSobelY";  
                        break;
                    }
                default:
                    {
                        matrix = new int[3, 3] { 
                        { 0, 0, 0 }, 
                        { 0, 0, 0 }, 
                        { 0, 0, 0 } 
                        };
                        _name = "Default";
                        break;
                    }
            }

            _matrix = matrix;
            _serialMatrix = SerializeMatrix(matrix);
            _matrixSum = SumMatrix(matrix);
 
        }

        /// <summary>
        /// Custom constructor
        /// </summary>
        /// <param name="custom_matrix">Requires MxM array where M is an odd integer >= 3</param>
        public Kernel(int[,] custom_matrix)
        {
            _matrix = custom_matrix;
            _serialMatrix = SerializeMatrix(custom_matrix);
            _name = "Custom";
            _matrixSum = SumMatrix(custom_matrix);
        }

        [DataMember(Name = "KernelMatrix")]
        private int[] _serialMatrix;
        public int[] SerialMatrix
        {
            get { return _serialMatrix; }
            set 
            { 
                _serialMatrix = value;
                _matrix = DeserializeMatrix(value);
            }
        }

        private int[,] _matrix;
        public int[,] matrix 
        {
            get { return _matrix; }
            set 
            { 
                _matrix = value;
                _serialMatrix = SerializeMatrix(value);
            }
        }

        private int _matrixSize;
        [DataMember(Name = "MatrixSize")]
        public int matrixSize 
        { 
            get { return _matrix.GetLength(1);}
            set { _matrixSize = value; }
        }

        private int _matrixSum = 0;
        [DataMember(Name = "MatrixSum")]
        public int matrixSum
        {
            get { return _matrixSum; }
            set { _matrixSum = value; }
        }

        private string _name;
        [DataMember(Name = "KernelName")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int SumMatrix(int[,] matrix)
        {

            int sum = 0;
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    sum += Math.Abs(matrix[x, y]);
                }
            }
            return sum;
        }

        public int[] SerializeMatrix(int[,] matrix)
        {

            int area = matrix.GetLength(1) * matrix.GetLength(1);
            int size = matrix.GetLength(1);
            int i = 0;
            int[] serialMatrix = new int[area];
             for (int y = 0; y < matrix.GetLength(1); y++)
             {
                i = y * size;
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    serialMatrix[i] = matrix[x, y];
                    i += 1;
                }
             }

             return serialMatrix;
        }

        public int[,] DeserializeMatrix(int[] serialMatrix)
        {
            int size = serialMatrix.GetLength(0);
            double dWidth = Math.Sqrt(size);
            int width = (int)dWidth;
            int i = 0;
            int[,] matrix = new int[width,width];
            for (int y = 0; y < width; y++)
            {
                i = y * width;
                for (int x = 0; x < width; x++)
                {
                    matrix[x,y] = serialMatrix[i];
                    i += 1;
                }
            }

            return matrix;
        }


    }
}
