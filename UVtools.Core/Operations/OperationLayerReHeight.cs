﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;
using UVtools.Core.Extensions;
using UVtools.Core.FileFormats;
using UVtools.Core.Objects;

namespace UVtools.Core.Operations
{
    [Serializable]
    public sealed class OperationLayerReHeight : Operation
    {
        #region Members
        private OperationLayerReHeightItem _item;
        #endregion

        #region Overrides

        public override Enumerations.LayerRangeSelection StartLayerRangeSelection => Enumerations.LayerRangeSelection.None;
        public override bool CanROI => false;
        public override string Title => "Adjust layer height";
        public override string Description =>
            "Adjust the layer height of the model\n\n" +
            "Adjusting to values lower than current height will reduce layer lines, adjusting to values higher" +
            " than current height will reduce model detail.\n\n" +
            "Note: Using dedicated slicer software to re-slice will usually yeild better results.";
        public override string ConfirmationText =>
            $"adjust layer height to {Item.LayerHeight}mm?";

        public override string ProgressTitle =>
            $"Adjusting layer height to {Item.LayerHeight}mm";

        public override string ProgressAction => "Height adjusted layers";

        public override bool CanHaveProfiles => false;

        public override StringTag Validate(params object[] parameters)
        {
            var sb = new StringBuilder();

            if (Item is null)
            {
                sb.AppendLine("No valid configurations, unable to proceed.");
            }


            return new StringTag(sb.ToString());
        }

        public override string ToString()
        {
            var result = $"[Layer Count: {Item.LayerCount}] [Layer Height: {Item.LayerHeight}]" + LayerRangeString;
            if (!string.IsNullOrEmpty(ProfileName)) result = $"{ProfileName}: {result}";
            return result;
        }
        #endregion

        #region Properties
        public OperationLayerReHeightItem Item
        {
            get => _item;
            set => RaiseAndSetIfChanged(ref _item, value);
        }


        public static OperationLayerReHeightItem[] GetItems(uint layerCount, decimal layerHeight)
        {
            var list = new List<OperationLayerReHeightItem>();
            for (byte i = 2; i < 255; i++) // Go lower heights
            {
                if (layerHeight / i < 0.01m) break;
                if ((layerCount * (decimal)i).DecimalDigits() > 0) continue; // Cant multiply layers, no half layers!
                if ((layerHeight / i).DecimalDigits() > 2) continue; // Cant divide height, more than 2 digits

                var item = new OperationLayerReHeightItem(false, i, Math.Round(layerHeight / i, 2), layerCount * i);
                list.Add(item);
            }

            for (byte i = 2; i < 255; i++) // Go higher heights
            {
                if (layerHeight * i > 0.2m) break;
                if ((layerCount / (decimal)i).DecimalDigits() > 0) continue; // Cant divide layers, no half layers!
                if ((layerHeight * i).DecimalDigits() > 2) continue; // Cant multiply height, more than 2 digits

                var item = new OperationLayerReHeightItem(true, i, Math.Round(layerHeight * i, 2), layerCount / i);
                list.Add(item);
            }

            return list.ToArray();
        }
        #endregion

        #region Subclasses
        public class OperationLayerReHeightItem
        {
            public bool IsMultiply { get; }
            public bool IsDivision => !IsMultiply;
            public byte Modifier { get; }
            public decimal LayerHeight { get; }
            public uint LayerCount { get; }

            public OperationLayerReHeightItem(bool isMultiply, byte modifier, decimal layerHeight, uint layerCount)
            {
                IsMultiply = isMultiply;
                Modifier = modifier;
                LayerHeight = layerHeight;
                LayerCount = layerCount;
            }

            public override string ToString()
            {
                return (IsMultiply ? 'x' : '÷') + $" {Modifier} → {LayerCount} layers at {LayerHeight}mm";
            }
        }
        #endregion

        #region Methods
        public override bool Execute(FileFormat slicerFile, OperationProgress progress = null)
        {
            progress ??= new OperationProgress();
            progress.Reset(ProgressAction, Item.LayerCount);

            var oldLayers = slicerFile.LayerManager.Layers;

            var layers = new Layer[Item.LayerCount];

            uint newLayerIndex = 0;
            for (uint layerIndex = 0; layerIndex < oldLayers.Length; layerIndex++)
            {
                progress.Token.ThrowIfCancellationRequested();
                
                var oldLayer = oldLayers[layerIndex];
                if (Item.IsDivision)
                {
                    for (byte i = 0; i < Item.Modifier; i++)
                    {
                        var newLayer = oldLayer.Clone();
                        newLayer.Index = newLayerIndex;
                        newLayer.PositionZ = (float)(Item.LayerHeight * (newLayerIndex + 1));
                        layers[newLayerIndex] = newLayer;
                        newLayerIndex++;
                        progress++;
                    }
                }
                else
                {
                    using var mat = oldLayers[layerIndex++].LayerMat;
                    for (byte i = 1; i < Item.Modifier; i++)
                    {
                        using var nextMat = oldLayers[layerIndex++].LayerMat;
                        CvInvoke.Add(mat, nextMat, mat);
                    }

                    var newLayer = oldLayer.Clone();
                    newLayer.Index = newLayerIndex;
                    newLayer.PositionZ = (float)(Item.LayerHeight * (newLayerIndex + 1));
                    newLayer.LayerMat = mat;
                    layers[newLayerIndex] = newLayer;
                    newLayerIndex++;
                    layerIndex--;
                    progress++;
                }
            }

            slicerFile.LayerManager.Layers = layers;
            slicerFile.LayerHeight = (float)Item.LayerHeight;
            slicerFile.RequireFullEncode = true;

            return true;
        }
        #endregion
    }
}
