﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

// https://github.com/cbiffle/catibo/blob/master/doc/cbddlp-ctb.adoc

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinarySerialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using UVtools.Core.Extensions;
using UVtools.Core.Operations;

namespace UVtools.Core.FileFormats
{
    public class ChituboxFile : FileFormat
    {

        #region Constants
        private const uint MAGIC_CBDDLP = 0x12FD0019; // 318570521
        private const uint MAGIC_CBT = 0x12FD0086; // 318570630
        private const ushort REPEATRGB15MASK = 0x20;

        private const byte RLE8EncodingLimit = 0x7d; // 125;
        private const ushort RLE16EncodingLimit = 0xFFF;

        private const uint ENCRYPTYION_MODE_CBDDLP = 0x8;  // 0 or 8
        private const uint ENCRYPTYION_MODE_CTBv2 = 0xF; // 15 for ctb v2 files
        private const uint ENCRYPTYION_MODE_CTBv3 = 0x2000000F; // 536870927 for ctb v3 files (This allow per layer settings, while 0xF don't)
        #endregion

        #region Sub Classes
        #region Header
        public class Header
        {

            /// <summary>
            /// Gets a magic number identifying the file type.
            /// 0x12fd_0019 for cbddlp
            /// 0x12fd_0086 for ctb
            /// </summary>
            [FieldOrder(0)]  public uint Magic     { get; set; }

            /// <summary>
            /// Gets the software version
            /// </summary>
            [FieldOrder(1)] public uint Version { get; set; } = 3;

            /// <summary>
            /// Gets dimensions of the printer’s X output volume, in millimeters.
            /// </summary>
            [FieldOrder(2)]  public float BedSizeX { get; set; }

            /// <summary>
            /// Gets dimensions of the printer’s Y output volume, in millimeters.
            /// </summary>
            [FieldOrder(3)]  public float BedSizeY { get; set; }

            /// <summary>
            /// Gets dimensions of the printer’s Z output volume, in millimeters.
            /// </summary>
            [FieldOrder(4)]  public float BedSizeZ { get; set; }

            [FieldOrder(5)]  public uint Unknown1  { get; set; }
            [FieldOrder(6)]  public uint Unknown2  { get; set; }

            /// <summary>
            /// Gets the height of the model described by this file, in millimeters.
            /// </summary>
            [FieldOrder(7)]  public float TotalHeightMilimeter { get; set; }

            /// <summary>
            /// Gets the layer height setting used at slicing, in millimeters. Actual height used by the machine is in the layer table.
            /// </summary>
            [FieldOrder(8)]  public float LayerHeightMilimeter  { get; set; }

            /// <summary>
            /// Gets the exposure time setting used at slicing, in seconds, for normal (non-bottom) layers, respectively. Actual time used by the machine is in the layer table.
            /// </summary>
            [FieldOrder(9)]  public float LayerExposureSeconds  { get; set; }

            /// <summary>
            /// Gets the exposure time setting used at slicing, in seconds, for bottom layers. Actual time used by the machine is in the layer table.
            /// </summary>
            [FieldOrder(10)] public float BottomExposureSeconds { get; set; }

            /// <summary>
            /// Gets the light off time setting used at slicing, for normal layers, in seconds. Actual time used by the machine is in the layer table. Note that light_off_time_s appears in both the file header and ExtConfig.
            /// </summary>
            [FieldOrder(11)] public float LightOffDelay     { get; set; } = 1;

            /// <summary>
            /// Gets number of layers configured as "bottom." Note that this field appears in both the file header and ExtConfig..
            /// </summary>
            [FieldOrder(12)] public uint BottomLayersCount { get; set; } = 10;

            /// <summary>
            /// Gets the printer resolution along X axis, in pixels. This information is critical to correctly decoding layer images.
            /// </summary>
            [FieldOrder(13)] public uint ResolutionX       { get; set; }

            /// <summary>
            /// Gets the printer resolution along Y axis, in pixels. This information is critical to correctly decoding layer images.
            /// </summary>
            [FieldOrder(14)] public uint ResolutionY       { get; set; }

            /// <summary>
            /// Gets the file offsets of ImageHeader records describing the larger preview images.
            /// </summary>
            [FieldOrder(15)] public uint PreviewLargeOffsetAddress { get; set; }

            /// <summary>
            /// Gets the file offset of a table of LayerHeader records giving parameters for each printed layer.
            /// </summary>
            [FieldOrder(16)] public uint LayersDefinitionOffsetAddress { get; set; }

            /// <summary>
            /// Gets the number of records in the layer table for the first level set. In ctb files, that’s equivalent to the total number of records, but records may be multiplied in antialiased cbddlp files.
            /// </summary>
            [FieldOrder(17)] public uint LayerCount { get; set; }

            /// <summary>
            /// Gets the file offsets of ImageHeader records describing the smaller preview images.
            /// </summary>
            [FieldOrder(18)] public uint PreviewSmallOffsetAddress { get; set; }

            /// <summary>
            /// Gets the estimated duration of print, in seconds.
            /// </summary>
            [FieldOrder(19)] public uint PrintTime { get; set; }

            /// <summary>
            /// Gets the records whether this file was generated assuming normal (0) or mirrored (1) image projection. LCD printers are "mirrored" for this purpose.
            /// </summary>
            [FieldOrder(20)] public uint ProjectorType { get; set; }

            /// <summary>
            /// Gets the print parameters table offset
            /// </summary>
            [FieldOrder(21)] public uint PrintParametersOffsetAddress { get; set; }

            /// <summary>
            /// Gets the print parameters table size in bytes.
            /// </summary>
            [FieldOrder(22)] public uint PrintParametersSize { get; set; }

            /// <summary>
            /// Gets the number of times each layer image is repeated in the file.
            /// This is used to implement antialiasing in cbddlp files. When greater than 1,
            /// the layer table will actually contain layer_table_count * level_set_count entries.
            /// See the section on antialiasing for details.
            /// </summary>
            [FieldOrder(23)] public uint AntiAliasLevel { get; set; } = 1;

            /// <summary>
            /// Gets the PWM duty cycle for the UV illumination source on normal levels, respectively.
            /// This appears to be an 8-bit quantity where 0xFF is fully on and 0x00 is fully off.
            /// </summary>
            [FieldOrder(24)] public ushort LightPWM { get; set; } = 255;

            /// <summary>
            /// Gets the PWM duty cycle for the UV illumination source on bottom levels, respectively.
            /// This appears to be an 8-bit quantity where 0xFF is fully on and 0x00 is fully off.
            /// </summary>
            [FieldOrder(25)] public ushort BottomLightPWM { get; set; } = 255;

            /// <summary>
            /// Gets the key used to encrypt layer data, or 0 if encryption is not used.
            /// </summary>
            [FieldOrder(26)] public uint EncryptionKey { get; set; }

            /// <summary>
            /// Gets the slicer tablet offset 
            /// </summary>
            [FieldOrder(27)] public uint SlicerOffset { get; set; }

            /// <summary>
            /// Gets the slicer table size in bytes
            /// </summary>
            [FieldOrder(28)] public uint SlicerSize { get; set; }

            public override string ToString()
            {
                return $"{nameof(Magic)}: {Magic}, {nameof(Version)}: {Version}, {nameof(BedSizeX)}: {BedSizeX}, {nameof(BedSizeY)}: {BedSizeY}, {nameof(BedSizeZ)}: {BedSizeZ}, {nameof(Unknown1)}: {Unknown1}, {nameof(Unknown2)}: {Unknown2}, {nameof(TotalHeightMilimeter)}: {TotalHeightMilimeter}, {nameof(LayerHeightMilimeter)}: {LayerHeightMilimeter}, {nameof(LayerExposureSeconds)}: {LayerExposureSeconds}, {nameof(BottomExposureSeconds)}: {BottomExposureSeconds}, {nameof(LightOffDelay)}: {LightOffDelay}, {nameof(BottomLayersCount)}: {BottomLayersCount}, {nameof(ResolutionX)}: {ResolutionX}, {nameof(ResolutionY)}: {ResolutionY}, {nameof(PreviewLargeOffsetAddress)}: {PreviewLargeOffsetAddress}, {nameof(LayersDefinitionOffsetAddress)}: {LayersDefinitionOffsetAddress}, {nameof(LayerCount)}: {LayerCount}, {nameof(PreviewSmallOffsetAddress)}: {PreviewSmallOffsetAddress}, {nameof(PrintTime)}: {PrintTime}, {nameof(ProjectorType)}: {ProjectorType}, {nameof(PrintParametersOffsetAddress)}: {PrintParametersOffsetAddress}, {nameof(PrintParametersSize)}: {PrintParametersSize}, {nameof(AntiAliasLevel)}: {AntiAliasLevel}, {nameof(LightPWM)}: {LightPWM}, {nameof(BottomLightPWM)}: {BottomLightPWM}, {nameof(EncryptionKey)}: {EncryptionKey}, {nameof(SlicerOffset)}: {SlicerOffset}, {nameof(SlicerSize)}: {SlicerSize}";
            }
        }
        #endregion

        #region PrintParameters
        public class PrintParameters
        {
            /// <summary>
            /// Gets the distance to lift the build platform away from the vat after bottom layers, in millimeters.
            /// </summary>
            [FieldOrder(0)] public float BottomLiftHeight { get; set; } = 5;

            /// <summary>
            /// Gets the speed at which to lift the build platform away from the vat after bottom layers, in millimeters per minute.
            /// </summary>
            [FieldOrder(1)]  public float BottomLiftSpeed     { get; set; } = 300;

            /// <summary>
            /// Gets the distance to lift the build platform away from the vat after normal layers, in millimeters.
            /// </summary>
            [FieldOrder(2)]  public float LiftHeight          { get; set; } = 5;

            /// <summary>
            /// Gets the speed at which to lift the build platform away from the vat after normal layers, in millimeters per minute.
            /// </summary>
            [FieldOrder(3)]  public float LiftSpeed        { get; set; } = 300;

            /// <summary>
            /// Gets the speed to use when the build platform re-approaches the vat after lift, in millimeters per minute.
            /// </summary>
            [FieldOrder(4)]  public float RetractSpeed        { get; set; } = 300;

            /// <summary>
            /// Gets the estimated required resin, measured in milliliters. The volume number is derived from the model.
            /// </summary>
            [FieldOrder(5)]  public float VolumeMl            { get; set; }

            /// <summary>
            /// Gets the estimated grams, derived from volume using configured factors for density.
            /// </summary>
            [FieldOrder(6)]  public float WeightG             { get; set; }

            /// <summary>
            /// Gets the estimated cost based on currency unit the user had configured. Derived from volume using configured factors for density and cost.
            /// </summary>
            [FieldOrder(7)]  public float CostDollars         { get; set; }

            /// <summary>
            /// Gets the light off time setting used at slicing, for bottom layers, in seconds. Actual time used by the machine is in the layer table. Note that light_off_time_s appears in both the file header and ExtConfig.
            /// </summary>
            [FieldOrder(8)]  public float BottomLightOffDelay { get; set; } = 1;

            /// <summary>
            /// Gets the light off time setting used at slicing, for normal layers, in seconds. Actual time used by the machine is in the layer table. Note that light_off_time_s appears in both the file header and ExtConfig.
            /// </summary>
            [FieldOrder(9)]  public float LightOffDelay       { get; set; } = 1;

            /// <summary>
            /// Gets number of layers configured as "bottom." Note that this field appears in both the file header and ExtConfig.
            /// </summary>
            [FieldOrder(10)] public uint BottomLayerCount     { get; set; } = 10;
            [FieldOrder(11)] public uint Padding1             { get; set; }
            [FieldOrder(12)] public uint Padding2             { get; set; }
            [FieldOrder(13)] public uint Padding3             { get; set; }
            [FieldOrder(14)] public uint Padding4             { get; set; }

            public override string ToString()
            {
                return $"{nameof(BottomLiftHeight)}: {BottomLiftHeight}, {nameof(BottomLiftSpeed)}: {BottomLiftSpeed}, {nameof(LiftHeight)}: {LiftHeight}, {nameof(LiftSpeed)}: {LiftSpeed}, {nameof(RetractSpeed)}: {RetractSpeed}, {nameof(VolumeMl)}: {VolumeMl}, {nameof(WeightG)}: {WeightG}, {nameof(CostDollars)}: {CostDollars}, {nameof(BottomLightOffDelay)}: {BottomLightOffDelay}, {nameof(LightOffDelay)}: {LightOffDelay}, {nameof(BottomLayerCount)}: {BottomLayerCount}, {nameof(Padding1)}: {Padding1}, {nameof(Padding2)}: {Padding2}, {nameof(Padding3)}: {Padding3}, {nameof(Padding4)}: {Padding4}";
            }
        }
        #endregion

        #region SlicerInfo

        public class SlicerInfo
        {
            private string _machineName;
            [FieldOrder(0)] public uint Padding1           { get; set; }
            [FieldOrder(1)] public uint Padding2           { get; set; }
            [FieldOrder(2)] public uint Padding3           { get; set; }
            [FieldOrder(3)] public uint Padding4           { get; set; }
            [FieldOrder(4)] public uint Padding5           { get; set; }
            [FieldOrder(5)] public uint Padding6           { get; set; }
            [FieldOrder(6)] public uint Padding7           { get; set; }

            /// <summary>
            /// Gets the machine name offset to a string naming the machine type, and its length in bytes.
            /// </summary>
            [FieldOrder(7)] public uint MachineNameAddress { get; set; }

            /// <summary>
            /// Gets the machine size in bytes
            /// </summary>
            [FieldOrder(8)] public uint MachineNameSize    { get; set; }

            /// <summary>
            /// Gets the parameter used to control encryption.
            /// Not totally understood. 0/8 for cbddlp files, 0xF (15) for ctb files, 0x2000000F (536870927) for v3 ctb files allow per layer parameters
            /// </summary>
            [FieldOrder(9)] public uint EncryptionMode     { get; set; } = ENCRYPTYION_MODE_CTBv3;

            /// <summary>
            /// Gets a number that increments with time or number of models sliced, or both. Zeroing it in output seems to have no effect. Possibly a user tracking bug.
            /// </summary>
            [FieldOrder(10)] public uint MysteriousId      { get; set; }

            /// <summary>
            /// Gets the user-selected antialiasing level. For cbddlp files this will match the level_set_count. For ctb files, this number is essentially arbitrary.
            /// </summary>
            [FieldOrder(11)] public uint AntiAliasLevel { get; set; } = 1;

            /// <summary>
            /// Gets a version of software that generated this file, encoded with major, minor, and patch release in bytes starting from the MSB down.
            /// (No provision is made to name the software being used, so this assumes that only one software package can generate the files.
            /// Probably best to hardcode it at 0x01060300.)
            /// </summary>
            [FieldOrder(12)] public uint SoftwareVersion { get; set; } = 0x01060300;
            [FieldOrder(13)] public uint Unknown1          { get; set; }
            [FieldOrder(14)] public uint Padding8          { get; set; }
            [FieldOrder(15)] public uint TransitionLayerCount { get; set; } // CTB not all printers
            [FieldOrder(16)] public uint Padding10         { get; set; }
            [FieldOrder(17)] public uint Padding11         { get; set; }
            [FieldOrder(18)] public uint Padding12         { get; set; }

            /// <summary>
            /// Gets the machine name. string is not nul-terminated.
            /// The character encoding is currently unknown — all observed files in the wild use 7-bit ASCII characters only.
            /// Note that the machine type here is set in the software profile, and is not the name the user assigned to the machine.
            /// </summary>
            [FieldOrder(19)]
            [FieldLength(nameof(MachineNameSize))]
            public string MachineName
            {
                get => _machineName;
                set
                {
                    _machineName = value;
                    MachineNameSize = string.IsNullOrEmpty(_machineName) ? 0 : (uint)_machineName.Length;
                }
                
            }

            public override string ToString()
            {
                return $"{nameof(Padding1)}: {Padding1}, {nameof(Padding2)}: {Padding2}, {nameof(Padding3)}: {Padding3}, {nameof(Padding4)}: {Padding4}, {nameof(Padding5)}: {Padding5}, {nameof(Padding6)}: {Padding6}, {nameof(Padding7)}: {Padding7}, {nameof(MachineNameAddress)}: {MachineNameAddress}, {nameof(MachineNameSize)}: {MachineNameSize}, {nameof(EncryptionMode)}: {EncryptionMode}, {nameof(MysteriousId)}: {MysteriousId}, {nameof(AntiAliasLevel)}: {AntiAliasLevel}, {nameof(SoftwareVersion)}: {SoftwareVersion}, {nameof(Unknown1)}: {Unknown1}, {nameof(Padding8)}: {Padding8}, {nameof(TransitionLayerCount)}: {TransitionLayerCount}, {nameof(Padding10)}: {Padding10}, {nameof(Padding11)}: {Padding11}, {nameof(Padding12)}: {Padding12}, {nameof(MachineName)}: {MachineName}";
            }
        }

        #endregion

        #region Preview
        /// <summary>
        /// The files contain two preview images.
        /// These are shown on the printer display when choosing which file to print, sparing the poor printer from needing to render a 3D image from scratch.
        /// </summary>
        public class Preview
        {
            /// <summary>
            /// Gets the X dimension of the preview image, in pixels. 
            /// </summary>
            [FieldOrder(0)] public uint ResolutionX { get; set; }

            /// <summary>
            /// Gets the Y dimension of the preview image, in pixels. 
            /// </summary>
            [FieldOrder(1)] public uint ResolutionY { get; set; }

            /// <summary>
            /// Gets the image offset of the encoded data blob.
            /// </summary>
            [FieldOrder(2)] public uint ImageOffset { get; set; }

            /// <summary>
            /// Gets the image length in bytes.
            /// </summary>
            [FieldOrder(3)] public uint ImageLength { get; set; }

            [FieldOrder(4)] public uint Unknown1    { get; set; }
            [FieldOrder(5)] public uint Unknown2    { get; set; }
            [FieldOrder(6)] public uint Unknown3    { get; set; }
            [FieldOrder(7)] public uint Unknown4    { get; set; }

            public unsafe Mat Decode(byte[] rawImageData)
            {
                var image = new Mat(new Size((int) ResolutionX, (int) ResolutionY), DepthType.Cv8U, 3);
                var span = image.GetBytePointer();

                int pixel = 0;
                for (int n = 0; n < rawImageData.Length; n++)
                {
                    uint dot = (uint)(rawImageData[n] & 0xFF | ((rawImageData[++n] & 0xFF) << 8));
                    //uint color = ((dot & 0xF800) << 8) | ((dot & 0x07C0) << 5) | ((dot & 0x001F) << 3);
                    byte red = (byte)(((dot >> 11) & 0x1F) << 3);
                    byte green = (byte)(((dot >> 6) & 0x1F) << 3);
                    byte blue = (byte)((dot & 0x1F) << 3);
                    int repeat = 1;
                    if ((dot & 0x0020) == 0x0020)
                    {
                        repeat += rawImageData[++n] & 0xFF | ((rawImageData[++n] & 0x0F) << 8);
                    }

                    for (int j = 0; j < repeat; j++)
                    {
                        span[pixel++] = blue;
                        span[pixel++] = green;
                        span[pixel++] = red;
                        //span[pixel++] = new Rgba32(red, green, blue);
                    }
                }

                return image;
            }

            public override string ToString()
            {
                return $"{nameof(ResolutionX)}: {ResolutionX}, {nameof(ResolutionY)}: {ResolutionY}, {nameof(ImageOffset)}: {ImageOffset}, {nameof(ImageLength)}: {ImageLength}, {nameof(Unknown1)}: {Unknown1}, {nameof(Unknown2)}: {Unknown2}, {nameof(Unknown3)}: {Unknown3}, {nameof(Unknown4)}: {Unknown4}";
            }

            public unsafe byte[] Encode(Mat image)
            {
                List<byte> rawData = new List<byte>();
                ushort color15 = 0;
                uint rep = 0;

                var span = image.GetBytePointer();
                var imageLength = image.GetLength();

                void RleRGB15()
                {
                    switch (rep)
                    {
                        case 0:
                            return;
                        case 1:
                            rawData.Add((byte)(color15 & ~REPEATRGB15MASK));
                            rawData.Add((byte)((color15 & ~REPEATRGB15MASK) >> 8));
                            break;
                        case 2:
                            for (int i = 0; i < 2; i++)
                            {
                                rawData.Add((byte)(color15 & ~REPEATRGB15MASK));
                                rawData.Add((byte)((color15 & ~REPEATRGB15MASK) >> 8));
                            }

                            break;
                        default:
                            rawData.Add((byte)(color15 | REPEATRGB15MASK));
                            rawData.Add((byte)((color15 | REPEATRGB15MASK) >> 8));
                            rawData.Add((byte)((rep - 1) | 0x3000));
                            rawData.Add((byte)(((rep - 1) | 0x3000) >> 8));
                            break;
                    }
                }

                int pixel = 0;
                while (pixel < imageLength)
                {
                    var ncolor15 =
                        // bgr
                        (span[pixel++] >> 3) | ((span[pixel++] >> 2) << 5) | ((span[pixel++] >> 3) << 11);

                    if (ncolor15 == color15)
                    {
                        rep++;
                        if (rep == RLE16EncodingLimit)
                        {
                            RleRGB15();
                            rep = 0;
                        }
                    }
                    else
                    {
                        RleRGB15();
                        color15 = (ushort) ncolor15;
                        rep = 1;
                    }
                }

                RleRGB15();

                ImageLength = (uint) rawData.Count;

                return rawData.ToArray();
            }
        }

        #endregion

        #region Layer
        public class LayerData
        {
            /// <summary>
            /// Gets the build platform Z position for this layer, measured in millimeters.
            /// </summary>
            [FieldOrder(0)] public float LayerPositionZ      { get; set; }

            /// <summary>
            /// Gets the exposure time for this layer, in seconds.
            /// </summary>
            [FieldOrder(1)] public float LayerExposure       { get; set; }

            /// <summary>
            /// Gets how long to keep the light off after exposing this layer, in seconds.
            /// </summary>
            [FieldOrder(2)] public float LightOffSeconds { get; set; }

            /// <summary>
            /// Gets the layer image offset to encoded layer data, and its length in bytes.
            /// </summary>
            [FieldOrder(3)] public uint DataAddress          { get; set; }

            /// <summary>
            /// Gets the layer image length in bytes.
            /// </summary>
            [FieldOrder(4)] public uint DataSize             { get; set; }
            [FieldOrder(5)] public uint Unknown1             { get; set; }
            [FieldOrder(6)] public uint Unknown2             { get; set; }// = 84; // Spoted on Mars 2 Pro
            [FieldOrder(7)] public uint Unknown3             { get; set; }
            [FieldOrder(8)] public uint Unknown4             { get; set; }


            [Ignore] public byte[] EncodedRle { get; set; }
            [Ignore] public ChituboxFile Parent { get; set; }

            [Ignore] public uint Version { get; set; } = 2;

            public LayerData()
            {
            }

            public LayerData(ChituboxFile parent, uint layerIndex)
            {
                Parent = parent;
                RefreshLayerData(parent, layerIndex);

                if (parent.HeaderSettings.Version >= 3 && Unknown2 == 0)
                {
                    Unknown2 = 84;
                }
            }

            public void RefreshLayerData(ChituboxFile parent, uint layerIndex)
            {
                LayerPositionZ = parent[layerIndex].PositionZ;
                LayerExposure = parent[layerIndex].ExposureTime;
                LightOffSeconds = parent[layerIndex].LightOffDelay;
            }


            public Mat Decode(uint layerIndex, bool consumeData = true)
            {
                var image = Parent.IsCbtFile ? DecodeCbtImage(layerIndex) : DecodeCbddlpImage(Parent, layerIndex);

                if (consumeData)
                    EncodedRle = null;

                return image;
            }

            public static unsafe Mat DecodeCbddlpImage(ChituboxFile parent, uint layerIndex)
            {
                var image = EmguExtensions.InitMat(parent.Resolution);
                var span = image.GetBytePointer();
                var imageLength = image.GetLength();

                for (byte bit = 0; bit < parent.AntiAliasing; bit++)
                {
                    var layer = parent.LayerDefinitions[bit, layerIndex];

                    int n = 0;
                    for (int index = 0; index < layer.DataSize; index++)
                    {
                        // Lower 7 bits is the repeat count for the bit (0..127)
                        int reps = layer.EncodedRle[index] & 0x7f;

                        // We only need to set the non-zero pixels
                        // High bit is on for white, off for black
                        if ((layer.EncodedRle[index] & 0x80) != 0)
                        {
                            for (int i = 0; i < reps; i++)
                            {
                                span[n + i]++;
                            }
                        }

                        n += reps;

                        if (n == imageLength)
                        {
                            break;
                        }

                        if (n > imageLength)
                        {
                            image.Dispose();
                            throw new FileLoadException("Error image ran off the end");
                        }
                    }
                }

                for (int i = 0; i < imageLength; i++)
                {
                    int newC = span[i] * (256 / parent.AntiAliasing);

                    if (newC > 0)
                    {
                        newC--;
                    }

                    span[i] = (byte) newC;


                }

                return image;
            }

            private unsafe Mat DecodeCbtImage(uint layerIndex)
            {
                var image = EmguExtensions.InitMat(Parent.Resolution);
                var span = image.GetBytePointer();

                if (Parent.HeaderSettings.EncryptionKey > 0)
                {
                    KeyRing kr = new KeyRing(Parent.HeaderSettings.EncryptionKey, layerIndex);
                    EncodedRle = kr.Read(EncodedRle);
                }

                int pixel = 0;
                for (var n = 0; n < EncodedRle.Length; n++)
                {
                    byte code = EncodedRle[n];
                    int stride = 1;

                    if ((code & 0x80) == 0x80) // It's a run
                    {
                        code &= 0x7f; // Get the run length
                        n++;

                        var slen = EncodedRle[n];

                        if ((slen & 0x80) == 0)
                        {
                            stride = slen;
                        }
                        else if ((slen & 0xc0) == 0x80)
                        {
                            stride = ((slen & 0x3f) << 8) + EncodedRle[n + 1];
                            n++;
                        }
                        else if ((slen & 0xe0) == 0xc0)
                        {
                            stride = ((slen & 0x1f) << 16) + (EncodedRle[n + 1] << 8) + EncodedRle[n + 2];
                            n += 2;
                        }
                        else if ((slen & 0xf0) == 0xe0)
                        {
                            stride = ((slen & 0xf) << 24) + (EncodedRle[n + 1] << 16) + (EncodedRle[n + 2] << 8) + EncodedRle[n + 3];
                            n += 3;
                        }
                        else
                        {
                            image.Dispose();
                            throw new FileLoadException("Corrupted RLE data");
                        }
                    }

                    // Bit extend from 7-bit to 8-bit greymap
                    if (code != 0)
                    {
                        code = (byte)((code << 1) | 1);
                    }

                    if (stride == 0) continue; // Nothing to do

                    if (code == 0) // Ignore blacks, spare cycles
                    {
                        pixel += stride;
                        continue;
                    }

                    for (; stride > 0; stride--)
                    {
                        span[pixel] = code;
                        pixel++;
                    }
                }

                return image;
            }

            public byte[] Encode(Mat image, byte aaIndex, uint layerIndex)
            {
                return Parent.IsCbtFile ? EncodeCbtImage(image, layerIndex) : EncodeCbddlpImage(image, aaIndex);
            }

            public unsafe byte[] EncodeCbddlpImage(Mat image, byte bit)
            {
                List<byte> rawData = new List<byte>();
                var span = image.GetBytePointer();
                var imageLength = image.GetLength();

                bool obit = false;
                int rep = 0;

                //ngrey:= uint16(r | g | b)
                // thresholds:
                // aa 1:  127
                // aa 2:  255 127
                // aa 4:  255 191 127 63
                // aa 8:  255 223 191 159 127 95 63 31
                byte threshold = (byte)(256 / Parent.AntiAliasing * bit - 1);

                void AddRep()
                {
                    if (rep <= 0) return;

                    byte by = (byte)rep;

                    if (obit)
                    {
                        by |= 0x80;
                        //bitsOn += uint(rep)
                    }

                    rawData.Add(by);
                }

                for (int pixel = 0; pixel < imageLength; pixel++)
                {
                    var nbit = span[pixel] >= threshold;

                    if (nbit == obit)
                    {
                        rep++;

                        if (rep == RLE8EncodingLimit)
                        {
                            AddRep();
                            rep = 0;
                        }
                    }
                    else
                    {
                        AddRep();
                        obit = nbit;
                        rep = 1;
                    }
                }

                // Collect stragglers
                AddRep();

                EncodedRle = rawData.ToArray();
                DataSize = (uint) EncodedRle.Length;

                return EncodedRle;
            }

            private unsafe byte[] EncodeCbtImage(Mat image, uint layerIndex)
            {
                List<byte> rawData = new List<byte>();
                byte color = byte.MaxValue >> 1;
                uint stride = 0;
                var span = image.GetBytePointer();
                var imageLength = image.GetLength();

                void AddRep()
                {
                    if (stride == 0)
                    {
                        return;
                    }

                    if (stride > 1)
                    {
                        color |= 0x80;
                    }
                    rawData.Add(color);

                    if (stride <= 1)
                    {
                        // no run needed
                        return;
                    }

                    if (stride <= 0x7f)
                    {
                        rawData.Add((byte)stride);
                        return;
                    }

                    if (stride <= 0x3fff)
                    {
                        rawData.Add((byte)((stride >> 8) | 0x80));
                        rawData.Add((byte)stride);
                        return;
                    }

                    if (stride <= 0x1fffff)
                    {
                        rawData.Add((byte)((stride >> 16) | 0xc0));
                        rawData.Add((byte)(stride >> 8));
                        rawData.Add((byte)stride);
                        return;
                    }

                    if (stride <= 0xfffffff)
                    {
                        rawData.Add((byte)((stride >> 24) | 0xe0));
                        rawData.Add((byte)(stride >> 16));
                        rawData.Add((byte)(stride >> 8));
                        rawData.Add((byte)stride);
                    }

                }


                for (int pixel = 0; pixel < imageLength; pixel++)
                {
                    var grey7 = (byte) (span[pixel] >> 1);

                    if (grey7 == color)
                    {
                        stride++;
                    }
                    else
                    {
                        AddRep();
                        color = grey7;
                        stride = 1;
                    }
                }

                AddRep();

                if (Parent.HeaderSettings.EncryptionKey > 0)
                {
                    KeyRing kr = new KeyRing(Parent.HeaderSettings.EncryptionKey, layerIndex);
                    EncodedRle = kr.Read(rawData.ToArray());
                }
                else
                {
                    EncodedRle = rawData.ToArray();
                }

                DataSize = (uint)EncodedRle.Length;

                return EncodedRle;
            }

            public override string ToString()
            {
                return $"{nameof(LayerPositionZ)}: {LayerPositionZ}, {nameof(LayerExposure)}: {LayerExposure}, {nameof(LightOffSeconds)}: {LightOffSeconds}, {nameof(DataAddress)}: {DataAddress}, {nameof(DataSize)}: {DataSize}, {nameof(Unknown1)}: {Unknown1}, {nameof(Unknown2)}: {Unknown2}, {nameof(Unknown3)}: {Unknown3}, {nameof(Unknown4)}: {Unknown4}";
            }


        }

        public class LayerDataEx
        {
            /// <summary>
            /// Gets a copy of layer data defenition
            /// </summary>
            [FieldOrder(0)] public LayerData LayerData { get; set; } = new LayerData();

            /// <summary>
            /// Gets the total size of ctbImageInfo and Image data
            /// </summary>
            [FieldOrder(1)] public uint TotalSize { get; set; }
            [FieldOrder(2)] public float LiftHeight { get; set; }
            [FieldOrder(3)] public float LiftSpeed { get; set; }
            [FieldOrder(4)] public uint Unknown6 { get; set; }
            [FieldOrder(5)] public uint Unknown7 { get; set; }
            [FieldOrder(6)] public float RetractSpeed { get; set; }
            [FieldOrder(7)] public uint Unknown8 { get; set; }
            [FieldOrder(8)] public uint Unknown9 { get; set; }
            [FieldOrder(9)] public uint Unknown10 { get; set; }
            [FieldOrder(10)] public uint Unknown11 { get; set; }
            [FieldOrder(11)] public uint Unknown12 { get; set; } = 28672; // 28672 v3?
            [FieldOrder(12)] public float LightPWM { get; set; }

            public LayerDataEx()
            {
            }

            public LayerDataEx(LayerData layerData, uint layerIndex)
            {
                LayerData = layerData;
                if (layerData.Parent is not null)
                {
                    LiftHeight = layerData.Parent[layerIndex].LiftHeight;
                    LiftSpeed = layerData.Parent[layerIndex].LiftSpeed;
                    RetractSpeed = layerData.Parent[layerIndex].RetractSpeed;
                    LightPWM = layerData.Parent[layerIndex].LightPWM;
                }

                if (layerData.DataSize > 0)
                {
                    TotalSize = (uint) (Helpers.Serializer.SizeOf(this) + layerData.DataSize);
                }
            }

            public override string ToString()
            {
                return $"{nameof(LayerData)}: {LayerData}, {nameof(TotalSize)}: {TotalSize}, {nameof(LiftHeight)}: {LiftHeight}, {nameof(LiftSpeed)}: {LiftSpeed}, {nameof(Unknown6)}: {Unknown6}, {nameof(Unknown7)}: {Unknown7}, {nameof(RetractSpeed)}: {RetractSpeed}, {nameof(Unknown8)}: {Unknown8}, {nameof(Unknown9)}: {Unknown9}, {nameof(Unknown10)}: {Unknown10}, {nameof(Unknown11)}: {Unknown11}, {nameof(Unknown12)}: {Unknown12}, {nameof(LightPWM)}: {LightPWM}";
            }
        }

        #endregion

            #region KeyRing

            public class KeyRing
        {
            public uint Init { get; }
            public uint Key { get; private set; }
            public uint Index { get; private set; }

            public KeyRing(uint seed, uint layerIndex)
            {
                Init = seed * 0x2d83cdac + 0xd8a83423;
                Key = (layerIndex * 0x1e1530cd + 0xec3d47cd) * Init;
            }

            public byte Next()
            {
                byte k = (byte)(Key >> (int)(8 * Index));

                Index++;

                if ((Index & 3) == 0)
                {
                    Key += Init;
                    Index = 0;
                }

                return k;
            }

            public List<byte> Read(List<byte> input)
            {
                List<byte> data = new List<byte>(input.Count);
                data.AddRange(input.Select(t => (byte) (t ^ Next())));

                return data;
            }

            public byte[] Read(byte[] input)
            {
                byte[] data = new byte[input.Length];
                for (int i = 0; i < input.Length; i++)
                {
                    data[i] = (byte)(input[i]^Next());
                }
                return data;
            }
        }

        #endregion

        #endregion

        #region Properties

        public Header HeaderSettings { get; protected internal set; } = new Header();
        public PrintParameters PrintParametersSettings { get; protected internal set; } = new PrintParameters();

        public SlicerInfo SlicerInfoSettings { get; protected internal set; } = new SlicerInfo();

        public Preview[] Previews { get; protected internal set; }

        public LayerData[,] LayerDefinitions { get; private set; }

        public Dictionary<string, LayerData> LayersHash { get; } = new Dictionary<string, LayerData>();

        public override FileFormatType FileType => FileFormatType.Binary;

        public override FileExtension[] FileExtensions { get; } = {
            new("ctb", "Chitubox CTB"),
            new("cbddlp", "Chitubox CBDDLP"),
            new("photon", "Chitubox Photon"),
        };

        public override PrintParameterModifier[] PrintParameterModifiers { get; } =
        {
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

        public override PrintParameterModifier[] PrintParameterPerLayerModifiers {
            get
            {
                if (HeaderSettings.Version >= 3)
                {
                    return new[]
                    {
                        PrintParameterModifier.ExposureSeconds,
                        PrintParameterModifier.LiftHeight,
                        PrintParameterModifier.LiftSpeed,
                        PrintParameterModifier.RetractSpeed,
                        PrintParameterModifier.LightOffDelay,
                        PrintParameterModifier.LightPWM,
                    };
                }
                
                if (HeaderSettings.Version >= 2)
                {
                    return new[]
                    {
                        PrintParameterModifier.ExposureSeconds,
                        PrintParameterModifier.LightOffDelay,
                    };
                }

                return null;
            } 
        }



        public override byte ThumbnailsCount { get; } = 2;

        public override Size[] ThumbnailsOriginalSize { get; } = {new Size(400, 300), new Size(200, 125)};

        public override uint ResolutionX
        {
            get => HeaderSettings.ResolutionX;
            set
            {
                HeaderSettings.ResolutionX = value;
                RaisePropertyChanged();
            }
        }

        public override uint ResolutionY
        {
            get => HeaderSettings.ResolutionY;
            set
            {
                HeaderSettings.ResolutionY = value;
                RaisePropertyChanged();
            }
        }

        public override float DisplayWidth
        {
            get => HeaderSettings.BedSizeX;
            set
            {
                HeaderSettings.BedSizeX = (float) Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float DisplayHeight
        {
            get => HeaderSettings.BedSizeY;
            set
            {
                HeaderSettings.BedSizeY = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float MaxPrintHeight
        {
            get => HeaderSettings.BedSizeZ > 0 ? HeaderSettings.BedSizeZ : base.MaxPrintHeight;
            set
            {
                HeaderSettings.BedSizeZ = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override bool MirrorDisplay
        {
            get => HeaderSettings.ProjectorType > 0;
            set
            {
                HeaderSettings.ProjectorType = value ? 1u : 0;
                RaisePropertyChanged();
            }
        }

        public override byte AntiAliasing
        {
            get => (byte) (IsCbtFile ? SlicerInfoSettings.AntiAliasLevel : HeaderSettings.AntiAliasLevel);
            set
            {
                if (IsCbtFile)
                {
                    SlicerInfoSettings.AntiAliasLevel = value;
                }
                else
                {
                    HeaderSettings.AntiAliasLevel = value.Clamp(1, 16);
                    ValidateAntiAliasingLevel();
                }
                RaisePropertyChanged();
            }
        }

        public override float LayerHeight
        {
            get => HeaderSettings.LayerHeightMilimeter;
            set
            {
                HeaderSettings.LayerHeightMilimeter = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float PrintHeight
        {
            get => base.PrintHeight;
            set
            {
                HeaderSettings.TotalHeightMilimeter = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override uint LayerCount
        {
            set
            {
                HeaderSettings.LayerCount = LayerCount;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NormalLayerCount));
            }
        }

        public override ushort BottomLayerCount
        {
            get => (ushort) HeaderSettings.BottomLayersCount;
            set
            {
                HeaderSettings.BottomLayersCount = value;
                RaisePropertyChanged();
            }
        }

        public override float BottomExposureTime
        {
            get => HeaderSettings.BottomExposureSeconds;
            set
            {
                HeaderSettings.BottomExposureSeconds = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float ExposureTime
        {
            get => HeaderSettings.LayerExposureSeconds;
            set
            {
                HeaderSettings.LayerExposureSeconds = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLightOffDelay
        {
            get => PrintParametersSettings.BottomLightOffDelay;
            set
            {
                PrintParametersSettings.BottomLightOffDelay = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LightOffDelay
        {
            get => PrintParametersSettings.LightOffDelay;
            set
            {
                HeaderSettings.LightOffDelay = PrintParametersSettings.LightOffDelay = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLiftHeight
        {
            get => PrintParametersSettings.BottomLiftHeight;
            set
            {
                PrintParametersSettings.BottomLiftHeight = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LiftHeight
        {
            get => PrintParametersSettings.LiftHeight;
            set
            {
                PrintParametersSettings.LiftHeight = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float BottomLiftSpeed
        {
            get => PrintParametersSettings.BottomLiftSpeed;
            set
            {
                PrintParametersSettings.BottomLiftSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float LiftSpeed
        {
            get => PrintParametersSettings.LiftSpeed;
            set
            {
                PrintParametersSettings.LiftSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override float RetractSpeed
        {
            get => PrintParametersSettings.RetractSpeed;
            set
            {
                PrintParametersSettings.RetractSpeed = (float)Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        public override byte BottomLightPWM
        {
            get => (byte) HeaderSettings.BottomLightPWM;
            set
            {
                HeaderSettings.BottomLightPWM = value;
                RaisePropertyChanged();
            }
        }

        public override byte LightPWM
        {
            get => (byte) HeaderSettings.BottomLightPWM;
            set
            {
                HeaderSettings.BottomLightPWM = value;
                RaisePropertyChanged();
            }
        }

        public override float PrintTime
        {
            get => base.PrintTime;
            set
            {
                base.PrintTime = value;
                HeaderSettings.PrintTime = (uint)base.PrintTime;
            }
        }

        public override float MaterialMilliliters
        {
            get => base.MaterialMilliliters;
            set
            {
                base.MaterialMilliliters = value;
                PrintParametersSettings.VolumeMl = base.MaterialMilliliters;
            }
        }

        public override float MaterialGrams
        {
            get => PrintParametersSettings.WeightG;
            set
            {
                PrintParametersSettings.WeightG = (float)Math.Round(value, 3);
                RaisePropertyChanged();
            }
        }

        public override float MaterialCost
        {
            get => (float) Math.Round(PrintParametersSettings.CostDollars, 3);
            set
            {
                PrintParametersSettings.CostDollars = (float) Math.Round(value, 3);
                RaisePropertyChanged();
            }
        }

        public override string MachineName
        {
            get => SlicerInfoSettings.MachineName;
            set
            {
                SlicerInfoSettings.MachineName = value;
                SlicerInfoSettings.MachineNameSize = (uint) SlicerInfoSettings.MachineName.Length;
                RequireFullEncode = true;
                RaisePropertyChanged();
            }
        }

        public override object[] Configs => new[] { (object)HeaderSettings, PrintParametersSettings, SlicerInfoSettings };

        public bool IsCbddlpFile => HeaderSettings.Magic == MAGIC_CBDDLP;
        public bool IsCbtFile => HeaderSettings.Magic == MAGIC_CBT;

        public bool CanHash => !IsCbtFile && HeaderSettings.Version <= 2;
        #endregion

        #region Constructors
        public ChituboxFile()
        {
            Previews = new Preview[ThumbnailsCount];
        }
        #endregion

        #region Methods
        public override void Clear()
        {
            base.Clear();

            for (byte i = 0; i < ThumbnailsCount; i++)
            {
                Previews[i] = new Preview();
            }

            LayerDefinitions = null;
        }

        protected override void EncodeInternally(string fileFullPath, OperationProgress progress)
        {
            LayersHash.Clear();

            HeaderSettings.Magic = fileFullPath.EndsWith(".ctb") || fileFullPath.EndsWith($".ctb{TemporaryFileAppend}") ? MAGIC_CBT : MAGIC_CBDDLP;
            HeaderSettings.PrintParametersSize = (uint)Helpers.Serializer.SizeOf(PrintParametersSettings);

            if (IsCbtFile)
            {
                if (SlicerInfoSettings.AntiAliasLevel <= 1)
                {
                    SlicerInfoSettings.AntiAliasLevel = HeaderSettings.AntiAliasLevel;
                }

                HeaderSettings.AntiAliasLevel = 1;

                if (HeaderSettings.Version <= 2)
                {
                    if (SlicerInfoSettings.Unknown1 == 0)
                        SlicerInfoSettings.Unknown1 = 0x200; // 512 for v2 | 0 for v3
                    SlicerInfoSettings.EncryptionMode = ENCRYPTYION_MODE_CTBv2;
                    PrintParametersSettings.Padding4 = 0x1234; // 4660
                }
                else
                {
                    SlicerInfoSettings.EncryptionMode = ENCRYPTYION_MODE_CTBv3;
                }

                
                if(SlicerInfoSettings.MysteriousId == 0)
                    SlicerInfoSettings.MysteriousId = 0x12345678;

                if (HeaderSettings.EncryptionKey == 0)
                {
                    Random rnd = new Random();
                    HeaderSettings.EncryptionKey = (uint)rnd.Next(byte.MaxValue, int.MaxValue);
                }
            }
            else
            {
                //HeaderSettings.Version = 2;
                HeaderSettings.EncryptionKey = 0; // Force disable encryption
                SlicerInfoSettings.EncryptionMode = ENCRYPTYION_MODE_CBDDLP;
            }

            uint currentOffset = (uint)Helpers.Serializer.SizeOf(HeaderSettings);
            LayerDefinitions = new LayerData[HeaderSettings.AntiAliasLevel, HeaderSettings.LayerCount];
            using var outputFile = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write);
            outputFile.Seek((int) currentOffset, SeekOrigin.Begin);

            Mat[] thumbnails = {GetThumbnail(true), GetThumbnail(false)};
            for (byte i = 0; i < thumbnails.Length; i++)
            {
                var image = thumbnails[i];

                Preview preview = new Preview
                {
                    ResolutionX = (uint)image.Width,
                    ResolutionY = (uint)image.Height,
                };

                var previewBytes = preview.Encode(image);

                if (previewBytes.Length == 0) continue;

                if (i == 0)
                {
                    HeaderSettings.PreviewLargeOffsetAddress = currentOffset;
                }
                else
                {
                    HeaderSettings.PreviewSmallOffsetAddress = currentOffset;
                }


                currentOffset += (uint) Helpers.Serializer.SizeOf(preview);
                preview.ImageOffset = currentOffset;

                Helpers.SerializeWriteFileStream(outputFile, preview);
                currentOffset += outputFile.WriteBytes(previewBytes);
            }


            if (HeaderSettings.Version >= 2)
            {
                HeaderSettings.PrintParametersOffsetAddress = currentOffset;

                currentOffset += Helpers.SerializeWriteFileStream(outputFile, PrintParametersSettings);

                HeaderSettings.SlicerOffset = currentOffset;
                HeaderSettings.SlicerSize = (uint) Helpers.Serializer.SizeOf(SlicerInfoSettings) - SlicerInfoSettings.MachineNameSize;

                SlicerInfoSettings.MachineNameAddress = currentOffset + HeaderSettings.SlicerSize;


                currentOffset += Helpers.SerializeWriteFileStream(outputFile, SlicerInfoSettings);
            }

            HeaderSettings.LayersDefinitionOffsetAddress = currentOffset;
            uint layerDataCurrentOffset = currentOffset + (uint)Helpers.Serializer.SizeOf(new LayerData()) * HeaderSettings.LayerCount * HeaderSettings.AntiAliasLevel;
                
            progress.ItemCount *= 2 * HeaderSettings.AntiAliasLevel;

            for (byte aaIndex = 0; aaIndex < HeaderSettings.AntiAliasLevel; aaIndex++)
            {
                progress.Token.ThrowIfCancellationRequested();
                Parallel.For(0, LayerCount, /*new ParallelOptions{MaxDegreeOfParallelism = 1},*/ layerIndex =>
                {
                    if (progress.Token.IsCancellationRequested) return;
                    LayerData layerData = new LayerData(this, (uint) layerIndex);
                    using (var image = this[layerIndex].LayerMat)
                    {
                        layerData.Encode(image, aaIndex, (uint) layerIndex);
                        LayerDefinitions[aaIndex, layerIndex] = layerData;
                    }

                    lock (progress.Mutex)
                    {
                        progress++;
                    }
                });

                for (uint layerIndex = 0; layerIndex < LayerCount; layerIndex++)
                {
                    progress.Token.ThrowIfCancellationRequested();
                    var layerData = LayerDefinitions[aaIndex, layerIndex];
                    LayerData layerDataHash = null;

                    if (CanHash)
                    {
                        string hash = Helpers.ComputeSHA1Hash(layerData.EncodedRle);
                        if (LayersHash.TryGetValue(hash, out layerDataHash))
                        {
                            layerData.DataAddress = layerDataHash.DataAddress;
                            layerData.DataSize = layerDataHash.DataSize;
                        }
                        else
                        {
                            LayersHash.Add(hash, layerData);
                        }
                    }

                    if (layerDataHash is null)
                    {
                        layerData.DataAddress = layerDataCurrentOffset;
                        outputFile.Seek(layerDataCurrentOffset, SeekOrigin.Begin);

                        if (HeaderSettings.Version >= 3)
                        {
                            var layerDataEx = new LayerDataEx(layerData, layerIndex);
                            layerDataCurrentOffset += (uint)Helpers.Serializer.SizeOf(layerDataEx);
                            layerData.DataAddress = layerDataCurrentOffset;
                            Helpers.SerializeWriteFileStream(outputFile, layerDataEx);
                        }

                        layerDataCurrentOffset += outputFile.WriteBytes(layerData.EncodedRle);
                    }
                        
                    outputFile.Seek(currentOffset, SeekOrigin.Begin);
                    currentOffset += Helpers.SerializeWriteFileStream(outputFile, layerData);

                    progress++;
                }
            }

            outputFile.Seek(0, SeekOrigin.Begin);
            Helpers.SerializeWriteFileStream(outputFile, HeaderSettings);

            Debug.WriteLine("Encode Results:");
            Debug.WriteLine(HeaderSettings);
            Debug.WriteLine(Previews[0]);
            Debug.WriteLine(Previews[1]);
            Debug.WriteLine(PrintParametersSettings);
            Debug.WriteLine(SlicerInfoSettings);
            Debug.WriteLine("-End-");
        }


        protected override void DecodeInternally(string fileFullPath, OperationProgress progress)
        {
            using var inputFile = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
            //HeaderSettings = Helpers.ByteToType<CbddlpFile.Header>(InputFile);
            //HeaderSettings = Helpers.Serializer.Deserialize<Header>(InputFile.ReadBytes(Helpers.Serializer.SizeOf(typeof(Header))));
            HeaderSettings = Helpers.Deserialize<Header>(inputFile);
            if (HeaderSettings.Magic != MAGIC_CBDDLP && HeaderSettings.Magic != MAGIC_CBT)
            {
                throw new FileLoadException("Not a valid CBDDLP nor CTB nor Photon file!", fileFullPath);
            }

            if (HeaderSettings.Version == 1 || HeaderSettings.AntiAliasLevel == 0)
            {
                HeaderSettings.AntiAliasLevel = 1;
            }

            FileFullPath = fileFullPath;

            progress.Reset(OperationProgress.StatusDecodeThumbnails, ThumbnailsCount);

            Debug.Write("Header -> ");
            Debug.WriteLine(HeaderSettings);

            for (byte i = 0; i < ThumbnailsCount; i++)
            {
                uint offsetAddress = i == 0
                    ? HeaderSettings.PreviewLargeOffsetAddress
                    : HeaderSettings.PreviewSmallOffsetAddress;
                if (offsetAddress == 0) continue;

                inputFile.Seek(offsetAddress, SeekOrigin.Begin);
                Previews[i] = Helpers.Deserialize<Preview>(inputFile);

                Debug.Write($"Preview {i} -> ");
                Debug.WriteLine(Previews[i]);

                inputFile.Seek(Previews[i].ImageOffset, SeekOrigin.Begin);
                byte[] rawImageData = new byte[Previews[i].ImageLength];
                inputFile.Read(rawImageData, 0, (int) Previews[i].ImageLength);

                Thumbnails[i] = Previews[i].Decode(rawImageData);
                progress++;
            }

            if (HeaderSettings.PrintParametersOffsetAddress > 0)
            {
                inputFile.Seek(HeaderSettings.PrintParametersOffsetAddress, SeekOrigin.Begin);
                PrintParametersSettings = Helpers.Deserialize<PrintParameters>(inputFile);
                Debug.Write("Print Parameters -> ");
                Debug.WriteLine(PrintParametersSettings);


            }

            if (HeaderSettings.SlicerOffset > 0)
            {
                inputFile.Seek(HeaderSettings.SlicerOffset, SeekOrigin.Begin);
                SlicerInfoSettings = Helpers.Deserialize<SlicerInfo>(inputFile);
                Debug.Write("Slicer Info -> ");
                Debug.WriteLine(SlicerInfoSettings);
            }

            /*InputFile.BaseStream.Seek(MachineInfoSettings.MachineNameAddress, SeekOrigin.Begin);
                byte[] bytes = InputFile.ReadBytes((int)MachineInfoSettings.MachineNameSize);
                MachineName = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.WriteLine($"{nameof(MachineName)}: {MachineName}");*/
            //}

            LayerDefinitions = new LayerData[HeaderSettings.AntiAliasLevel, HeaderSettings.LayerCount];
            var LayerDefinitionsEx = HeaderSettings.Version >= 3 ? new LayerDataEx[HeaderSettings.LayerCount] : null;

            uint layerOffset = HeaderSettings.LayersDefinitionOffsetAddress;

            progress.Reset(OperationProgress.StatusGatherLayers,
                HeaderSettings.AntiAliasLevel * HeaderSettings.LayerCount);

            for (byte aaIndex = 0; aaIndex < HeaderSettings.AntiAliasLevel; aaIndex++)
            {
                Debug.WriteLine($"-Image GROUP {aaIndex}-");
                for (uint layerIndex = 0; layerIndex < HeaderSettings.LayerCount; layerIndex++)
                {
                    inputFile.Seek(layerOffset, SeekOrigin.Begin);
                    LayerData layerData = Helpers.Deserialize<LayerData>(inputFile);
                    layerData.Parent = this;
                    LayerDefinitions[aaIndex, layerIndex] = layerData;

                    layerOffset += (uint) Helpers.Serializer.SizeOf(layerData);
                    Debug.Write($"LAYER {layerIndex} -> ");
                    Debug.WriteLine(layerData);

                    layerData.EncodedRle = new byte[layerData.DataSize];
                        

                    if (HeaderSettings.Version < 3)
                    {
                        inputFile.Seek(layerData.DataAddress, SeekOrigin.Begin);
                    }
                    else
                    {
                        inputFile.Seek(layerData.DataAddress - 84, SeekOrigin.Begin);
                        LayerDefinitionsEx[layerIndex] = Helpers.Deserialize<LayerDataEx>(inputFile);
                        Debug.Write($"LAYER {layerIndex} -> ");
                        Debug.WriteLine(LayerDefinitionsEx[layerIndex]);
                    }


                    inputFile.Read(layerData.EncodedRle, 0, (int) layerData.DataSize);
                        
                    progress++;
                    progress.Token.ThrowIfCancellationRequested();
                }
            }

            LayerManager = new LayerManager(HeaderSettings.LayerCount, this);

            progress.Reset(OperationProgress.StatusDecodeLayers, LayerCount);

            Parallel.For(0, LayerCount, layerIndex =>
                //for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                if (progress.Token.IsCancellationRequested)
                {
                    return;
                }

                using var image = LayerDefinitions[0, layerIndex].Decode((uint) layerIndex);
                var layer = new Layer((uint) layerIndex, image, LayerManager)
                {
                    PositionZ = LayerDefinitions[0, layerIndex].LayerPositionZ,
                    ExposureTime = LayerDefinitions[0, layerIndex].LayerExposure,
                    LightOffDelay = LayerDefinitions[0, layerIndex].LightOffSeconds,
                };

                if (LayerDefinitionsEx is not null)
                {
                    layer.LiftHeight = LayerDefinitionsEx[layerIndex].LiftHeight;
                    layer.LiftSpeed = LayerDefinitionsEx[layerIndex].LiftSpeed;
                    layer.RetractSpeed = LayerDefinitionsEx[layerIndex].RetractSpeed;
                    layer.LightPWM = (byte) LayerDefinitionsEx[layerIndex].LightPWM;
                }

                this[layerIndex] = layer;

                lock (progress.Mutex)
                {
                    progress++;
                }
            });
        }
        private void Optimize()
        {
            float base_time,base_lift;
            int layer_num = 0;
            
            foreach (var layer in LayerManager.Layers)
            {
                layer_num++;
                base_lift = BottomLiftHeight;
                //get exposure time of first layer, then gradually draw down
                if (layer_num > BottomLayerCount + 6)
                {
                    var mat = layer.LayerMat;

                    {
                        using var nonZeroMat = new Mat();
                        CvInvoke.FindNonZero(mat, nonZeroMat);
                        var NonZeroPixelCount = (uint)nonZeroMat.Height;
                        float percent_pixels = NonZeroPixelCount / (float)(ResolutionX * ResolutionY);
                        float lift_height = (BottomLiftHeight - LiftHeight) * (percent_pixels) + LiftHeight;
                        layer.LiftHeight = lift_height;
                        layer.LiftSpeed = (LiftSpeed - BottomLiftSpeed) * (1-percent_pixels) + BottomLiftSpeed;
                    }
                    mat.Dispose();
                }
                else 
                {
                    float[] divisors = { 1.3F, 1.6F, 2, 3, 5,6,8,10};
                    if (layer_num > BottomLayerCount)
                        //gradually decrease layer exposure time
                        if (BottomExposureTime / (divisors[layer_num - BottomLayerCount]) > ExposureTime)
                        {
                            layer.ExposureTime = BottomExposureTime / (divisors[layer_num - BottomLayerCount]);
                        }
                        else
                        {
                            layer.ExposureTime = ExposureTime;
                        }
                }
            }
        }
        public override void SaveAs(string filePath = null, OperationProgress progress = null)
        {
            Optimize();
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

            using var outputFile = new FileStream(FileFullPath, FileMode.Open, FileAccess.Write);
            outputFile.Seek(0, SeekOrigin.Begin);
            Helpers.SerializeWriteFileStream(outputFile, HeaderSettings);

            if (HeaderSettings.Version >= 2 && HeaderSettings.PrintParametersOffsetAddress > 0)
            {
                outputFile.Seek(HeaderSettings.PrintParametersOffsetAddress, SeekOrigin.Begin);
                Helpers.SerializeWriteFileStream(outputFile, PrintParametersSettings);
                Helpers.SerializeWriteFileStream(outputFile, SlicerInfoSettings);
            }

            uint layerOffset = HeaderSettings.LayersDefinitionOffsetAddress;
            for (byte aaIndex = 0; aaIndex < HeaderSettings.AntiAliasLevel; aaIndex++)
            {
                for (uint layerIndex = 0; layerIndex < HeaderSettings.LayerCount; layerIndex++)
                {
                    LayerDefinitions[aaIndex, layerIndex].RefreshLayerData(this, layerIndex);

                    outputFile.Seek(layerOffset, SeekOrigin.Begin);
                    layerOffset +=
                        Helpers.SerializeWriteFileStream(outputFile, LayerDefinitions[aaIndex, layerIndex]);
                }
            }

            if (HeaderSettings.Version >= 3)
            {
                for (uint layerIndex = 0; layerIndex < HeaderSettings.LayerCount; layerIndex++)
                {
                    outputFile.Seek(LayerDefinitions[0, layerIndex].DataAddress - 84, SeekOrigin.Begin);
                    Helpers.SerializeWriteFileStream(outputFile, new LayerDataEx(LayerDefinitions[0, layerIndex], layerIndex));
                }
            }
        }

        #endregion
    }
}
