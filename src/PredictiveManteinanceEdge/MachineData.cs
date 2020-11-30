using System;
using Microsoft.ML.Data;

namespace PredictiveManteinanceEdge
{
    public class MachineData
    {
        [ColumnName("machine_temperature"), LoadColumn(0)]
        public float Machine_temperature { get; set; }


        [ColumnName("machine_pressure"), LoadColumn(1)]
        public float Machine_pressure { get; set; }


        [ColumnName("ambient_temperature"), LoadColumn(2)]
        public float Ambient_temperature { get; set; }


        [ColumnName("ambient_humidity"), LoadColumn(3)]
        public float Ambient_humidity { get; set; }

        [ColumnName("anomally"), LoadColumn(4)]
        public bool Anomally { get; set; }
    }
}
