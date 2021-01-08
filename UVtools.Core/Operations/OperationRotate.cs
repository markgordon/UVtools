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
using UVtools.Core.Extensions;
using UVtools.Core.FileFormats;

namespace UVtools.Core.Operations
{
    [Serializable]
    public class OperationRotate : Operation
    {
        #region Members
        private decimal _angleDegrees = 90;
        #endregion

        #region Overrides
        public override string Title => "Rotate";
        public override string Description =>
            "Rotate the layers of the model.\n";

        public override string ConfirmationText =>
            $"rotate layers {LayerIndexStart} through {LayerIndexEnd} {(AngleDegrees < 0?"counter-clockwise":"clockwise")} by {Math.Abs(AngleDegrees)}°?";

        public override string ProgressTitle =>
            $"Rotating layers {LayerIndexStart} through {LayerIndexEnd} {(AngleDegrees < 0 ? "counter-clockwise" : "clockwise")} by {Math.Abs(AngleDegrees)}°";

        public override string ProgressAction => "Rotated layers";
        
        public override string ToString()
        {
            var result = $"[Angle: {Math.Abs(_angleDegrees)}º {(AngleDegrees < 0 ? "CCW" : "CW")}]" + LayerRangeString;
            if (!string.IsNullOrEmpty(ProfileName)) result = $"{ProfileName}: {result}";
            return result;
        }
        #endregion

        #region Properties
        public decimal AngleDegrees
        {
            get => _angleDegrees;
            set => RaiseAndSetIfChanged(ref _angleDegrees, value);
        }
        #endregion

        #region Equality

        protected bool Equals(OperationRotate other)
        {
            return _angleDegrees == other._angleDegrees;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OperationRotate) obj);
        }

        public override int GetHashCode()
        {
            return _angleDegrees.GetHashCode();
        }

        #endregion

        #region Methods

        public override bool Execute(FileFormat slicerFile, OperationProgress progress = null)
        {
            progress ??= new OperationProgress();
            progress.Reset(ProgressAction, LayerRangeCount);
            Parallel.For(LayerIndexStart, LayerIndexEnd + 1, layerIndex =>
            {
                if (progress.Token.IsCancellationRequested) return;
                using var mat = slicerFile[layerIndex].LayerMat;
                Execute(mat);
                slicerFile[layerIndex].LayerMat = mat;
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
            target.Rotate((double)AngleDegrees);
            return true;
        }

        #endregion
    }
}
