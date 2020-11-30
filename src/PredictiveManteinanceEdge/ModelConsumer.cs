using System;
using System.IO;
using Microsoft.ML;

namespace PredictiveManteinanceEdge
{
    public class ModelConsumer
    {
        static readonly string MODEL_PATH = Path.Combine(Environment.CurrentDirectory, "Model", "model.zip");
        private static Lazy<PredictionEngine<MachineData, MachinePrediction>> PredictionEngine = new Lazy<PredictionEngine<MachineData, MachinePrediction>>(CreatePredictionEngine);

        public static MachinePrediction Predict(MachineData input)
        {
            MachinePrediction result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<MachineData, MachinePrediction> CreatePredictionEngine()
        {
            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MODEL_PATH, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<MachineData, MachinePrediction>(mlModel);

            return predEngine;
        }
    }
}
