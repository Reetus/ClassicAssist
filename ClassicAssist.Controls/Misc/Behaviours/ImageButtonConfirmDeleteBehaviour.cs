#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using Brush = System.Windows.Media.Brush;
using Image = System.Windows.Controls.Image;

namespace ClassicAssist.Controls.Misc.Behaviours
{
    public class ImageButtonConfirmDeleteBehaviour : Behavior<ImageButton>
    {
        private ICommand _deleteCommand;
        private CancellationTokenSource _cancellationTokenSource;

        private DrawingImage _clonedDrawingImage;

        private bool _isPending;
        private Brush _originalBrush;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;

            if ( AssociatedObject.Command == null )
            {
                return;
            }

            _deleteCommand = AssociatedObject.Command;
            AssociatedObject.Command = null;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnClick;
        }

        private void OnClick( object sender, RoutedEventArgs e )
        {
            if ( !_isPending )
            {
                _isPending = true;
                PrepareClone();
                StoreOriginalBrush();
                SetDrawingImageColor( Brushes.Red );
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;
                Task.Delay( 2000, token ).ContinueWith( t =>
                {
                    if ( !t.IsCanceled )
                    {
                        Reset();
                    }
                }, TaskScheduler.Default );
            }
            else
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                if ( _deleteCommand?.CanExecute( AssociatedObject.CommandParameter ) == true )
                {
                    _deleteCommand.Execute( AssociatedObject.CommandParameter );
                }

                _isPending = false;
                _clonedDrawingImage = null;
                _originalBrush = null;
            }

            e.Handled = true;
        }

        private void StoreOriginalBrush()
        {
            if ( !( _clonedDrawingImage?.Drawing is DrawingGroup group ) )
            {
                return;
            }

            if ( _originalBrush == null )
            {
                _originalBrush = group.Children.OfType<GeometryDrawing>().FirstOrDefault( g => g.Brush != null )?.Brush.Clone();
            }
        }

        private void PrepareClone()
        {
            if ( !( AssociatedObject.Content is Image img ) )
            {
                return;
            }

            if ( !( img.Source is DrawingImage original ) )
            {
                return;
            }

            if ( _clonedDrawingImage != null )
            {
                return;
            }

            _clonedDrawingImage = original.CloneCurrentValue();
            img.Source = _clonedDrawingImage;
        }

        private void SetDrawingImageColor( Brush brush )
        {
            if ( !( AssociatedObject.Content is Image img ) )
            {
                return;
            }

            if ( !( img.Source is DrawingImage drawingImage ) )
            {
                return;
            }

            if ( !( drawingImage.Drawing is DrawingGroup group ) )
            {
                return;
            }

            foreach ( GeometryDrawing drawing in group.Children.OfType<GeometryDrawing>() )
            {
                drawing.Brush = brush;
            }
        }

        private void Reset()
        {
            Dispatcher.Invoke( () =>
            {
                if ( _originalBrush == null || _clonedDrawingImage == null )
                {
                    return;
                }

                if ( _clonedDrawingImage.Drawing is DrawingGroup group )
                {
                    foreach ( GeometryDrawing drawing in group.Children.OfType<GeometryDrawing>() )
                    {
                        drawing.Brush = _originalBrush;
                    }
                }

                _isPending = false;
                _clonedDrawingImage = null;
                _originalBrush = null;
            } );
        }
    }
}