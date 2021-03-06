﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */
using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Utilities;

namespace UVtools.WPF.Controls
{
    [PseudoClasses(":vertical", ":horizontal", ":pressed")]
    public class SliderEx : RangeBase, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Slider);

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            ScrollBar.OrientationProperty.AddOwner<SliderEx>();

        /// <summary>
        /// Defines the <see cref="IsSnapToTickEnabled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
            AvaloniaProperty.Register<SliderEx, bool>(nameof(IsSnapToTickEnabled), false);

        /// <summary>
        /// Defines the <see cref="TickFrequency"/> property.
        /// </summary>
        public static readonly StyledProperty<double> TickFrequencyProperty =
            AvaloniaProperty.Register<SliderEx, double>(nameof(TickFrequency), 0.0);

        /// <summary>
        /// Defines the <see cref="TickPlacement"/> property.
        /// </summary>
        public static readonly StyledProperty<TickPlacement> TickPlacementProperty =
            AvaloniaProperty.Register<SliderEx, TickPlacement>(nameof(TickPlacement), 0d);

        /// <summary>
        /// Defines the <see cref="TicksProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<double>> TicksProperty =
            TickBar.TicksProperty.AddOwner<SliderEx>();

        // Slider required parts
        private bool _isDragging = false;
        private Track _track;
        private Button _decreaseButton;
        private Button _increaseButton;
        private IDisposable _decreaseButtonPressDispose;
        private IDisposable _decreaseButtonReleaseDispose;
        private IDisposable _increaseButtonSubscription;
        private IDisposable _increaseButtonReleaseDispose;
        private IDisposable _pointerMovedDispose;

        public Track Track => _track;

        /// <summary>
        /// Initializes static members of the <see cref="Slider"/> class. 
        /// </summary>
        static SliderEx()
        {
            PressedMixin.Attach<SliderEx>();
            OrientationProperty.OverrideDefaultValue(typeof(SliderEx), Orientation.Horizontal);
            Thumb.DragStartedEvent.AddClassHandler<SliderEx>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
            Thumb.DragCompletedEvent.AddClassHandler<SliderEx>((x, e) => x.OnThumbDragCompleted(e),
                RoutingStrategies.Bubble);

            ValueProperty.OverrideMetadata<SliderEx>(new DirectPropertyMetadata<double>(enableDataValidation: true));
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="Slider"/> class. 
        /// </summary>
        public SliderEx()
        {
            UpdatePseudoClasses(Orientation);
        }

        /// <summary>
        /// Defines the ticks to be drawn on the tick bar.
        /// </summary>
        public AvaloniaList<double> Ticks
        {
            get => GetValue(TicksProperty);
            set => SetValue(TicksProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation of a <see cref="Slider"/>.
        /// </summary>
        public Orientation Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Slider"/> automatically moves the <see cref="Thumb"/> to the closest tick mark.
        /// </summary>
        public bool IsSnapToTickEnabled
        {
            get { return GetValue(IsSnapToTickEnabledProperty); }
            set { SetValue(IsSnapToTickEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the interval between tick marks.
        /// </summary>
        public double TickFrequency
        {
            get { return GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates where to draw 
        /// tick marks in relation to the track.
        /// </summary>
        public TickPlacement TickPlacement
        {
            get { return GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _decreaseButtonPressDispose?.Dispose();
            _decreaseButtonReleaseDispose?.Dispose();
            _increaseButtonSubscription?.Dispose();
            _increaseButtonReleaseDispose?.Dispose();
            _pointerMovedDispose?.Dispose();

            _decreaseButton = e.NameScope.Find<Button>("PART_DecreaseButton");
            _track = e.NameScope.Find<Track>("PART_Track");
            _increaseButton = e.NameScope.Find<Button>("PART_IncreaseButton");

            if (_track != null)
            {
                _track.IsThumbDragHandled = true;
            }

            if (_decreaseButton != null)
            {
                _decreaseButtonPressDispose = _decreaseButton.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
                _decreaseButtonReleaseDispose = _decreaseButton.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
            }

            if (_increaseButton != null)
            {
                _increaseButtonSubscription = _increaseButton.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
                _increaseButtonReleaseDispose = _increaseButton.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
            }

            _pointerMovedDispose = this.AddDisposableHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);
        }

        private void TrackMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                MoveToPoint(e.GetCurrentPoint(_track));
            }
        }

        private void TrackReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }

        private void TrackPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                MoveToPoint(e.GetCurrentPoint(_track));
                _isDragging = true;
            }
        }

        private void MoveToPoint(PointerPoint x)
        {
            var orient = Orientation == Orientation.Horizontal;

            var pointDen = orient ? _track.Bounds.Width : _track.Bounds.Height;
            // Just add epsilon to avoid NaN in case 0/0
            pointDen += double.Epsilon;

            var pointNum = orient ? x.Position.X : x.Position.Y;
            var logicalPos = MathUtilities.Clamp(pointNum / pointDen, 0.0d, 1.0d);
            var invert = orient ? 0 : 1;
            var calcVal = Math.Abs(invert - logicalPos);
            var range = Maximum - Minimum;
            var finalValue = calcVal * range + Minimum;

            Value = IsSnapToTickEnabled ? SnapToTick(finalValue) : finalValue;
        }

        protected override void UpdateDataValidation<T>(AvaloniaProperty<T> property, BindingValue<T> value)
        {
            if (property == ValueProperty)
            {
                DataValidationErrors.SetError(this, value.Error);
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == OrientationProperty)
            {
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<Orientation>());
            }
        }

        /// <summary>
        /// Called when user start dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnThumbDragStarted(VectorEventArgs e)
        {
            _isDragging = true;
        }

        /// <summary>
        /// Called when user stop dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnThumbDragCompleted(VectorEventArgs e)
        {
            _isDragging = false;
        }

        /// <summary>
        /// Snap the input 'value' to the closest tick.
        /// </summary>
        /// <param name="value">Value that want to snap to closest Tick.</param>
        private double SnapToTick(double value)
        {
            if (IsSnapToTickEnabled)
            {
                double previous = Minimum;
                double next = Maximum;

                // This property is rarely set so let's try to avoid the GetValue
                var ticks = Ticks;

                // If ticks collection is available, use it.
                // Note that ticks may be unsorted.
                if ((ticks != null) && (ticks.Count > 0))
                {
                    for (int i = 0; i < ticks.Count; i++)
                    {
                        double tick = ticks[i];
                        if (MathUtilities.AreClose(tick, value))
                        {
                            return value;
                        }

                        if (MathUtilities.LessThan(tick, value) && MathUtilities.GreaterThan(tick, previous))
                        {
                            previous = tick;
                        }
                        else if (MathUtilities.GreaterThan(tick, value) && MathUtilities.LessThan(tick, next))
                        {
                            next = tick;
                        }
                    }
                }
                else if (MathUtilities.GreaterThan(TickFrequency, 0.0))
                {
                    previous = Minimum + (Math.Round(((value - Minimum) / TickFrequency)) * TickFrequency);
                    next = Math.Min(Maximum, previous + TickFrequency);
                }

                // Choose the closest value between previous and next. If tie, snap to 'next'.
                value = MathUtilities.GreaterThanOrClose(value, (previous + next) * 0.5) ? next : previous;
            }

            return value;
        }


        private void UpdatePseudoClasses(Orientation o)
        {
            PseudoClasses.Set(":vertical", o == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
        }
    }
}
