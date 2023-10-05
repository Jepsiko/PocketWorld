using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ChunkData {
    public Vector3Int chunkCoord;
    public float[,] height;
    public float[,] slope;
    public float[,] temperature;
    public float[,] humidity;
    public Color[,] color;

    private Settings settings;

    public ChunkData(Settings settings) {
        this.settings = settings;
        height = new float[settings.chunkSize, settings.chunkSize];
        slope = new float[settings.chunkSize, settings.chunkSize];
        temperature = new float[settings.chunkSize, settings.chunkSize];
        humidity = new float[settings.chunkSize, settings.chunkSize];
        color = new Color[settings.chunkSize, settings.chunkSize];
    }

    public ChunkData(Settings settings, string chunkDataString) {
        this.settings = settings;
        string[] lines = chunkDataString.Split('\n');
        
        height = StringToFloatMatrix(lines, 0);
        slope = StringToFloatMatrix(lines, 1);
        temperature = StringToFloatMatrix(lines, 2);
        humidity = StringToFloatMatrix(lines, 3);
        color = StringToColorMatrix(lines, 4);
    }

    private float[,] StringToFloatMatrix(string[] lines, int offset) {
        offset *= settings.chunkSize;
        float[,] matrix = new float[settings.chunkSize, settings.chunkSize];
        for (int i = 0; i < settings.chunkSize; i++) {
            string[] line = lines[offset+i].Split('\t');
            for (int j = 0; j < settings.chunkSize; j++) {
                matrix[i, j] = float.Parse(line[j]);
            }
        }
        return matrix;
    }

    private Color[,] StringToColorMatrix(string[] lines, int offset) {
        offset *= settings.chunkSize;
        Color[,] matrix = new Color[settings.chunkSize, settings.chunkSize];
        for (int i = 0; i < settings.chunkSize; i++) {
            string[] line = lines[offset+i].Split('\t');
            for (int j = 0; j < settings.chunkSize; j++) {
                string[] rgba = line[j].Split(',');
                float red = float.Parse(rgba[0]);
                float green = float.Parse(rgba[1]);
                float blue = float.Parse(rgba[2]);
                matrix[i, j] = new Color(red, green, blue);
            }
        }
        return matrix;
    }

    public string GetString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(FloatMatrixToString(height));
        sb.Append(FloatMatrixToString(slope));
        sb.Append(FloatMatrixToString(temperature));
        sb.Append(FloatMatrixToString(humidity));
        sb.Append(ColorMatrixToString(color));
        return sb.ToString();
    }

    private string FloatMatrixToString(float[,] matrix) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                sb.Append(matrix[i, j]);
                sb.Append("\t");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string ColorMatrixToString(Color[,] matrix) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                sb.Append(matrix[i, j].r.ToString());
                sb.Append(",");
                sb.Append(matrix[i, j].g.ToString());
                sb.Append(",");
                sb.Append(matrix[i, j].b.ToString());
                sb.Append("\t");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
