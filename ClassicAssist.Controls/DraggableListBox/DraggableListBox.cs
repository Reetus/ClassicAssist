#region License

// Copyright (C) 2023 Reetus
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
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace ClassicAssist.Controls.DraggableListBox
{
    public class DraggableListBox : ListBox, IDropTarget
    {
        public DraggableListBox()
        {
            DragDrop.SetIsDragSource( this, true );
            DragDrop.SetIsDropTarget( this, true );
            DragDrop.SetDropHandler( this, this );
        }

        public new void DragEnter( IDropInfo dropInfo )
        {
        }

        public new void DragOver( IDropInfo dropInfo )
        {
            Type type = ItemsSource.GetType().GetGenericArguments()[0];

            if ( dropInfo.DragInfo.SourceItem.GetType() != type )
            {
                return;
            }

            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public new void DragLeave( IDropInfo dropInfo )
        {
        }

        public new void Drop( IDropInfo dropInfo )
        {
            Type type = ItemsSource.GetType().GetGenericArguments()[0];

            if ( dropInfo.DragInfo.SourceItem.GetType() != type )
            {
                return;
            }

            if ( dropInfo.TargetItem != null )
            {
                if ( dropInfo.TargetItem.GetType() != type )
                {
                    return;
                }
            }

            object sourceEntry = Convert.ChangeType( dropInfo.DragInfo.SourceItem, type );
            object targetEntry = Convert.ChangeType( dropInfo.TargetItem, type );

            int sourceIndex = ( (IList) ItemsSource ).IndexOf( sourceEntry );
            int targetIndex = ( (IList) ItemsSource ).IndexOf( targetEntry );

            ( (dynamic) ItemsSource ).Move( sourceIndex,
                targetIndex == -1 ? ( (IList) ItemsSource ).Count - 1 : targetIndex );
        }
    }
}