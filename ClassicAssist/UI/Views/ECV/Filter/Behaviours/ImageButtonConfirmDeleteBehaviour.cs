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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Controls;
using ClassicAssist.UI.Views.ECV.Filter.Models;
using Microsoft.Xaml.Behaviors;
using Brush = System.Windows.Media.Brush;
using Image = System.Windows.Controls.Image;

namespace ClassicAssist.UI.Views.ECV.Filter.Behaviours
{
    public class ImageButtonConfirmDeleteBehaviour : Behavior<ImageButton>
    {
        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register( nameof( DeleteCommand ), typeof( ICommand ),
            typeof( ImageButtonConfirmDeleteBehaviour ), new PropertyMetadata( default( ICommand ) ) );

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register( nameof( Item ), typeof( EntityCollectionFilterEntry ),
            typeof( ImageButtonConfirmDeleteBehaviour ), new PropertyMetadata( default( EntityCollectionFilterEntry ) ) );

        private DrawingImage _clonedDrawingImage;

        private bool _isPending;
        private DateTime _lastClickTime;
        private Brush _originalBrush;
        private CancellationTokenSource _cancellationTokenSource;

        public ICommand DeleteCommand
        {
            get => (ICommand) GetValue( DeleteCommandProperty );
            set => SetValue( DeleteCommandProperty, value );
        }

        public EntityCollectionFilterEntry Item
        {
            get => (EntityCollectionFilterEntry) GetValue( ItemProperty );
            set => SetValue( ItemProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
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
                _lastClickTime = DateTime.Now;
                _originalBrush = AssociatedObject.Background;
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

                if ( DeleteCommand?.CanExecute( Item ) == true )
                {
                    DeleteCommand.Execute( Item );
                }
                
                _isPending = false;
                _clonedDrawingImage = null;
                _originalBrush = null;
            }
            }
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