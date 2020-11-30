using System;
using Microsoft.ML.Data;

namespace PredictiveManteinance.Model
{
    public class MachinePrediction
    {
        public float Score { get; set; }

        [ColumnName("PredictedLabel")]
        public bool Anomally { get; set; }

        public float Probability { get; set; }
    }
}
