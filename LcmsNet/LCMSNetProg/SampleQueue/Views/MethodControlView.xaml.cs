﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LcmsNet.SampleQueue.ViewModels;

namespace LcmsNet.SampleQueue.Views
{
    /// <summary>
    /// Interaction logic for MethodControlView.xaml
    /// </summary>
    public partial class MethodControlView : UserControl
    {
        public MethodControlView()
        {
            InitializeComponent();
            if (!CommandsGrid.IsVisible)
            {
                GridButtonRow.Height = new GridLength(0);
            }
        }

        private void UIElement_OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MethodControlViewModel mcvm)
            {
                mcvm.ContainsKeyboardFocus = this.IsKeyboardFocusWithin;
            }
        }

        /// <summary>
        /// Hides the grid row for the commands according to the set visibility.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Grid g)
            {
                if (g.IsVisible)
                {
                    GridButtonRow.Height = new GridLength(70);
                }
                else
                {
                    GridButtonRow.Height = new GridLength(0);
                }
            }
        }
    }
}
