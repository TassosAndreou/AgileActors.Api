using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileActors.Shared
{
    public class WeatherResponse
    {
        public List<WeatherDescription>? weather { get; set; }
        public MainInfo main { get; set; } = new();
        public WindInfo wind { get; set; } = new();
        public long dt { get; set; }

        public class WeatherDescription
        {
            public string? main { get; set; }
            public string? description { get; set; }
            public string? icon { get; set; }
        }

        public class MainInfo
        {
            public double temp { get; set; }
            public double feels_like { get; set; }
            public int humidity { get; set; }
        }

        public class WindInfo
        {
            public double speed { get; set; }
            public int deg { get; set; }
        }
    }
}
