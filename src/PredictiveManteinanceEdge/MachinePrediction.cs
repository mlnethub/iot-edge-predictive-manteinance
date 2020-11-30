using System;
using Microsoft.ML.Data;

namespace PredictiveManteinanceEdge
{
    public class MachinePrediction
    {
        public float Score { get; set; }

        [ColumnName("PredictedLabel")]
        public bool Anomally { get; set; }

        public float Probability { get; set; }
    }
}
