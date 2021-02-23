﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using UVtools.Core.Extensions;
using UVtools.Core.Objects;
using UVtools.Core.Operations;

namespace UVtools.Core.FileFormats
{
    /// <summary>
    /// Slicer <see cref="FileFormat"/> representation
    /// </summary>
    public abstract class FileFormat : BindableBase, IDisposable, IEquatable<FileFormat>, IEnumerable<Layer>
    {
        #region Constants
        public const string TemporaryFileAppend = ".tmp";
        public const ushort ExtraPrintTime = 300;

        private const string ExtractConfigFileName = "Configuration";
        private const string ExtractConfigFileExtension = "ini";

        public const ushort DefaultBottomLayerCount = 4;

        public const float DefaultBottomExposureTime = 30;
        public const float DefaultBottomLiftHeight = 5;
        public const float DefaultLiftHeight = 5;
        public const float DefaultBottomLiftSpeed = 100;

        public const float DefaultExposureTime = 3;
        public const float DefaultLiftSpeed = 100;
        public const float DefaultRetractSpeed = 100;
        public const float DefaultBottomLightOffDelay = 0;
        public const float DefaultLightOffDelay = 0;
        public const byte DefaultBottomLightPWM = 255;
        public const byte DefaultLightPWM = 255;

        public const float MinimumLayerHeight = 0.01f;
        public const float MaximumLayerHeight = 0.20f;
        #endregion 

        #region Enums

        /// <summary>
        /// Enumeration of file format types
        /// </summary>
        public enum FileFormatType : byte
        {
            Archive,
            Binary
        }

        /// <summary>
        /// Enumeration of file thumbnail size types
        /// </summary>
        public enum FileThumbnailSize : byte
        {
            Small = 0,
            Large
        }
        #endregion

        #region Sub Classes
        /// <summary>
        /// Available Print Parameters to modify
        /// </summary>
        public class PrintParameterModifier
        {
            
            #region Instances
            public static PrintParameterModifier BottomLayerCount { get; } = new PrintParameterModifier("Bottom layer count", null, "layers",0, ushort.MaxValue, 0);
            public static PrintParameterModifier BottomExposureSeconds { get; } = new PrintParameterModifier("Bottom exposure time", null, "s", 0.1M, 1000, 2);
            public static PrintParameterModifier ExposureSeconds { get; } = new PrintParameterModifier("Exposure time", null, "s", 0.1M, 1000, 2);
            
            public static PrintParameterModifier BottomLightOffDelay { get; } = new PrintParameterModifier("Bottom light-off seconds", null, "s");
            public static PrintParameterModifier LightOffDelay { get; } = new PrintParameterModifier("Light-off seconds", null, "s");
            public static PrintParameterModifier BottomLiftHeight { get; } = new PrintParameterModifier("Bottom lift height", @"Modify 'Bottom lift height' millimeters between bottom layers", "mm", 1);
            public static PrintParameterModifier LiftHeight { get; } = new PrintParameterModifier("Lift height", @"Modify 'Lift height' millimeters between layers", "mm", 1);
            public static PrintParameterModifier BottomLiftSpeed { get; } = new PrintParameterModifier("Bottom lift Speed", @"Modify 'Bottom lift Speed' mm/min between bottom layers", "mm/min", 10);
            public static PrintParameterModifier LiftSpeed { get; } = new PrintParameterModifier("Lift speed", @"Modify 'Lift speed' mm/min between layers", "mm/min", 10, 5000, 2);
            public static PrintParameterModifier RetractSpeed { get; } = new PrintParameterModifier("Retract speed", @"Modify 'Retract speed' mm/min between layer", "mm/min", 10, 5000, 2);

            public static PrintParameterModifier BottomLightPWM { get; } = new PrintParameterModifier("Bottom light PWM", @"Modify 'Bottom light PWM' value", null, 1, byte.MaxValue, 0);
            public static PrintParameterModifier LightPWM { get; } = new PrintParameterModifier("Light PWM", @"Modify 'Light PWM' value", null, 1, byte.MaxValue, 0);

            public static PrintParameterModifier[] Parameters = {
                BottomLayerCount,
                BottomExposureSeconds,
                ExposureSeconds,

                BottomLightOffDelay,
                LightOffDelay,
                BottomLiftHeight,
                BottomLiftSpeed,
                LiftHeight,
                LiftSpeed,
                RetractSpeed,

                BottomLightPWM,
                LightPWM
            };
            #endregion

            #region Properties

            /// <summary>
            /// Gets the name
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the description
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// Gets the value unit
            /// </summary>
            public string ValueUnit { get; }

            /// <summary>
            /// Gets the minimum value
            /// </summary>
            public decimal Minimum { get; }

            /// <summary>
            /// Gets the maximum value
            /// </summary>
            public decimal Maximum { get; }

            /// <summary>
            /// Gets the number of decimal plates
            /// </summary>
            public byte DecimalPlates { get; }

            /// <summary>
            /// Gets or sets the current / old value
            /// </summary>
            public decimal OldValue { get; set; }

            /// <summary>
            /// Gets or sets the new value
            /// </summary>
            public decimal NewValue { get; set; }

            public decimal Value
            {
                get => NewValue;
                set => OldValue = NewValue = value;
            }

            /// <summary>
            /// Gets if the value has changed
            /// </summary>
            public bool HasChanged => OldValue != NewValue;
            #endregion

            #region Constructor
            public PrintParameterModifier(string name, string description = null, string valueUnit = null, decimal minimum = 0, decimal maximum = 1000, byte decimalPlates = 2)
            {
                Name = name;
                Description = description ?? $"Modify '{name}'";
                ValueUnit = valueUnit ?? string.Empty;
                Minimum = minimum;
                Maximum = maximum;
                DecimalPlates = decimalPlates;
            }
            #endregion

            #region Overrides
            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(ValueUnit)}: {ValueUnit}, {nameof(Minimum)}: {Minimum}, {nameof(Maximum)}: {Maximum}, {nameof(DecimalPlates)}: {DecimalPlates}, {nameof(OldValue)}: {OldValue}, {nameof(NewValue)}: {NewValue}, {nameof(HasChanged)}: {HasChanged}";
            }
            #endregion
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets the available formats to process
        /// </summary>
        public static FileFormat[] AvailableFormats { get; } =
        {
            new SL1File(),      // Prusa SL1
            new ChituboxZipFile(), // Zip
            new ChituboxFile(), // cbddlp, cbt, photon
            new PhotonSFile(), // photons
            new PHZFile(), // phz
            new FDGFile(), // fdg
            new PhotonWorkshopFile(),   // PSW
            new ZCodexFile(),   // zcodex
            new CWSFile(),   // CWS
            //new MakerbaseFile(),   // MKS
            new LGSFile(),   // LGS, LGS30
            new UVJFile(),   // UVJ
            new ImageFile(),   // images
        };

        public static string AllSlicerFiles => AvailableFormats.Aggregate("All slicer files|",
            (current, fileFormat) => current.EndsWith("|")
                ? $"{current}{fileFormat.FileFilterExtensionsOnly}"
                : $"{current}; {fileFormat.FileFilterExtensionsOnly}");

        /// <summary>
        /// Gets all filters for open and save file dialogs
        /// </summary>
        public static string AllFileFilters =>
            AllSlicerFiles
            +
            AvailableFormats.Aggregate(string.Empty,
                (current, fileFormat) => $"{current}|" + fileFormat.FileFilter);

        public static List<KeyValuePair<string, List<string>>> AllFileFiltersAvalonia
        {
            get
            {
                var result = new List<KeyValuePair<string, List<string>>>
                {
                    new KeyValuePair<string, List<string>>("All slicer files", new List<string>())
                };
                
                for (int i = 0; i < AvailableFormats.Length; i++)
                {
                    foreach (var fileExtension in AvailableFormats[i].FileExtensions)
                    {
                        result[0].Value.Add(fileExtension.Extension);
                        result.Add(new KeyValuePair<string, List<string>>(fileExtension.Description, new List<string>
                        {
                            fileExtension.Extension
                        }));
                    }
                }

                return result;
            }
            
        }

        public static List<FileExtension> AllFileExtensions
        {
            get
            {
                List<FileExtension> extensions = new();
                foreach (var slicerFile in AvailableFormats)
                {
                    extensions.AddRange(slicerFile.FileExtensions);
                }
                return extensions;
            }
        }

        public static List<string> AllFileExtensionsString => (from slicerFile in AvailableFormats from extension in slicerFile.FileExtensions select extension.Extension).ToList();


        /// <summary>
        /// Gets the count of available file extensions
        /// </summary>
        public static byte FileExtensionsCount => AvailableFormats.Aggregate<FileFormat, byte>(0, (current, fileFormat) => (byte) (current + fileFormat.FileExtensions.Length));

        /// <summary>
        /// Find <see cref="FileFormat"/> by an extension
        /// </summary>
        /// <param name="extension">Extension name to find</param>
        /// <param name="isFilePath">True if <see cref="extension"/> is a file path rather than only a extension name</param>
        /// <param name="createNewInstance">True to create a new instance of found file format, otherwise will return a pre created one which should be used for read-only purpose</param>
        /// <returns><see cref="FileFormat"/> object or null if not found</returns>
        public static FileFormat FindByExtension(string extension, bool isFilePath = false, bool createNewInstance = false)
        {
            return (from fileFormat in AvailableFormats where fileFormat.IsExtensionValid(extension, isFilePath) select createNewInstance ? (FileFormat) Activator.CreateInstance(fileFormat.GetType()) : fileFormat).FirstOrDefault();
        }

        public static FileExtension FindExtension(string extension, bool isFilePath = false, bool createNewInstance = false)
        {
            return AvailableFormats.SelectMany(format => format.FileExtensions).FirstOrDefault(ext => ext.Equals(extension));
        }

        /// <summary>
        /// Find <see cref="FileFormat"/> by an type
        /// </summary>
        /// <param name="type">Type to find</param>
        /// <param name="createNewInstance">True to create a new instance of found file format, otherwise will return a pre created one which should be used for read-only purpose</param>
        /// <returns><see cref="FileFormat"/> object or null if not found</returns>
        public static FileFormat FindByType(Type type, bool createNewInstance = false)
        {
            return (from t in AvailableFormats where type == t.GetType() select createNewInstance ? (FileFormat) Activator.CreateInstance(type) : t).FirstOrDefault();
        }
        #endregion

        #region Members
        private bool _haveModifiedLayers;
        private LayerManager _layerManager;
        private float _printTime;
        private float _materialMilliliters;
        private float _maxPrintHeight;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the file format type
        /// </summary>
        public abstract FileFormatType FileType { get; }

        /// <summary>
        /// Gets the valid file extensions for this <see cref="FileFormat"/>
        /// </summary>
        public abstract FileExtension[] FileExtensions { get; }

        /// <summary>
        /// Gets the available <see cref="FileFormat.PrintParameterModifier"/>
        /// </summary>
        public abstract PrintParameterModifier[] PrintParameterModifiers { get; }

        /// <summary>
        /// Gets the available <see cref="FileFormat.PrintParameterModifier"/> per layer
        /// </summary>
        public virtual PrintParameterModifier[] PrintParameterPerLayerModifiers { get; } = null;

        /// <summary>
        /// Checks if a <see cref="PrintParameterModifier"/> exists on print parameters
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns>True if exists, otherwise false</returns>
        public bool HavePrintParameterModifier(PrintParameterModifier modifier) =>
            PrintParameterModifiers is not null && PrintParameterModifiers.Contains(modifier);

        /// <summary>
        /// Checks if a <see cref="PrintParameterModifier"/> exists on print parameters
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns>True if exists, otherwise false</returns>
        public bool HavePrintParameterPerLayerModifier(PrintParameterModifier modifier) =>
            PrintParameterPerLayerModifiers is not null && PrintParameterPerLayerModifiers.Contains(modifier);

        /// <summary>
        /// Gets the file filter for open and save dialogs
        /// </summary>
        public string FileFilter {
            get
            {
                var result = string.Empty;

                foreach (var fileExt in FileExtensions)
                {
                    if (!ReferenceEquals(result, string.Empty))
                    {
                        result += '|';
                    }
                    result += fileExt.Filter;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets all valid file extensions for Avalonia file dialog
        /// </summary>
        public List<KeyValuePair<string, List<string>>> FileFilterAvalonia 
            => FileExtensions.Select(fileExt => new KeyValuePair<string, List<string>>(fileExt.Description, new List<string> {fileExt.Extension})).ToList();

        /// <summary>
        /// Gets all valid file extensions in "*.extension1;*.extension2" format
        /// </summary>
        public string FileFilterExtensionsOnly
        {
            get
            {
                var result = string.Empty;

                foreach (var fileExt in FileExtensions)
                {
                    if (!ReferenceEquals(result, string.Empty))
                    {
                        result += "; ";
                    }
                    result += $"*.{fileExt.Extension}";
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets if change a global property should rebuild every layer data based on them
        /// </summary>
        public bool SuppressRebuildProperties { get; set; }

        /// <summary>
        /// Gets the input file path loaded into this <see cref="FileFormat"/>
        /// </summary>
        public string FileFullPath { get; set; }

        /// <summary>
        /// Gets the thumbnails count present in this file format
        /// </summary>
        public abstract byte ThumbnailsCount { get; }

        /// <summary>
        /// Gets the number of created thumbnails
        /// </summary>
        public byte CreatedThumbnailsCount {
            get
            {
                if (Thumbnails is null) return 0;
                byte count = 0;

                foreach (var thumbnail in Thumbnails)
                {
                    if (thumbnail is null) continue;
                    count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the original thumbnail sizes
        /// </summary>
        public abstract Size[] ThumbnailsOriginalSize { get; }

        /// <summary>
        /// Gets the thumbnails for this <see cref="FileFormat"/>
        /// </summary>
        public Mat[] Thumbnails { get; set; }

        /// <summary>
        /// Gets the cached layers into compressed bytes
        /// </summary>
        public LayerManager LayerManager
        {
            get => _layerManager;
            set
            {
                var oldLayerManager = _layerManager;
                if (!RaiseAndSetIfChanged(ref _layerManager, value) || value is null) return;

                if(!ReferenceEquals(this, _layerManager.SlicerFile)) // Auto fix parent slicer file
                {
                    _layerManager.SlicerFile = this;
                }

                // Recalculate changes
                PrintHeight = PrintHeight;
                PrintTime = PrintTimeComputed;
                MaterialMilliliters = 0;

                if (oldLayerManager is null) return; // Init

                if (oldLayerManager.Count != LayerCount)
                {
                    LayerCount = _layerManager.Count;
                    if (SuppressRebuildProperties) return;
                    if (LayerCount == 0 || this[LayerCount - 1] is null) return; // Not initialized
                    LayerManager.RebuildLayersProperties();
                }
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of the object
        /// </summary>
        public Rectangle BoundingRectangle => _layerManager?.BoundingRectangle ?? Rectangle.Empty;

        /// <summary>
        /// Gets the bounding rectangle of the object in millimeters
        /// </summary>
        public RectangleF BoundingRectangleMillimeters => _layerManager?.BoundingRectangleMillimeters ?? Rectangle.Empty;

        /// <summary>
        /// Gets or sets if modifications require a full encode to save
        /// </summary>
        public bool RequireFullEncode
        {
            get => _haveModifiedLayers || LayerManager.IsModified;
            set => _haveModifiedLayers = value;
        } // => LayerManager.IsModified;

        /// <summary>
        /// Gets the image width resolution
        /// </summary>
        public Size Resolution
        {
            get => new((int)ResolutionX, (int)ResolutionY);
            set
            {
                ResolutionX = (uint) value.Width;
                ResolutionY = (uint) value.Height;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the image width resolution
        /// </summary>
        public abstract uint ResolutionX { get; set; }

        /// <summary>
        /// Gets the image height resolution
        /// </summary>
        public abstract uint ResolutionY { get; set; }

        /// <summary>
        /// Gets the size of display in millimeters
        /// </summary>
        public SizeF Display
        {
            get => new(DisplayWidth, DisplayHeight);
            set
            {
                DisplayWidth = value.Width;
                DisplayHeight = value.Height;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display width in millimeters
        /// </summary>
        public abstract float DisplayWidth { get; set; }

        /// <summary>
        /// Gets or sets the display height in millimeters
        /// </summary>
        public abstract float DisplayHeight { get; set; }

        /// <summary>
        /// Gets or sets if images need to be mirrored on lcd to print on the correct orientation
        /// </summary>
        public abstract bool MirrorDisplay { get; set; }

        /// <summary>
        /// Gets or sets the maximum printer build Z volume
        /// </summary>
        public virtual float MaxPrintHeight
        {
            get => _maxPrintHeight > 0 ? _maxPrintHeight : PrintHeight;
            set => RaiseAndSetIfChanged(ref _maxPrintHeight, value);
        }

        /// <summary>
        /// Gets or sets the pixels per mm on X direction
        /// </summary>
        public virtual float Xppmm
        {
            get => DisplayWidth > 0 ? ResolutionX / DisplayWidth : 0;
            set
            {
                RaisePropertyChanged(nameof(Xppmm));
                RaisePropertyChanged(nameof(Ppmm));
            }
        }

        /// <summary>
        /// Gets or sets the pixels per mm on Y direction
        /// </summary>
        public virtual float Yppmm
        {
            get => DisplayHeight > 0 ? ResolutionY / DisplayHeight : 0;
            set
            {
                RaisePropertyChanged(nameof(Yppmm));
                RaisePropertyChanged(nameof(Ppmm));
            }
        }

        /// <summary>
        /// Gets or sets the pixels per mm
        /// </summary>
        public SizeF Ppmm
        {
            get => new(Xppmm, Yppmm);
            set
            {
                Xppmm = value.Width;
                Yppmm = value.Height;
            }
        }

        /// <summary>
        /// Gets the pixel width in millimeters
        /// </summary>
        public float PixelWidth => DisplayWidth > 0 ? (float) Math.Round(DisplayWidth / ResolutionX, 3) : 0;

        /// <summary>
        /// Gets the pixel height in millimeters
        /// </summary>
        public float PixelHeight => DisplayHeight > 0 ? (float) Math.Round(DisplayHeight / ResolutionY, 3) : 0;

        /// <summary>
        /// Gets the pixel size in millimeters
        /// </summary>
        public SizeF PixelSize => new(PixelWidth, PixelHeight);

        /// <summary>
        /// Gets the maximum pixel between width and height in millimeters
        /// </summary>
        public float PixelSizeMax => PixelSize.Max();

        /// <summary>
        /// Gets the pixel area in millimeters
        /// </summary>
        public float PixelArea => PixelSize.Area();

        /// <summary>
        /// Gets the pixel width in microns
        /// </summary>
        public float PixelWidthMicrons => DisplayWidth > 0 ? (float)Math.Round(DisplayWidth / ResolutionX * 1000, 3) : 0;

        /// <summary>
        /// Gets the pixel height in microns
        /// </summary>
        public float PixelHeightMicrons => DisplayHeight > 0 ? (float)Math.Round(DisplayHeight / ResolutionY * 1000, 3) : 0;

        /// <summary>
        /// Gets the pixel size in microns
        /// </summary>
        public SizeF PixelSizeMicrons => new(PixelWidthMicrons, PixelHeightMicrons);

        /// <summary>
        /// Gets the maximum pixel between width and height in microns
        /// </summary>
        public float PixelSizeMicronsMax => PixelSizeMicrons.Max();

        /// <summary>
        /// Gets the pixel area in millimeters
        /// </summary>
        public float PixelAreaMicrons => PixelSizeMicrons.Area();

        /// <summary>
        /// Checks if this file have AntiAliasing
        /// </summary>
        public bool HaveAntiAliasing => AntiAliasing > 1;

        /// <summary>
        /// Gets or sets the AntiAliasing level
        /// </summary>
        public abstract byte AntiAliasing { get; set; }

        /// <summary>
        /// Gets Layer Height in mm
        /// </summary>
        public abstract float LayerHeight { get; set; }

        /// <summary>
        /// Gets Layer Height in um
        /// </summary>
        public ushort LayerHeightUm => (ushort) (LayerHeight * 1000);


        /// <summary>
        /// Gets or sets the print height in mm
        /// </summary>
        public virtual float PrintHeight
        {
            get => LayerCount == 0 ? 0 : this[LayerCount - 1]?.PositionZ ?? 0;
            set
            {
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the last layer index
        /// </summary>
        public uint LastLayerIndex => LayerCount - 1;

        /// <summary>
        /// Checks if this file format supports per layer settings
        /// </summary>
        public virtual bool SupportPerLayerSettings => !(PrintParameterPerLayerModifiers is null || PrintParameterPerLayerModifiers.Length == 0);

        /// <summary>
        /// Gets or sets the layer count
        /// </summary>
        public virtual uint LayerCount
        {
            get => LayerManager?.Count ?? 0;
            set {
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NormalLayerCount));
            }
        }

        #region Universal Properties

        /// <summary>
        /// Gets or sets the number of initial layer count
        /// </summary>
        public virtual ushort BottomLayerCount { get; set; } = DefaultBottomLayerCount;

        /// <summary>
        /// Gets the number of normal layer count
        /// </summary>
        public uint NormalLayerCount => LayerCount - BottomLayerCount;

        /// <summary>
        /// Gets or sets the initial exposure time for <see cref="BottomLayerCount"/> in seconds
        /// </summary>
        public virtual float BottomExposureTime { get; set; } = DefaultBottomExposureTime;

        /// <summary>
        /// Gets or sets the normal layer exposure time in seconds
        /// </summary>
        public virtual float ExposureTime { get; set; } = DefaultExposureTime;

        /// <summary>
        /// Gets or sets the bottom layer off time in seconds
        /// </summary>
        public virtual float BottomLightOffDelay { get; set; } = DefaultBottomLightOffDelay;

        /// <summary>
        /// Gets or sets the layer off time in seconds
        /// </summary>
        public virtual float LightOffDelay { get; set; } = DefaultLightOffDelay;

        /// <summary>
        /// Gets or sets the bottom lift height in mm
        /// </summary>
        public virtual float BottomLiftHeight { get; set; } = DefaultBottomLiftHeight;

        /// <summary>
        /// Gets or sets the lift height in mm
        /// </summary>
        public virtual float LiftHeight { get; set; } = DefaultLiftHeight;

        /// <summary>
        /// Gets or sets the bottom lift speed in mm/min
        /// </summary>
        public virtual float BottomLiftSpeed { get; set; } = DefaultBottomLiftSpeed;

        /// <summary>
        /// Gets or sets the speed in mm/min
        /// </summary>
        public virtual float LiftSpeed { get; set; } = DefaultLiftSpeed;

        /// <summary>
        /// Gets the speed in mm/min for the retracts
        /// </summary>
        public virtual float RetractSpeed { get; set; } = DefaultRetractSpeed;

        /// <summary>
        /// Gets or sets the bottom pwm value from 0 to 255
        /// </summary>
        public virtual byte BottomLightPWM { get; set; } = DefaultBottomLightPWM;

        /// <summary>
        /// Gets or sets the pwm value from 0 to 255
        /// </summary>
        public virtual byte LightPWM { get; set; } = DefaultLightPWM;

        #endregion

        /// <summary>
        /// Gets the estimate print time in seconds
        /// </summary>
        public virtual float PrintTime
        {
            get
            {
                if (_printTime <= 0)
                {
                    _printTime = PrintTimeComputed;
                }
                return _printTime;
            } 
            set
            {
                if (value <= 0)
                {
                    value = PrintTimeComputed;
                }
                if(!RaiseAndSetIfChanged(ref _printTime, value)) return;
                RaisePropertyChanged(nameof(PrintTimeHours));
                RaisePropertyChanged(nameof(PrintTimeString));
            }
        }

        /// <summary>
        /// Gets the calculated estimate print time in seconds
        /// </summary>
        public float PrintTimeComputed
        {
            get
            {
                if (LayerCount == 0) return 0;
                float time = ExtraPrintTime;
                bool computeGeneral = LayerManager is null;
                if (!computeGeneral)
                {
                    foreach (var layer in this)
                    {
                        if (layer is null)
                        {
                            computeGeneral = true;
                            break;
                        }

                        var lightOffDelay = OperationCalculator.LightOffDelayC.CalculateSeconds(layer.LiftHeight, layer.LiftSpeed, layer.RetractSpeed);
                        time += layer.ExposureTime;
                        if (lightOffDelay >= layer.LightOffDelay)
                            time += lightOffDelay;
                        else
                            time += layer.LightOffDelay;
                    }
                }

                if (computeGeneral)
                {
                    time = ExtraPrintTime + 
                           BottomLightOffDelay * BottomLayerCount +
                           LightOffDelay * NormalLayerCount +
                           OperationCalculator.LightOffDelayC.CalculateSeconds(BottomLiftHeight, BottomLiftSpeed, RetractSpeed) * BottomLayerCount +
                           OperationCalculator.LightOffDelayC.CalculateSeconds(LiftHeight, LiftSpeed, RetractSpeed) * NormalLayerCount;
                }

                return (float) Math.Round(time, 2);
            }
        }

        /// <summary>
        /// Gets the estimate print time in hours
        /// </summary>
        public float PrintTimeHours => (float) Math.Round(PrintTime / 3600, 2);

        /// <summary>
        /// Gets the estimate print time in hours and minutes formatted
        /// </summary>
        public string PrintTimeString => TimeSpan.FromSeconds(PrintTime).ToString("hh\\hmm\\m");

        /// <summary>
        /// Gets the estimate used material in ml
        /// </summary>
        public virtual float MaterialMilliliters {
            get => _materialMilliliters;
            set
            {
                if (value <= 0)
                {
                    value = (float)Math.Round(this.Where(layer => layer is not null).Sum(layer => layer.MaterialMilliliters), 3); ;
                }
                RaiseAndSetIfChanged(ref _materialMilliliters, value);
            }
        }

        //public float MaterialMillilitersComputed =>
            

        /// <summary>
        /// Gets the estimate material in grams
        /// </summary>
        public virtual float MaterialGrams { get; set; }

        /// <summary>
        /// Gets the estimate material cost
        /// </summary>
        public virtual float MaterialCost { get; set; }

        /// <summary>
        /// Gets the material name
        /// </summary>
        public virtual string MaterialName { get; set; }

        /// <summary>
        /// Gets the machine name
        /// </summary>
        public virtual string MachineName { get; set; } = "Unknown";

        /// <summary>
        /// Gets the GCode, returns null if not supported
        /// </summary>
        public StringBuilder GCode { get; set; }

        /// <summary>
        /// Gets the GCode, returns null if not supported
        /// </summary>
        public string GCodeStr => GCode?.ToString();

        /// <summary>
        /// Gets if this file have available gcode
        /// </summary>
        public bool HaveGCode => GCode is not null;

        /// <summary>
        /// Get all configuration objects with properties and values
        /// </summary>
        public abstract object[] Configs { get; }

        /// <summary>
        /// Gets if this file is valid to read
        /// </summary>
        public bool IsValid => FileFullPath is not null;
        #endregion

        #region Constructor
        protected FileFormat()
        {
            Thumbnails = new Mat[ThumbnailsCount];
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SuppressRebuildProperties) return;
            if (
                e.PropertyName == nameof(BottomLayerCount) ||
                e.PropertyName == nameof(BottomExposureTime) ||
                e.PropertyName == nameof(ExposureTime) ||
                e.PropertyName == nameof(BottomLightOffDelay) ||
                e.PropertyName == nameof(LightOffDelay) ||
                e.PropertyName == nameof(BottomLiftHeight) ||
                e.PropertyName == nameof(LiftHeight) ||
                e.PropertyName == nameof(BottomLiftSpeed) ||
                e.PropertyName == nameof(LiftSpeed) ||
                e.PropertyName == nameof(RetractSpeed) ||
                e.PropertyName == nameof(BottomLightPWM) ||
                e.PropertyName == nameof(LightPWM)
            )
            {
                LayerManager.RebuildLayersProperties(false, e.PropertyName);
                if(e.PropertyName != nameof(BottomLightPWM) && e.PropertyName != nameof(LightPWM))
                    PrintTime = PrintTimeComputed;
                return;
            }
        }

        #endregion

        #region Indexers
        public Layer this[int index]
        {
            get => LayerManager[index];
            set => LayerManager[index] = value;
        }

        public Layer this[uint index]
        {
            get => LayerManager[index];
            set => LayerManager[index] = value;
        }

        public Layer this[long index]
        {
            get => LayerManager[index];
            set => LayerManager[index] = value;
        }
        #endregion

        #region Numerators
        public IEnumerator<Layer> GetEnumerator()
        {
            return ((IEnumerable<Layer>)LayerManager.Layers).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as FileFormat);
        }

        public bool Equals(FileFormat other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FileFullPath == other.FileFullPath;
        }

        public override int GetHashCode()
        {
            return (FileFullPath != null ? FileFullPath.GetHashCode() : 0);
        }

        public void Dispose()
        {
            Clear();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Clears all definitions and properties, it also dispose valid candidates 
        /// </summary>
        public virtual void Clear()
        {
            FileFullPath = null;
            LayerManager = null;
            GCode = null;

            if (Thumbnails is not null)
            {
                for (int i = 0; i < ThumbnailsCount; i++)
                {
                    Thumbnails[i]?.Dispose();
                }
            }
        }

        /// <summary>
        /// Validate if a file is a valid <see cref="FileFormat"/>
        /// </summary>
        /// <param name="fileFullPath">Full file path</param>
        public void FileValidation(string fileFullPath)
        {
            if (fileFullPath is null) throw new ArgumentNullException(nameof(FileFullPath), "fullFilePath can't be null.");
            if (!File.Exists(fileFullPath)) throw new FileNotFoundException("The specified file does not exists.", fileFullPath);

            if (IsExtensionValid(fileFullPath, true))
            {
                return;
            }

            throw new FileLoadException($"The specified file is not valid.", fileFullPath);
        }

        /// <summary>
        /// Checks if a extension is valid under the <see cref="FileFormat"/>
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <param name="isFilePath">True if <see cref="extension"/> is a full file path, otherwise false for extension only</param>
        /// <returns>True if valid, otherwise false</returns>
        public bool IsExtensionValid(string extension, bool isFilePath = false)
        {
            extension = isFilePath ? Path.GetExtension(extension)?.Remove(0, 1) : extension;
            return FileExtensions.Any(fileExtension => fileExtension.Equals(extension));
        }

        /// <summary>
        /// Gets all valid file extensions in a specified format
        /// </summary>
        public string GetFileExtensions(string prepend = ".", string separator = ", ")
        {
            var result = string.Empty;

            foreach (var fileExt in FileExtensions)
            {
                if (!ReferenceEquals(result, string.Empty))
                {
                    result += separator;
                }
                result += $"{prepend}{fileExt.Extension}";
            }

            return result;
        }

        /// <summary>
        /// Gets a thumbnail by it height or lower
        /// </summary>
        /// <param name="maxHeight">Max height allowed</param>
        /// <returns></returns>
        public Mat GetThumbnail(uint maxHeight = 400)
        {
            for (int i = 0; i < ThumbnailsCount; i++)
            {
                if(ReferenceEquals(Thumbnails[i], null)) continue;
                if (Thumbnails[i].Height <= maxHeight) return Thumbnails[i];
            }

            return null;
        }

        /// <summary>
        /// Gets a thumbnail by the largest or smallest
        /// </summary>
        /// <param name="largest">True to get the largest, otherwise false</param>
        /// <returns></returns>
        public Mat GetThumbnail(bool largest)
        {
            switch (CreatedThumbnailsCount)
            {
                case 0:
                    return null;
                case 1:
                    return Thumbnails[0];
                default:
                    if (largest)
                    {
                        return Thumbnails[0].Size.Area() >= Thumbnails[1].Size.Area() ? Thumbnails[0] : Thumbnails[1];
                    }
                    else
                    {
                        return Thumbnails[0].Size.Area() <= Thumbnails[1].Size.Area() ? Thumbnails[0] : Thumbnails[1];
                    }
            }
        }

        /// <summary>
        /// Sets thumbnails from a list of thumbnails and clone them
        /// </summary>
        /// <param name="images"></param>
        public void SetThumbnails(Mat[] images)
        {
            for (var i = 0; i < ThumbnailsCount; i++)
            {
                Thumbnails[i] = images[Math.Min(i, images.Length - 1)].Clone();
                if (Thumbnails[i].Size != ThumbnailsOriginalSize[i])
                {
                    CvInvoke.Resize(Thumbnails[i], Thumbnails[i], ThumbnailsOriginalSize[i]);
                }
            }
        }

        /// <summary>
        /// Sets all thumbnails the same image
        /// </summary>
        /// <param name="images">Image to set</param>
        public void SetThumbnails(Mat image)
        {
            for (var i = 0; i < ThumbnailsCount; i++)
            {
                Thumbnails[i] = image.Clone();
                if (ThumbnailsOriginalSize is null || i >= ThumbnailsOriginalSize.Length) continue;
                if (Thumbnails[i].Size != ThumbnailsOriginalSize[i])
                {
                    CvInvoke.Resize(Thumbnails[i], Thumbnails[i], ThumbnailsOriginalSize[i]);
                }
            }
        }

        /// <summary>
        /// Sets a thumbnail from a disk file
        /// </summary>
        /// <param name="index">Thumbnail index</param>
        /// <param name="filePath"></param>
        public void SetThumbnail(int index, string filePath)
        {
            Thumbnails[index] = CvInvoke.Imread(filePath, ImreadModes.AnyColor);
            if (Thumbnails[index].Size != ThumbnailsOriginalSize[index])
            {
                CvInvoke.Resize(Thumbnails[index], Thumbnails[index], ThumbnailsOriginalSize[index]);
            }
        }

        /// <summary>
        /// Encode to an output file
        /// </summary>
        /// <param name="fileFullPath">Output file</param>
        /// <param name="progress"></param>
        protected abstract void EncodeInternally(string fileFullPath, OperationProgress progress);

        /// <summary>
        /// Encode to an output file
        /// </summary>
        /// <param name="fileFullPath">Output file</param>
        /// <param name="progress"></param>
        public void Encode(string fileFullPath, OperationProgress progress = null)
        {
            progress ??= new OperationProgress();
            progress.Reset(OperationProgress.StatusEncodeLayers, LayerCount);

            FileFullPath = fileFullPath;

            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }

            for (var i = 0; i < Thumbnails.Length; i++)
            {
                if (Thumbnails[i] is null) continue;
                if(Thumbnails[i].Size == ThumbnailsOriginalSize[i]) continue;
                CvInvoke.Resize(Thumbnails[i], Thumbnails[i], new Size(ThumbnailsOriginalSize[i].Width, ThumbnailsOriginalSize[i].Height));
            }

            EncodeInternally(fileFullPath, progress);

            LayerManager.Desmodify();
            RequireFullEncode = false;
        }

        /// <summary>
        /// Decode a slicer file
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="progress"></param>
        protected abstract void DecodeInternally(string fileFullPath, OperationProgress progress);

        /// <summary>
        /// Decode a slicer file
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="progress"></param>
        public void Decode(string fileFullPath, OperationProgress progress = null)
        {
            Clear();
            FileValidation(fileFullPath);
            FileFullPath = fileFullPath;
            progress ??= new OperationProgress();
            progress.Reset(OperationProgress.StatusGatherLayers, LayerCount);

            DecodeInternally(fileFullPath, progress);

            progress.Token.ThrowIfCancellationRequested();

            // Sanitize
            for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                // Check for null layers
                if(this[layerIndex] is null) throw new FileLoadException($"Layer {layerIndex} was defined but doesn't contain a valid image.", fileFullPath);
                if(layerIndex <= 0) continue;
                // Check for bigger position z than it successor
                if(this[layerIndex-1].PositionZ > this[layerIndex].PositionZ) throw new FileLoadException($"Layer {layerIndex-1} ({this[layerIndex - 1].PositionZ}mm) have a higher Z position than the successor layer {layerIndex} ({this[layerIndex].PositionZ}mm).\n", fileFullPath);
            }

            // Fix 0mm positions at layer 0
            if(this[0].PositionZ == 0)
            {
                for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
                {
                    this[layerIndex].PositionZ += LayerHeight;
                }
                Save(progress);
            }
        }

        /// <summary>
        /// Extract contents to a folder
        /// </summary>
        /// <param name="path">Path to folder where content will be extracted</param>
        /// <param name="genericConfigExtract"></param>
        /// <param name="genericLayersExtract"></param>
        /// <param name="progress"></param>
        public virtual void Extract(string path, bool genericConfigExtract = true, bool genericLayersExtract = true,
            OperationProgress progress = null)
        {
            progress ??= new OperationProgress();
            progress.ItemName = OperationProgress.StatusExtracting;
                /*if (emptyFirst)
                {
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(path);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                }*/

            //if (!Directory.Exists(path))
            //{
            Directory.CreateDirectory(path);
            //}
            

            if (FileType == FileFormatType.Archive)
            {
                
                progress.CanCancel = false;
                //ZipFile.ExtractToDirectory(FileFullPath, path);
                ZipArchiveExtensions.ImprovedExtractToDirectory(FileFullPath, path, ZipArchiveExtensions.Overwrite.Always);
                return;
            }

            progress.ItemCount = LayerCount;

            if (genericConfigExtract)
            {
                if (!ReferenceEquals(Configs, null))
                {
                    using TextWriter tw = new StreamWriter(Path.Combine(path, $"{ExtractConfigFileName}.{ExtractConfigFileExtension}"), false);
                    foreach (var config in Configs)
                    {
                        var type = config.GetType();
                        tw.WriteLine($"[{type.Name}]");
                        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (property.Name.Equals("Item")) continue;
                            tw.WriteLine($"{property.Name} = {property.GetValue(config)}");
                        }

                        tw.WriteLine();
                    }

                    tw.Close();
                }
            }

            if (genericLayersExtract)
            {
                uint i = 0;
                if (!ReferenceEquals(Thumbnails, null))
                {
                    foreach (var thumbnail in Thumbnails)
                    {
                        if (ReferenceEquals(thumbnail, null))
                        {
                            continue;
                        }

                        thumbnail.Save(Path.Combine(path, $"Thumbnail{i}.png"));
                        i++;
                    }
                }

                if (LayerCount > 0)
                {
                    Parallel.ForEach(this, (layer) =>
                    {
                        if (progress.Token.IsCancellationRequested) return;
                        var byteArr = layer.CompressedBytes;
                        using (FileStream stream = File.Create(Path.Combine(path, layer.Filename),
                            byteArr.Length))
                        {
                            stream.Write(byteArr, 0, byteArr.Length);
                            stream.Close();
                            lock (progress.Mutex)
                            {
                                progress++;
                            }
                        }
                    });
                }

                /* Parallel.For(0, LayerCount, layerIndex => {
                         var byteArr = this[layerIndex].RawData;
                         using (FileStream stream = File.Create(Path.Combine(path, $"Layer{layerIndex}.png"), byteArr.Length))
                         {
                             stream.Write(byteArr, 0, byteArr.Length);
                             stream.Close();
                         }
                     });*/
                /*for (i = 0; i < LayerCount; i++)
                {
                    var byteArr = GetLayer(i);
                    using (FileStream stream = File.Create(Path.Combine(path, $"Layer{i}.png"), byteArr.Length))
                    {
                        stream.Write(byteArr, 0, byteArr.Length);
                        stream.Close();
                    }
                }*/
            }
        }

        /// <summary>
        /// Get height in mm from layer height
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="realHeight"></param>
        /// <returns>The height in mm</returns>
        public float GetHeightFromLayer(uint layerIndex, bool realHeight = true)
        {
            return (float)Math.Round((layerIndex+(realHeight ? 1 : 0)) * LayerHeight, 2);
        }

        /// <summary>
        /// Gets the value for initial layer or normal layers based on layer index
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="layerIndex">Layer index</param>
        /// <param name="initialLayerValue">Initial value</param>
        /// <param name="normalLayerValue">Normal value</param>
        /// <returns></returns>
        public T GetInitialLayerValueOrNormal<T>(uint layerIndex, T initialLayerValue, T normalLayerValue)
        {
            return layerIndex < BottomLayerCount ? initialLayerValue : normalLayerValue;
        }

        /// <summary>
        /// Refresh print parameters globals with this file settings
        /// </summary>
        public void RefreshPrintParametersModifiersValues()
        {
            if (PrintParameterModifiers is null) return;
            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomLayerCount))
            {
                PrintParameterModifier.BottomLayerCount.Value = BottomLayerCount;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomExposureSeconds))
            {
                PrintParameterModifier.BottomExposureSeconds.Value = (decimal) BottomExposureTime;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.ExposureSeconds))
            {
                PrintParameterModifier.ExposureSeconds.Value = (decimal)ExposureTime;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomLightOffDelay))
            {
                PrintParameterModifier.BottomLightOffDelay.Value = (decimal)BottomLightOffDelay;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.LightOffDelay))
            {
                PrintParameterModifier.LightOffDelay.Value = (decimal)LightOffDelay;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomLiftHeight))
            {
                PrintParameterModifier.BottomLiftHeight.Value = (decimal)BottomLiftHeight;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.LiftHeight))
            {
                PrintParameterModifier.LiftHeight.Value = (decimal)LiftHeight;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomLiftSpeed))
            {
                PrintParameterModifier.BottomLiftSpeed.Value = (decimal)BottomLiftSpeed;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.LiftSpeed))
            {
                PrintParameterModifier.LiftSpeed.Value = (decimal)LiftSpeed;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.RetractSpeed))
            {
                PrintParameterModifier.RetractSpeed.Value = (decimal)RetractSpeed;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.BottomLightPWM))
            {
                PrintParameterModifier.BottomLightPWM.Value = BottomLightPWM;
            }

            if (PrintParameterModifiers.Contains(PrintParameterModifier.LightPWM))
            {
                PrintParameterModifier.LightPWM.Value = LightPWM;
            }
        }

        /// <summary>
        /// Refresh print parameters per layer globals with this file settings
        /// </summary>
        public void RefreshPrintParametersPerLayerModifiersValues(uint layerIndex)
        {
            if (PrintParameterPerLayerModifiers is null) return;
            var layer = this[layerIndex];

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.ExposureSeconds))
            {
                PrintParameterModifier.ExposureSeconds.Value = (decimal)layer.ExposureTime;
            }

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.LightOffDelay))
            {
                PrintParameterModifier.LightOffDelay.Value = (decimal)layer.LightOffDelay;
            }

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.LiftHeight))
            {
                PrintParameterModifier.LiftHeight.Value = (decimal)layer.LiftHeight;
            }

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.LiftSpeed))
            {
                PrintParameterModifier.LiftSpeed.Value = (decimal)layer.LiftSpeed;
            }

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.RetractSpeed))
            {
                PrintParameterModifier.RetractSpeed.Value = (decimal)layer.RetractSpeed;
            }

            if (PrintParameterPerLayerModifiers.Contains(PrintParameterModifier.LightPWM))
            {
                PrintParameterModifier.LightPWM.Value = layer.LightPWM;
            }
        }

        /// <summary>
        /// Gets the value attributed to <see cref="FileFormat.PrintParameterModifier"/>
        /// </summary>
        /// <param name="modifier">Modifier to use</param>
        /// <returns>A value</returns>
        public object GetValueFromPrintParameterModifier(PrintParameterModifier modifier)
        {
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLayerCount))
                return BottomLayerCount;
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomExposureSeconds))
                return BottomExposureTime;
            if (ReferenceEquals(modifier, PrintParameterModifier.ExposureSeconds))
                return ExposureTime;

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLightOffDelay))
                return BottomLightOffDelay;
            if (ReferenceEquals(modifier, PrintParameterModifier.LightOffDelay))
                return LightOffDelay;

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLiftHeight))
                return BottomLiftHeight;
            if (ReferenceEquals(modifier, PrintParameterModifier.LiftHeight))
                return LiftHeight;
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLiftSpeed))
                return BottomLiftSpeed;
            if (ReferenceEquals(modifier, PrintParameterModifier.LiftSpeed))
                return LiftSpeed;
            if (ReferenceEquals(modifier, PrintParameterModifier.RetractSpeed))
                return RetractSpeed;

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLightPWM))
                return BottomLightPWM;
            if (ReferenceEquals(modifier, PrintParameterModifier.LightPWM))
                return LightPWM;

            return null;
        }

        /// <summary>
        /// Sets a property value attributed to <see cref="modifier"/>
        /// </summary>
        /// <param name="modifier">Modifier to use</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if set, otherwise false = <see cref="modifier"/> not found</returns>
        public bool SetValueFromPrintParameterModifier(PrintParameterModifier modifier, decimal value)
        {
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLayerCount))
            {
                BottomLayerCount = (ushort)value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomExposureSeconds))
            {
                BottomExposureTime = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.ExposureSeconds))
            {
                ExposureTime = (float) value;
                return true;
            }

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLightOffDelay))
            {
                BottomLightOffDelay = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.LightOffDelay))
            {
                LightOffDelay = (float) value;
                return true;
            }

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLiftHeight))
            {
                BottomLiftHeight = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.LiftHeight))
            {
                LiftHeight = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLiftSpeed))
            {
                BottomLiftSpeed = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.LiftSpeed))
            {
                LiftSpeed = (float) value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.RetractSpeed))
            {
                RetractSpeed = (float) value;
                return true;
            }

            if (ReferenceEquals(modifier, PrintParameterModifier.BottomLightPWM))
            {
                BottomLightPWM = (byte)value;
                return true;
            }
            if (ReferenceEquals(modifier, PrintParameterModifier.LightPWM))
            {
                LightPWM = (byte)value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets properties from print parameters
        /// </summary>
        /// <returns>Number of affected parameters</returns>
        public byte SetValuesFromPrintParametersModifiers()
        {
            if (PrintParameterModifiers is null) return 0;
            byte changed = 0;
            foreach (var modifier in PrintParameterModifiers)
            {
                if(!modifier.HasChanged) continue;
                modifier.OldValue = modifier.NewValue;
                SetValueFromPrintParameterModifier(modifier, modifier.NewValue);
                changed++;
            }

            return changed;
        }

        /// <summary>
        /// Rebuilds GCode based on current settings
        /// </summary>
        public virtual void RebuildGCode() { }

        /// <summary>
        /// Saves current configuration on input file
        /// </summary>
        /// <param name="progress"></param>
        public void Save(OperationProgress progress = null)
        {
            SaveAs(null, progress);
        }

        /// <summary>
        /// Saves current configuration on a copy
        /// </summary>
        /// <param name="filePath">File path to save copy as, use null to overwrite active file (Same as <see cref="Save"/>)</param>
        /// <param name="progress"></param>
        public abstract void SaveAs(string filePath = null, OperationProgress progress = null);

        /// <summary>
        /// Converts this file type to another file type
        /// </summary>
        /// <param name="to">Target file format</param>
        /// <param name="fileFullPath">Output path file</param>
        /// <param name="progress"></param>
        /// <returns>The converted file if successful, otherwise null</returns>
        public virtual FileFormat Convert(Type to, string fileFullPath, OperationProgress progress = null)
        {
            if (!IsValid) return null;
            var found = AvailableFormats.Any(format => to == format.GetType());
            if (!found) return null;

            progress ??= new OperationProgress("Converting");
            
            var slicerFile = (FileFormat)Activator.CreateInstance(to);
            if (slicerFile is null) return null;

            slicerFile.SuppressRebuildProperties = true;

            slicerFile.LayerManager = _layerManager.Clone();
            slicerFile.AntiAliasing = ValidateAntiAliasingLevel();
            slicerFile.LayerCount = _layerManager.Count;
            slicerFile.BottomLayerCount = BottomLayerCount;
            slicerFile.LayerHeight = LayerHeight;
            slicerFile.ResolutionX = ResolutionX;
            slicerFile.ResolutionY = ResolutionY;
            slicerFile.DisplayWidth = DisplayWidth;
            slicerFile.DisplayHeight = DisplayHeight;
            slicerFile.MaxPrintHeight = MaxPrintHeight;
            slicerFile.MirrorDisplay = MirrorDisplay;
            slicerFile.BottomExposureTime = BottomExposureTime;
            slicerFile.ExposureTime = ExposureTime;
            
            slicerFile.BottomLiftHeight = BottomLiftHeight;
            slicerFile.LiftHeight = LiftHeight;

            slicerFile.BottomLiftSpeed = BottomLiftSpeed;
            slicerFile.LiftSpeed = LiftSpeed;
            slicerFile.RetractSpeed = RetractSpeed;

            slicerFile.BottomLightOffDelay = BottomLightOffDelay;
            slicerFile.LightOffDelay = LightOffDelay;

            slicerFile.BottomLightPWM = BottomLightPWM;
            slicerFile.LightPWM = LightPWM;

            slicerFile.MachineName = MachineName;
            slicerFile.MaterialName = MaterialName;
            slicerFile.MaterialMilliliters = MaterialMilliliters;
            slicerFile.MaterialGrams = MaterialGrams;
            slicerFile.MaterialCost = MaterialCost;
            slicerFile.Xppmm = Xppmm;
            slicerFile.Yppmm = Yppmm;
            slicerFile.PrintTime = PrintTime;
            slicerFile.PrintHeight = PrintHeight;
            


            slicerFile.SuppressRebuildProperties = false;
            slicerFile.SetThumbnails(Thumbnails);
            slicerFile.Encode(fileFullPath, progress);

            return slicerFile;
        }

        /// <summary>
        /// Converts this file type to another file type
        /// </summary>
        /// <param name="to">Target file format</param>
        /// <param name="fileFullPath">Output path file</param>
        /// <param name="progress"></param>
        /// <returns>TThe converted file if successful, otherwise null</returns>
        public FileFormat Convert(FileFormat to, string fileFullPath, OperationProgress progress = null)
            => Convert(to.GetType(), fileFullPath, progress);

        /// <summary>
        /// Validate AntiAlias Level
        /// </summary>
        public byte ValidateAntiAliasingLevel()
        {
            if (AntiAliasing < 2) return 1;
            if(AntiAliasing % 2 != 0) throw new ArgumentException("AntiAliasing must be multiples of 2, otherwise use 0 or 1 to disable it", nameof(AntiAliasing));
            return AntiAliasing;
        }

        #endregion
    }
}
