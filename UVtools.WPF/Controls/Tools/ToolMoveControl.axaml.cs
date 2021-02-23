﻿using Avalonia.Markup.Xaml;
using UVtools.Core.Operations;
using UVtools.WPF.Windows;

namespace UVtools.WPF.Controls.Tools
{
    public class ToolMoveControl : ToolControl
    {
        private bool _isMiddleCenterChecked = true;
        public OperationMove Operation => BaseOperation as OperationMove;

        public bool IsMiddleCenterChecked
        {
            get => _isMiddleCenterChecked;
            set => RaiseAndSetIfChanged(ref _isMiddleCenterChecked, value);
        }

        public ToolMoveControl()
        {
            InitializeComponent();
            var roi = App.MainWindow.ROI;
            BaseOperation = new OperationMove(SlicerFile, roi);

            Operation.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals(nameof(Operation.IsWithinBoundary)))
                {
                    ParentWindow.ButtonOkEnabled = Operation.IsWithinBoundary;
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Callback(ToolWindow.Callbacks callback)
        {
            switch (callback)
            {
                case ToolWindow.Callbacks.Init:
                    ParentWindow.IsButton1Visible = true;
                    break;
                case ToolWindow.Callbacks.ClearROI:
                    Operation.ROI = App.SlicerFile.LayerManager.BoundingRectangle;
                    Operation.Reset();
                    break;
                case ToolWindow.Callbacks.Button1:
                    Operation.Reset();
                    IsMiddleCenterChecked = true;
                    break;
            }
        }
    }
}
