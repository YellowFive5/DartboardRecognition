#region Usings

using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;

#endregion

namespace DartboardRecognition.Services
{
    public class ConfigService
    {
        private readonly object locker;
        private readonly Logger logger;
        private readonly IConfigurationRoot appSettings;
        private readonly string appSettingsJsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        public ConfigService(Logger logger)
        {
            this.logger = logger;
            locker = new object();
            appSettings = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", true, true)
                          .Build();
        }

        public void Write(string key, object value)
        {
            lock (locker)
            {
                var json = File.ReadAllText(appSettingsJsonFilePath);
                var jObject = JObject.Parse(json);
                var appSettingsNode = (JObject) jObject["AppSettings"];
                appSettingsNode[key] = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                var outputText = Newtonsoft.Json.JsonConvert.SerializeObject(jObject, Newtonsoft.Json.Formatting.Indented);

                File.WriteAllText(appSettingsJsonFilePath, outputText);
            }
        }

        public T Read<T>(string key)
        {
            lock (locker)
            {
                appSettings.Reload();

                object value;
                if (typeof(T) == typeof(double))
                {
                    value = double.Parse(appSettings.GetSection($"AppSettings:{key}").Value, CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(float))
                {
                    value = float.Parse(appSettings.GetSection($"AppSettings:{key}").Value, CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(int))
                {
                    value = int.Parse(appSettings.GetSection($"AppSettings:{key}").Value);
                }
                else if (typeof(T) == typeof(bool))
                {
                    value = bool.Parse(appSettings.GetSection($"AppSettings:{key}").Value);
                }
                else if (typeof(T) == typeof(string))
                {
                    value = appSettings.GetSection($"AppSettings:{key}").Value;
                }
                else
                {
                    throw new Exception($"Not supported type for {nameof(Read)} method");
                }

                return (T) value;
            }
        }
    }
}