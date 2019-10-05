#region Usings

using System;
using System.Configuration;
using System.Xml;

#endregion

namespace DartboardRecognition.Services
{
    public class ConfigService
    {
        private readonly object locker;
        private readonly XmlDocument appConfig;

        public ConfigService()
        {
            locker = new object();
            appConfig = new XmlDocument();
            appConfig.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }

        public void Set(string key, object value)
        {
            lock (locker)
            {
                if (appConfig.DocumentElement != null)
                {
                    foreach (XmlElement element in appConfig.DocumentElement)
                    {
                        if (element.Name.Equals("appSettings"))
                        {
                            foreach (XmlNode node in element.ChildNodes)
                            {
                                if (node.Attributes != null && node.Attributes[0].Value.Equals(key))
                                {
                                    node.Attributes[1].Value = value.ToString();
                                }
                            }
                        }
                    }
                }

                appConfig.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public T Get<T>(string key)
        {
            object value;

            if (typeof(T) == typeof(double))
            {
                value = double.Parse(ConfigurationManager.AppSettings[$"{key}"]);
            }
            else if (typeof(T) == typeof(int))
            {
                value = int.Parse(ConfigurationManager.AppSettings[$"{key}"]);
            }
            else if (typeof(T) == typeof(string))
            {
                value = ConfigurationManager.AppSettings[$"{key}"];
            }
            else
            {
                throw new Exception($"Not supported type for {nameof(Get)} method");
            }

            return (T) value;
        }
    }
}