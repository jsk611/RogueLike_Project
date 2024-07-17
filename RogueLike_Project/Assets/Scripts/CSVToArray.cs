using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class CSVToArray : MonoBehaviour
{
    public int[,] CSVFileToArray(string path)
    {
        string[] lines = File.ReadAllLines(path);
        int rows = lines.Length;
        int cols = lines[0].Split(',').Length;

        int[,] dataArray = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < cols; j++)
            {
                dataArray[i, j] = int.Parse(values[j]);
            }
        }

        return dataArray;
    }

}
