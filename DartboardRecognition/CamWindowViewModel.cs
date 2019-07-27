#region Usings

using System;
using System.Configuration;
using System.Xml;

#endregion

namespace DartboardRecognition
{
    public class CamWindowViewModel
    {
        private CamWindow view;
        private int camNumber;

        public CamWindowViewModel(CamWindow view, int camNumber)
        {
            this.camNumber = camNumber;
            this.view = view;
            SetWindowTitle();
            LoadSettings();
        }

        private void SetWindowTitle()
        {
            view.Title = $"Cam {camNumber.ToString()}";
        }

        private void LoadSettings()
        {
            var camNumberStr = camNumber.ToString();
            view.TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMinSlider"]);
            view.TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMaxSlider"]);
            view.RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosXSlider"]);
            view.RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosYSlider"]);
            view.RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiWidthSlider"]);
            view.RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiHeightSlider"]);
            view.SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceSlider"]);
            view.IndexBox.Text = ConfigurationManager.AppSettings[$"Cam{camNumberStr}IndexBox"];
            view.SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceCenterSlider"]);
            view.SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceLeftSlider"]);
            view.SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceRightSlider"]);
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            var camNumberStr = camNumber.ToString();
            Rewrite($"Cam{camNumberStr}TresholdMinSlider", view.TresholdMinSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}TresholdMaxSlider", view.TresholdMaxSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}RoiPosXSlider", view.RoiPosXSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}RoiPosYSlider", view.RoiPosYSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}RoiWidthSlider", view.RoiWidthSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}RoiHeightSlider", view.RoiHeightSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}SurfaceSlider", view.SurfaceSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}IndexBox", view.IndexBox.Text);
            Rewrite($"Cam{camNumberStr}SurfaceCenterSlider", view.SurfaceCenterSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}SurfaceLeftSlider", view.SurfaceLeftSlider.Value.ToString());
            Rewrite($"Cam{camNumberStr}SurfaceRightSlider", view.SurfaceRightSlider.Value.ToString());

            void Rewrite(string key, string value)
            {
                doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                foreach (XmlElement element in doc.DocumentElement)
                {
                    if (element.Name.Equals("appSettings"))
                    {
                        foreach (XmlNode node in element.ChildNodes)
                        {
                            if (node.Attributes[0].Value.Equals(key))
                            {
                                node.Attributes[1].Value = value;
                            }
                        }
                    }
                }

                doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}