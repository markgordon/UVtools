﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;
using Emgu.CV;
using UVtools.Core.FileFormats;
using UVtools.Core.Objects;

namespace UVtools.Core.Operations
{
    [Serializable]
    public abstract class Operation : BindableBase, IDisposable
    {
        private Rectangle _roi = Rectangle.Empty;
        private uint _layerIndexEnd;
        private uint _layerIndexStart;
        private string _profileName;
        private bool _profileIsDefault;
        private Enumerations.LayerRangeSelection _layerRangeSelection = Enumerations.LayerRangeSelection.All;
        public const byte ClassNameLength = 9;

        /// <summary>
        /// Gets the ID name of this operation, this comes from class name with "Operation" removed
        /// </summary>
        public string Id => GetType().Name.Remove(0, ClassNameLength);

        public virtual Enumerations.LayerRangeSelection StartLayerRangeSelection => Enumerations.LayerRangeSelection.All;

        /// <summary>
        /// Gets the last used layer range selection, returns none if custom
        /// </summary>
        public Enumerations.LayerRangeSelection LayerRangeSelection
        {
            get => _layerRangeSelection;
            set => RaiseAndSetIfChanged(ref _layerRangeSelection, value);
        }

        /// <summary>
        /// Gets if this operation should set layer range to the actual layer index on layer preview
        /// </summary>
        public virtual bool PassActualLayerIndex => false;

        /// <summary>
        /// Gets if this operation can make use of ROI
        /// </summary>
        public virtual bool CanROI => true;

        /// <summary>
        /// Gets if this operation can store profiles
        /// </summary>
        public virtual bool CanHaveProfiles => true;

        /// <summary>
        /// Gets if this operation supports cancellation
        /// </summary>
        public virtual bool CanCancel => true;

        /// <summary>
        /// Gets the title of this operation
        /// </summary>
        public virtual string Title => Id;

        /// <summary>
        /// Gets a descriptive text of this operation
        /// </summary>
        public virtual string Description => Id;

        /// <summary>
        /// Gets the Ok button text
        /// </summary>
        public virtual string ButtonOkText => Title;

        /// <summary>
        /// Gets the confirmation text for the operation
        /// </summary>
        public virtual string ConfirmationText => $"Are you sure you want to {Id}";

        /// <summary>
        /// Gets the progress window title
        /// </summary>
        public virtual string ProgressTitle => "Processing items";

        /// <summary>
        /// Gets the progress action name
        /// </summary>
        public virtual string ProgressAction => Id;

        public bool HaveAction => !string.IsNullOrEmpty(ProgressAction);

        /// <summary>
        /// Validates the operation
        /// </summary>
        /// <returns>null or empty if validates, or else, return a string with error message</returns>
        public virtual StringTag Validate(params object[] parameters) => null;

        public bool CanValidate(params object[] parameters)
        {
            var result = Validate(parameters);
            return result is null || string.IsNullOrEmpty(result.Content);
        }

        /// <summary>
        /// Gets the start layer index where operation will starts in
        /// </summary>
        public virtual uint LayerIndexStart
        {
            get => _layerIndexStart;
            set => RaiseAndSetIfChanged(ref _layerIndexStart, value);
        }

        /// <summary>
        /// Gets the end layer index where operation will ends in
        /// </summary>
        public virtual uint LayerIndexEnd
        {
            get => _layerIndexEnd;
            set => RaiseAndSetIfChanged(ref _layerIndexEnd, value);
        }

        public uint LayerRangeCount => LayerIndexEnd - LayerIndexStart + 1;

        /// <summary>
        /// Gets the name for this profile
        /// </summary>
        public string ProfileName
        {
            get => _profileName;
            set => RaiseAndSetIfChanged(ref _profileName, value);
        }

        public bool ProfileIsDefault
        {
            get => _profileIsDefault;
            set => RaiseAndSetIfChanged(ref _profileIsDefault, value);
        }

        /// <summary>
        /// Gets or sets an ROI to process this operation
        /// </summary>
        [XmlIgnore]
        public Rectangle ROI
        {
            get => _roi;
            set => RaiseAndSetIfChanged(ref _roi, value);
        }

        public bool HaveROI => !ROI.IsEmpty;

        public Mat GetRoiOrDefault(Mat defaultMat)
        {
            return HaveROI ? new Mat(defaultMat, ROI) : defaultMat;
        }

        public virtual Operation Clone()
        {
            return MemberwiseClone() as Operation;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ProfileName)) return ProfileName;

            var result = $"{Title}: {LayerRangeString}";
            return result;
        }

        public virtual string LayerRangeString 
        {
            get
            {
                if (LayerRangeSelection == Enumerations.LayerRangeSelection.None)
                {
                    return $" [Layers: {LayerIndexStart}-{LayerIndexEnd}]";
                }

                return $" [Layers: {LayerRangeSelection}]";
            }
        }

        public virtual bool Execute(FileFormat slicerFile, OperationProgress progress = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool Execute(Mat mat, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose() { }
    }
}
