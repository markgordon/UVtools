﻿using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Emgu.CV.CvEnum;
using UVtools.Core.Operations;
using UVtools.WPF.Windows;

namespace UVtools.WPF.Controls.Tools
{
    public class ToolMorphControl : ToolControl
    {
        private byte _morphSelectedIndex;
        public OperationMorph Operation => BaseOperation as OperationMorph;

        private KernelControl _kernelCtrl;

        public byte MorphSelectedIndex
        {
            get => _morphSelectedIndex;
            set
            {
                if(!RaiseAndSetIfChanged(ref _morphSelectedIndex, value)) return;
                Operation.MorphOperation = (MorphOp)OperationMorph.MorphOperations[_morphSelectedIndex].Tag;
            }
        }

        public ToolMorphControl()
        {
            InitializeComponent();
            BaseOperation = new OperationMorph();
            _kernelCtrl = this.Find<KernelControl>("KernelCtrl");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override bool UpdateOperation()
        {
            Operation.Kernel.Matrix = _kernelCtrl.GetMatrix();
            Operation.Kernel.Anchor = _kernelCtrl.Anchor;
            return !(Operation.Kernel.Matrix is null);
        }

        public override void Callback(ToolWindow.Callbacks callback)
        {
            switch (callback)
            {
                case ToolWindow.Callbacks.ProfileLoaded:
                    MorphSelectedIndex = Operation.MorphOperationIndex;
                    break;
            }
        }
    }
}
