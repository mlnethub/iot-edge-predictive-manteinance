namespace PredictiveManteinanceEdge
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;

    class Program
    {
        static int counter;
        static JsonSerializerSettings serializerSettings;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);

            // set direct method
            await ioTHubModuleClient.SetMethodHandlerAsync(
                                            "processTemperature",
                                            ProcessTemperatureCallback,
                                            ioTHubModuleClient);

            // create the prediction engine
            ModelConsumer.CreatePredictionEngine();

            serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }
            
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);            

            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            try
            {
                if (!string.IsNullOrEmpty(messageString))
                {
                    // deserialize the request message
                    Temperatures sensorData = JsonConvert.DeserializeObject<Temperatures>(messageString, serializerSettings);

                    // create the model input
                    MachineData input = new MachineData()
                    {
                        Machine_pressure = sensorData.Machine.Pressure,
                        Machine_temperature = sensorData.Machine.Temperature,
                        Ambient_humidity = sensorData.Ambient.Humidity,
                        Ambient_temperature = sensorData.Ambient.Temperature
                    };

                    // run the model
                    var modelOutput = ModelConsumer.Predict(input);

                    sensorData.Anomally = modelOutput.Anomally;

                    messageString = JsonConvert.SerializeObject(sensorData, serializerSettings);
                    messageBytes = Encoding.UTF8.GetBytes(messageString);

                    using var pipeMessage = new Message(messageBytes);

                    foreach (var prop in message.Properties)
                    {
                        pipeMessage.Properties.Add(prop.Key, prop.Value);
                    }

                    Console.WriteLine($"Sending processed message: {counterValue}, Body: [{messageString}]");

                    await moduleClient.SendEventAsync("output1", pipeMessage);

                }                
            }
            catch (Exception exc)
            {
                Console.WriteLine($"ERROR: {exc}");
            }

            return MessageResponse.Completed;
        }

        private static async Task<MethodResponse> ProcessTemperatureCallback(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"ProcessTemperature started at {DateTime.UtcNow}");

            var moduleClient = userContext as ModuleClient;

            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            try
            {
                var sensorData = JsonConvert.DeserializeObject<Temperatures>(methodRequest.DataAsJson);

                // create the model input
                MachineData input = new MachineData()
                {
                    Machine_pressure = sensorData.Machine.Pressure,
                    Machine_temperature = sensorData.Machine.Temperature,
                    Ambient_humidity = sensorData.Ambient.Humidity,
                    Ambient_temperature = sensorData.Ambient.Temperature
                };

                // run the model
                var modelOutput = ModelConsumer.Predict(input);

                sensorData.Anomally = modelOutput.Anomally;

                var messageString = JsonConvert.SerializeObject(sensorData, serializerSettings);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);

                using var pipeMessage = new Message(messageBytes);
                
                Console.WriteLine($"Sending processed message Body: [{messageString}]");

                await moduleClient.SendEventAsync("output1", pipeMessage);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception {ex.Message}");
            }

            var response = new MethodResponse(200);

            return response;
        }
    }
}
