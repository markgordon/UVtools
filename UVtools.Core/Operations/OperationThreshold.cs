﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using UVtools.Core.FileFormats;

namespace UVtools.Core.Operations
{
    [Serializable]
    public class OperationThreshold : Operation
    {
        #region Members
        private byte _threshold = 127;
        private byte _maximum = 255;
        private ThresholdType _type = ThresholdType.Binary;
        #endregion

        #region Overrides
        public override string Title => "Threshold pixels";
        public override string Description =>
            "Manipulate pixel values based on a threshold.\n\n" +
            "Pixles brighter than the theshold will be set to the Max value, " +
            "all other pixels will be set to 0.\n\n" +
            "See https://docs.opencv.org/master/d7/d4d/tutorial_py_thresholding.html";

        public override string ConfirmationText =>
            $"apply threshold {Threshold} with max {Maximum} from layers {LayerIndexStart} through {LayerIndexEnd}";

        public override string ProgressTitle =>
            $"Applying threshold {Threshold} with max {Maximum} from layers {LayerIndexStart} through {LayerIndexEnd}";

        public override string ProgressAction => "Thresholded layers";

        public override string ToString()
        {
            var result = $"[{_type} = {_threshold} / {_maximum}]" + LayerRangeString;
            if (!string.IsNullOrEmpty(ProfileName)) result = $"{ProfileName}: {result}";
            return result;
        }
        #endregion

        #region Properties
        public byte Threshold
        {
            get => _threshold;
            set => RaiseAndSetIfChanged(ref _threshold, value);
        }

        public byte Maximum
        {
            get => _maximum;
            set => RaiseAndSetIfChanged(ref _maximum, value);
        }

        public ThresholdType Type
        {
            get => _type;
            set => RaiseAndSetIfChanged(ref _type, value);
        }

        public static Array ThresholdTypes => Enum.GetValues(typeof(ThresholdType));
        #endregion

        #region Methods

        public override bool Execute(FileFormat slicerFile, OperationProgress progress = null)
        {
            progress ??= new OperationProgress();
            progress.Reset(ProgressAction, LayerRangeCount);
            Parallel.For(LayerIndexStart, LayerIndexEnd + 1, layerIndex =>
            {
                if (progress.Token.IsCancellationRequested) return;
                using (var mat = slicerFile[layerIndex].LayerMat)
                {
                    Execute(mat);
                    slicerFile[layerIndex].LayerMat = mat;
                }

                lock (progress.Mutex)
                {
                    progress++;
                }
            });
            progress.Token.ThrowIfCancellationRequested();

            return true;
        }

        public override bool Execute(Mat mat, params object[] arguments)
        {
            Mat target = GetRoiOrDefault(mat);
            CvInvoke.Threshold(target, target, Threshold, Maximum, Type);
            return true;
        }

        #endregion

        #region Equality

        protected bool Equals(OperationThreshold other)
        {
            return _threshold == other._threshold && _maximum == other._maximum && _type == other._type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationThreshold) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _threshold.GetHashCode();
                hashCode = (hashCode * 397) ^ _maximum.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _type;
                return hashCode;
            }
        }

        #endregion
    }
}
