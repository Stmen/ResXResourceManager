﻿namespace tomenglertde.ResXManager.View.Behaviors
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;
    using DataGridExtensions;
    using tomenglertde.ResXManager.Model;
    using tomenglertde.ResXManager.View.ColumnHeaders;

    public class ShowErrorsOnlyBehavior : Behavior<DataGrid>
    {
        public ToggleButton ToggleButton
        {
            get { return (ToggleButton)GetValue(ToggleButtonProperty); }
            set { SetValue(ToggleButtonProperty, value); }
        }
        /// <summary>
        /// Identifies the ToggleButton dependency property
        /// </summary>
        public static readonly DependencyProperty ToggleButtonProperty =
            DependencyProperty.Register("ToggleButton", typeof(ToggleButton), typeof(ShowErrorsOnlyBehavior), new FrameworkPropertyMetadata(null, (sender, e) => ((ShowErrorsOnlyBehavior)sender).ToggleButton_Changed((ToggleButton)e.OldValue, (ToggleButton)e.NewValue)));

        protected override void OnAttached()
        {
            base.OnAttached();

            DataGrid.AddHandler(ColumnVisibilityChangedEventBehavior.ColumnVisibilityChangedEvent, (RoutedEventHandler)DataGrid_ColumnVisibilityChanged);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            DataGrid.RemoveHandler(ColumnVisibilityChangedEventBehavior.ColumnVisibilityChangedEvent, (RoutedEventHandler)DataGrid_ColumnVisibilityChanged);
        }

        private DataGrid DataGrid
        {
            get
            {
                return AssociatedObject;
            }
        }

        private void ToggleButton_Changed(ToggleButton oldValue, ToggleButton newValue)
        {
            if (oldValue != null)
            {
                oldValue.Checked -= ToggleButton_StateChanged;
                oldValue.Unchecked -= ToggleButton_StateChanged;
            }

            if (newValue != null)
            {
                newValue.Checked += ToggleButton_StateChanged;
                newValue.Unchecked += ToggleButton_StateChanged;
                ToggleButton_StateChanged(newValue, EventArgs.Empty);
            }
        }

        private void ToggleButton_StateChanged(object sender, EventArgs e)
        {
            if ((sender == null) || (DataGrid == null))
                return;

            var button = (ToggleButton)sender;

            if (button.IsChecked.GetValueOrDefault())
            {
                UpdateErrorsOnlyFilter();
            }
            else
            {
                DataGrid.Items.Filter = null;
                DataGrid.SetIsAutoFilterEnabled(true);
            }
        }

        private void DataGrid_ColumnVisibilityChanged(object source, RoutedEventArgs e)
        {
            if (ToggleButton == null)
                return;

            if (ToggleButton.IsChecked.GetValueOrDefault())
            {
                this.BeginInvoke(UpdateErrorsOnlyFilter);
            }
        }

        private void UpdateErrorsOnlyFilter()
        {
            if (DataGrid == null)
                return;

            var visibleLanguages = DataGrid.Columns
                .Where(column => column.Visibility == Visibility.Visible)
                .Select(column => column.Header)
                .OfType<LanguageHeader>()
                .Select(header => header.CultureKey)
                .ToArray();

            DataGrid.SetIsAutoFilterEnabled(false);
            DataGrid.Items.Filter = row =>
            {
                var entry = (ResourceTableEntry)row;
                var values = visibleLanguages.Select(lang => entry.Values.GetValue(lang));
                return !entry.IsInvariant && values.Any(string.IsNullOrEmpty);
            };
        }
    }
}