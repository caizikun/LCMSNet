﻿using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using LcmsNet.ViewModels;
using ReactiveUI;

namespace LcmsNet.Views
{
    /// <summary>
    /// Interaction logic for PopoutView.xaml
    /// </summary>
    public partial class PopoutView : UserControl
    {
        public PopoutView()
        {
            InitializeComponent();
        }

        #region Button Positioning

        // Using a DependencyProperty as the backing store for HorizontalButtonAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalButtonAlignmentProperty =
            DependencyProperty.Register("HorizontalButtonAlignment", typeof(HorizontalAlignment), typeof(PopoutView), new PropertyMetadata(HorizontalAlignment.Left, UpdatePositioning));

        // Using a DependencyProperty as the backing store for VerticalButtonAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalButtonAlignmentProperty =
            DependencyProperty.Register("VerticalButtonAlignment", typeof(VerticalAlignment), typeof(PopoutView), new PropertyMetadata(VerticalAlignment.Bottom, UpdatePositioning));

        // Using a DependencyProperty as the backing store for OverlayButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverlayButtonProperty =
            DependencyProperty.Register("OverlayButton", typeof(bool), typeof(PopoutView), new PropertyMetadata(false, UpdatePositioning));

        // Using a DependencyProperty as the backing store for ButtonGridRow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonGridRowProperty =
            DependencyProperty.Register("ButtonGridRow", typeof(int), typeof(PopoutView), new PropertyMetadata(2));

        // Using a DependencyProperty as the backing store for ButtonGridColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonGridColumnProperty =
            DependencyProperty.Register("ButtonGridColumn", typeof(int), typeof(PopoutView), new PropertyMetadata(1));

        // Using a DependencyProperty as the backing store for PreferVerticalBorder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreferVerticalBorderProperty =
            DependencyProperty.Register("PreferVerticalBorder", typeof(bool), typeof(PopoutView), new PropertyMetadata(false, UpdatePositioning));

        /// <summary>
        /// Horizontal positioning of the popout button
        /// </summary>
        public HorizontalAlignment HorizontalButtonAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalButtonAlignmentProperty); }
            set { SetValue(HorizontalButtonAlignmentProperty, value); }
        }

        /// <summary>
        /// Vertical positioning of the popout button
        /// </summary>
        public VerticalAlignment VerticalButtonAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalButtonAlignmentProperty); }
            set { SetValue(VerticalButtonAlignmentProperty, value); }
        }

        /// <summary>
        /// If the button should be overlaid on top of the content
        /// </summary>
        public bool OverlayButton
        {
            get { return (bool)GetValue(OverlayButtonProperty); }
            set { SetValue(OverlayButtonProperty, value); }
        }

        /// <summary>
        /// Positioning of the button in the display grid rows. Set internally.
        /// </summary>
        public int ButtonGridRow
        {
            get { return (int)GetValue(ButtonGridRowProperty); }
            private set { SetValue(ButtonGridRowProperty, value); }
        }

        /// <summary>
        /// Positioning of the button in the display grid columns. Set internally.
        /// </summary>
        public int ButtonGridColumn
        {
            get { return (int)GetValue(ButtonGridColumnProperty); }
            private set { SetValue(ButtonGridColumnProperty, value); }
        }

        /// <summary>
        /// When button is not overlaid and placed in a corner, if a vertical border is prefered over a horizontal border
        /// </summary>
        public bool PreferVerticalBorder
        {
            get { return (bool)GetValue(PreferVerticalBorderProperty); }
            set { SetValue(PreferVerticalBorderProperty, value); }
        }

        private static void UpdatePositioning(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PopoutView pv))
            {
                return;
            }
            if (pv.OverlayButton)
            {
                pv.ButtonGridRow = 1;
                pv.ButtonGridColumn = 1;
                return;
            }
            var row = 1;
            var column = 1;
            switch (pv.HorizontalButtonAlignment)
            {
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    column = 1;
                    break;
                case HorizontalAlignment.Right:
                    column = 2;
                    break;
                case HorizontalAlignment.Left:
                default:
                    column = 0;
                    break;
            }
            switch (pv.VerticalButtonAlignment)
            {
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    row = 1;
                    break;
                case VerticalAlignment.Top:
                    row = 0;
                    break;
                case VerticalAlignment.Bottom:
                default:
                    row = 2;
                    break;
            }
            if (row != 1 && column != 1)
            {
                if (pv.PreferVerticalBorder)
                {
                    row = 1;
                }
                else
                {
                    column = 1;
                }
            }
            pv.ButtonGridRow = row;
            pv.ButtonGridColumn = column;
        }

        #endregion

        private void PopoutView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is PopoutViewModel pvm)
            {
                pvm.WhenAnyValue(x => x.Tacked).Where(x => !x).Throttle(TimeSpan.FromMilliseconds(250)).Subscribe(x => PopoutToWindow());
            }
        }

        private void PopoutToWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => PopoutToWindow());
                return;
            }
            if (this.DataContext is PopoutViewModel pvm && !pvm.Tacked)
            {
                // TODO: Doesn't work with WinForms base: PopoutWindow existing = null;
                // TODO: Doesn't work with WinForms base: foreach (var window in Application.Current.Windows)
                // TODO: Doesn't work with WinForms base: {
                // TODO: Doesn't work with WinForms base:     if (window is PopoutWindow pw && pw.Title.Equals(pvm.Title))
                // TODO: Doesn't work with WinForms base:     {
                // TODO: Doesn't work with WinForms base:         existing = pw;
                // TODO: Doesn't work with WinForms base:     }
                // TODO: Doesn't work with WinForms base: }
                // TODO: Doesn't work with WinForms base: if (existing != null)
                // TODO: Doesn't work with WinForms base: {
                // TODO: Doesn't work with WinForms base:     existing.Activate();
                // TODO: Doesn't work with WinForms base:     return;
                // TODO: Doesn't work with WinForms base: }
                var popoutWindow = new PopoutWindow() { DataContext = pvm };
                // move the content to the new window
                var child = this.Content;
                this.Content = null;
                popoutWindow.Content = child;
                popoutWindow.Width = pvm.WindowWidth;
                popoutWindow.Height = pvm.WindowHeight;
                popoutWindow.HorizontalButtonAlignment = this.HorizontalButtonAlignment;
                popoutWindow.VerticalButtonAlignment = this.VerticalButtonAlignment;
                popoutWindow.OverlayButton = this.OverlayButton;
                popoutWindow.PreferVerticalBorder = this.PreferVerticalBorder;
                popoutWindow.Show();
                // When the window closes, move the content back
                popoutWindow.Closed += (o, args) =>
                {
                    var obj = popoutWindow.Content;
                    popoutWindow.Content = null;
                    this.Content = obj;
                };
            }
        }
    }
}
