﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Newtonsoft.Json;
using UVtools.Core.Extensions;
using UVtools.Core.Operations;

namespace UVtools.Core.FileFormats
{
    public class UVJFile : FileFormat
    {
        #region Constants

        private const string FileConfigName = "config.json";
        private const string FolderImageName = "slice";
        private const string FolderPreviewName = "preview";
        private const string FilePreviewHugeName = "preview/huge.png";
        private const string FilePreviewTinyName = "preview/tiny.png";
        #endregion

        #region Sub Classes

        public class Millimeter
        {
            public float X { get; set; }
            public float Y { get; set; }
        }

        public class Size
        {
            public ushort X { get; set; }
            public ushort Y { get; set; }

            public Millimeter Millimeter { get; set; } = new Millimeter();

            public uint Layers { get; set; }
            public float LayerHeight { get; set; }

            public override string ToString()
            {
                return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Millimeter)}: {Millimeter}, {nameof(Layers)}: {Layers}, {nameof(LayerHeight)}: {LayerHeight}";
            }
        }

        public class Exposure
        {
            public float LightOnTime { get; set; }
            public float LightOffTime { get; set; }
            public byte LightPWM { get; set; } = 255;
            public float LiftHeight { get; set; } = 5;
            public float LiftSpeed { get; set; } = 100;
            public float RetractHeight { get; set; }
            public float RetractSpeed { get; set; } = 100;

            public override string ToString()
            {
                return $"{nameof(LightOnTime)}: {LightOnTime}, {nameof(LightOffTime)}: {LightOffTime}, {nameof(LightPWM)}: {LightPWM}, {nameof(LiftHeight)}: {LiftHeight}, {nameof(LiftSpeed)}: {LiftSpeed}, {nameof(RetractHeight)}: {RetractHeight}, {nameof(RetractSpeed)}: {RetractSpeed}";
            }
        }

        public class Bottom
        {
            public float LightOnTime { get; set; }
            public float LightOffTime { get; set; }
            public byte LightPWM { get; set; } = 255;
            public float LiftHeight { get; set; } = 5;
            public float LiftSpeed { get; set; } = 100;
            public float RetractHeight { get; set; }
            public float RetractSpeed { get; set; } = 100;
            public ushort Count { get; set; }

            public override string ToString()
            {
                return $"{nameof(LightOnTime)}: {LightOnTime}, {nameof(LightOffTime)}: {LightOffTime}, {nameof(LightPWM)}: {LightPWM}, {nameof(LiftHeight)}: {LiftHeight}, {nameof(LiftSpeed)}: {LiftSpeed}, {nameof(RetractHeight)}: {RetractHeight}, {nameof(RetractSpeed)}: {RetractSpeed}, {nameof(Count)}: {Count}";
            }
        }

        public class LayerData
        {
            public float Z { get; set; }
            public Exposure Exposure { get; set; }

            public override string ToString()
            {
                return $"{nameof(Z)}: {Z}, {nameof(Exposure)}: {Exposure}";
            }
        }

        public class Properties
        {
            public Size Size { get; set; } = new Size();
            public Exposure Exposure { get; set; } = new Exposure();
            public Bottom Bottom { get; set; } = new Bottom();
            public byte AntiAliasLevel { get; set; } = 1;

            public override string ToString()
            {
                return $"{nameof(Size)}: {Size}, {nameof(Exposure)}: {Exposure}, {nameof(Bottom)}: {Bottom}, {nameof(AntiAliasLevel)}: {AntiAliasLevel}";
            }
        }

        public class Settings
        {
            public Properties Properties { get; set; } = new Properties();
            public List<LayerData> Layers { get; set; } = new List<LayerData>();

            public override string ToString()
            {
                return $"{nameof(Properties)}: {Properties}, {nameof(Layers)}: {Layers.Count}";
            }
        }

        #endregion

        #region Properties
        public Settings JsonSettings { get; set; } = new();

        public override FileFormatType FileType => FileFormatType.Archive;

        public override FileExtension[] FileExtensions { get; } = {
            new("uvj", "UVJ")
        };

        public override PrintParameterModifier[] PrintParameterModifiers { get; } = {
            PrintParameterModifier.BottomLayerCount,
            PrintParameterModifier.BottomExposureSeconds,
            PrintParameterModifier.ExposureSeconds,

            PrintParameterModifier.BottomLightOffDelay,
            PrintParameterModifier.LightOffDelay,
            PrintParameterModifier.BottomLiftHeight,
            PrintParameterModifier.BottomLiftSpeed,
            PrintParameterModifier.LiftHeight,
            PrintParameterModifier.LiftSpeed,
            PrintParameterModifier.RetractSpeed,

            PrintParameterModifier.BottomLightPWM,
            PrintParameterModifier.LightPWM,
        };

        public override byte ThumbnailsCount { get; } = 2;

        public override System.Drawing.Size[] ThumbnailsOriginalSize { get; } = {new System.Drawing.Size(400, 400), new System.Drawing.Size(800, 480) };

        public override uint ResolutionX
        {
            get => JsonSettings.Properties.Size.X;
            set
            {
                JsonSettings.Properties.Size.X = (ushort) value;
                RaisePropertyChanged();
            }
        }

        public override uint ResolutionY
        {
            get => JsonSettings.Properties.Size.Y;
            set
            {
                JsonSettings.Properties.Size.Y = (ushort) value;
                RaisePropertyChanged();
            }
        }

        public override float DisplayWidth
        {
            get => JsonSettings.Properties.Size.Millimeter.X;
            set
            {
                JsonSettings.Properties.Size.Millimeter.X = (float) Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float DisplayHeight
        {
            get => JsonSettings.Properties.Size.Millimeter.Y;
            set
            {
                JsonSettings.Properties.Size.Millimeter.Y = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override bool MirrorDisplay { get; set; }

        public override byte AntiAliasing
        {
            get => JsonSettings.Properties.AntiAliasLevel;
            set
            {
                JsonSettings.Properties.AntiAliasLevel = value.Clamp(1, 16);
                RaisePropertyChanged();
            }

        }

        public override bool SupportPerLayerSettings => true;

        public override float LayerHeight
        {
            get => JsonSettings.Properties.Size.LayerHeight;
            set
            {
                JsonSettings.Properties.Size.LayerHeight = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override uint LayerCount
        {
            set
            {
                JsonSettings.Properties.Size.Layers = LayerCount;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NormalLayerCount));
            }
        }

        public override ushort BottomLayerCount
        {
            get => JsonSettings.Properties.Bottom.Count;
            set
            {
                JsonSettings.Properties.Bottom.Count = value;
                RaisePropertyChanged();
            }
        }

        public override float BottomExposureTime
        {
            get => JsonSettings.Properties.Bottom.LightOnTime;
            set
            {
                JsonSettings.Properties.Bottom.LightOnTime = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float ExposureTime
        {
            get => JsonSettings.Properties.Exposure.LightOnTime;
            set
            {
                JsonSettings.Properties.Exposure.LightOnTime = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLightOffDelay
        {
            get => JsonSettings.Properties.Bottom.LightOffTime;
            set
            {
                JsonSettings.Properties.Bottom.LightOffTime = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LightOffDelay
        {
            get => JsonSettings.Properties.Exposure.LightOffTime;
            set
            {
                JsonSettings.Properties.Exposure.LightOffTime = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLiftHeight
        {
            get => JsonSettings.Properties.Bottom.LiftHeight;
            set
            {
                JsonSettings.Properties.Bottom.LiftHeight = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LiftHeight
        {
            get => JsonSettings.Properties.Exposure.LiftHeight;
            set
            {
                JsonSettings.Properties.Exposure.LiftHeight = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLiftSpeed
        {
            get => JsonSettings.Properties.Bottom.LiftSpeed;
            set
            {
                JsonSettings.Properties.Bottom.LiftSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LiftSpeed
        {
            get => JsonSettings.Properties.Exposure.LiftSpeed;
            set
            {
                JsonSettings.Properties.Exposure.LiftSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float RetractSpeed
        {
            get => JsonSettings.Properties.Exposure.RetractSpeed;
            set
            {
                JsonSettings.Properties.Exposure.RetractSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override byte BottomLightPWM
        {
            get => JsonSettings.Properties.Bottom.LightPWM;
            set
            {
                JsonSettings.Properties.Bottom.LightPWM = value;
                RaisePropertyChanged();
            }
        }

        public override byte LightPWM
        {
            get => JsonSettings.Properties.Exposure.LightPWM;
            set
            {
                JsonSettings.Properties.Exposure.LightPWM = value;
                RaisePropertyChanged();
            }
        }

        public override object[] Configs => new[] {(object) JsonSettings.Properties.Size, JsonSettings.Properties.Size.Millimeter, JsonSettings.Properties.Bottom, JsonSettings.Properties.Exposure};
        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();
            JsonSettings.Layers = new List<LayerData>();
        }

        protected override void EncodeInternally(string fileFullPath, OperationProgress progress)
        {
            // Redo layer data
            JsonSettings.Layers.Clear();
            for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                var layer = this[layerIndex];
                JsonSettings.Layers.Add(new LayerData
                {
                    Z = layer.PositionZ,
                    Exposure = new Exposure
                    {
                        LiftHeight = layer.LiftHeight,
                        LiftSpeed = layer.LiftSpeed,
                        RetractHeight = layer.LiftHeight+1,
                        RetractSpeed = layer.RetractSpeed,
                        LightOffTime = layer.LightOffDelay,
                        LightOnTime = layer.ExposureTime,
                        LightPWM = layer.LightPWM
                    }
                });
            }

            using (ZipArchive outputFile = ZipFile.Open(fileFullPath, ZipArchiveMode.Create))
            {
                outputFile.PutFileContent(FileConfigName, JsonConvert.SerializeObject(JsonSettings), ZipArchiveMode.Create);

                if (CreatedThumbnailsCount > 0)
                {
                    using (Stream stream = outputFile.CreateEntry(FilePreviewTinyName).Open())
                    {
                        using (var vec = new VectorOfByte())
                        {
                            CvInvoke.Imencode(".png", Thumbnails[0], vec);
                            stream.WriteBytes(vec.ToArray());
                            stream.Close();
                        }
                    }
                }

                if (CreatedThumbnailsCount > 1)
                {
                    using (Stream stream = outputFile.CreateEntry(FilePreviewHugeName).Open())
                    {
                        using (var vec = new VectorOfByte())
                        {
                            CvInvoke.Imencode(".png", Thumbnails[1], vec);
                            stream.WriteBytes(vec.ToArray());
                            stream.Close();
                        }
                    }
                }

                for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
                {
                    progress.Token.ThrowIfCancellationRequested();

                    Layer layer = this[layerIndex];

                    var layerimagePath = $"{FolderImageName}/{layerIndex:D8}.png";
                    outputFile.PutFileContent(layerimagePath, layer.CompressedBytes, ZipArchiveMode.Create);
                    
                    progress++;
                }
            }
        }

        protected override void DecodeInternally(string fileFullPath, OperationProgress progress)
        {
            using (var inputFile = ZipFile.Open(fileFullPath, ZipArchiveMode.Read))
            {
                var entry = inputFile.GetEntry(FileConfigName);
                if (ReferenceEquals(entry, null))
                {
                    Clear();
                    throw new FileLoadException($"{FileConfigName} not found", fileFullPath);
                }

                JsonSettings = Helpers.JsonDeserializeObject<Settings>(entry.Open());
                
                LayerManager = new LayerManager(JsonSettings.Properties.Size.Layers, this);

                entry = inputFile.GetEntry(FilePreviewTinyName);
                if (!ReferenceEquals(entry, null))
                {
                    using (Stream stream = entry.Open())
                    {
                        Mat image = new Mat();
                        CvInvoke.Imdecode(stream.ToArray(), ImreadModes.AnyColor, image);
                        Thumbnails[0] = image;
                        stream.Close();
                    }
                }

                entry = inputFile.GetEntry(FilePreviewHugeName);
                if (entry is not null)
                {
                    using Stream stream = entry.Open();
                    Mat image = new Mat();
                    CvInvoke.Imdecode(stream.ToArray(), ImreadModes.AnyColor, image);
                    Thumbnails[1] = image;
                    stream.Close();
                }

                for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
                {
                    entry = inputFile.GetEntry($"{FolderImageName}/{layerIndex:D8}.png");
                    if (entry is null) continue;

                    this[layerIndex] = new Layer(layerIndex, entry.Open(), LayerManager)
                    {
                        PositionZ = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int) layerIndex].Z : GetHeightFromLayer(layerIndex),
                        LiftHeight = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.LiftHeight : GetInitialLayerValueOrNormal(layerIndex, BottomLiftHeight, LiftHeight),
                        LiftSpeed = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.LiftSpeed : GetInitialLayerValueOrNormal(layerIndex, BottomLiftSpeed, LiftSpeed),
                        RetractSpeed = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.RetractSpeed : RetractSpeed,
                        LightOffDelay = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.LightOffTime : GetInitialLayerValueOrNormal(layerIndex, BottomLightOffDelay, LightOffDelay),
                        ExposureTime = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.LightOnTime : GetInitialLayerValueOrNormal(layerIndex, BottomExposureTime, ExposureTime),
                        LightPWM = JsonSettings.Layers.Count >= layerIndex ? JsonSettings.Layers[(int)layerIndex].Exposure.LightPWM : GetInitialLayerValueOrNormal(layerIndex, BottomLightPWM, LightPWM),
                    };
                }
                
                progress.ProcessedItems++;
            }

            LayerManager.GetBoundingRectangle(progress);
        }

        public override void SaveAs(string filePath = null, OperationProgress progress = null)
        {
            if (RequireFullEncode)
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    FileFullPath = filePath;
                }
                Encode(FileFullPath, progress);
                return;
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                File.Copy(FileFullPath, filePath, true);
                FileFullPath = filePath;

            }

            using (var outputFile = ZipFile.Open(FileFullPath, ZipArchiveMode.Update))
            {
                outputFile.PutFileContent(FileConfigName, JsonConvert.SerializeObject(JsonSettings), ZipArchiveMode.Update);
            }

            //Decode(FileFullPath, progress);
        }
        #endregion
    }
}
