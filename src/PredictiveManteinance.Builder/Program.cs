using System;
using PredictiveManteinance.Model;

namespace PredictiveManteinance.Builder
{
    class Program
    {
        static void Main(string[] args)
        {
            ModelBuilder.CreateModel();

            // Create single instance of sample data from first line of dataset for model input
            MachineData sampleData = new MachineData()
            {
                Machine_temperature = Convert.ToSingle(101.937256),
                Machine_pressure = Convert.ToSingle(10.2207),
                Ambient_temperature = Convert.ToSingle(21.497833),
                Ambient_humidity = Convert.ToSingle(26),
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ModelConsumer.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Anomaly with predicted Anomaly from sample data...\n\n");
            Console.WriteLine($"Machine_temperature: {sampleData.Machine_temperature}");
            Console.WriteLine($"Machine_pressure: {sampleData.Machine_pressure}");
            Console.WriteLine($"Ambient_temperature: {sampleData.Ambient_temperature}");
            Console.WriteLine($"Ambient_humidity: {sampleData.Ambient_humidity}");
            Console.WriteLine($"\n\nPredicted Anomaly: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.Read();        
        }
    }
}
