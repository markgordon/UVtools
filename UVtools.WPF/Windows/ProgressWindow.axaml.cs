﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.Diagnostics;
using System.Timers;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using UVtools.Core.Operations;
using UVtools.WPF.Controls;
using UVtools.WPF.Structures;

namespace UVtools.WPF.Windows
{
    public class ProgressWindow : WindowEx, IDisposable
    {
        public Stopwatch StopWatch => Progress.StopWatch;
        public OperationProgress Progress { get; } = new ();

        private LogItem _logItem = new ();

        private Timer _timer = new (200) { AutoReset = true };

        private long _lastTotalSeconds = 0;

        public bool CanCancel
        {
            get => Progress?.CanCancel ?? false;
            set
            {
                Progress.CanCancel = value;
                RaisePropertyChanged();
            }
        }

        public ProgressWindow()
        {
            InitializeComponent();

            Cursor = new Cursor(StandardCursorType.AppStarting);
            
            _timer.Elapsed += (sender, args) =>
            {
                var elapsedSeconds = StopWatch.ElapsedMilliseconds / 1000;
                if (_lastTotalSeconds == elapsedSeconds) return;
                /*Debug.WriteLine(StopWatch.ElapsedMilliseconds);
                Debug.WriteLine(elapsedSeconds);
                Debug.WriteLine(_lastTotalSeconds);*/
                _lastTotalSeconds = elapsedSeconds;


                Dispatcher.UIThread.InvokeAsync(() => Progress.TriggerRefresh(), DispatcherPriority.Render);

            };

            DataContext = this;
        }

        public ProgressWindow(string title) : this()
        {
            SetTitle(title);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer.Stop();
            Progress.StopWatch.Stop();
            _logItem.ElapsedTime = Math.Round(Progress.StopWatch.Elapsed.TotalSeconds, 2);
            App.MainWindow.AddLog(_logItem);
        }

        public void OnClickCancel()
        {
            if (!CanCancel) return;
            DialogResult = DialogResults.Cancel;
            RaisePropertyChanged(nameof(CanCancel));
            Progress.TokenSource.Cancel();
        }

        public void SetTitle(string title)
        {
            Progress.Title = title;
            _logItem.Description = title;
        }

        public OperationProgress RestartProgress(bool canCancel = true)
        {
            CanCancel = canCancel;
            Progress.StopWatch.Restart();
            _lastTotalSeconds = 0;

            if (!_timer.Enabled)
            {
                _timer.Enabled = true;
                _timer.Start();
            }

            return Progress;
        }

        public void Dispose()
        {
            _timer.Close();
            _timer.Dispose();
        }
    }
}
