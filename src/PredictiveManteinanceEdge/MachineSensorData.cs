using System;
using Newtonsoft.Json;

namespace PredictiveManteinanceEdge
{
    public partial class Temperatures
    {
        [JsonProperty("machine")]
        public Machine Machine { get; set; }

        [JsonProperty("ambient")]
        public Ambient Ambient { get; set; }

        [JsonProperty("timeCreated")]
        public DateTimeOffset TimeCreated { get; set; }

        [JsonProperty("anomally")]
        public bool? Anomally { get; set; }
    }

    public partial class Ambient
    {
        [JsonProperty("temperature")]
        public float Temperature { get; set; }

        [JsonProperty("humidity")]
        public float Humidity { get; set; }
    }

    public partial class Machine
    {
        [JsonProperty("temperature")]
        public float Temperature { get; set; }

        [JsonProperty("pressure")]
        public float Pressure { get; set; }
    }
}
