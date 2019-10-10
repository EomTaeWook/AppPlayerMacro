﻿using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Macro.View
{
    /// <summary>
    /// GameContentVIew.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameContentView : BaseContentView
    {
        private readonly List<CaptureView> _captureViews;
        public GameContentView()
        {
            _captureViews = new List<CaptureView>();

            InitializeComponent();

            InitEvent();
            Init();
        }
        private void InitEvent()
        {
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnAddSameContent.Click += Button_Click;
            btnCapture.Click += Button_Click;
            btnHpCapture.Click += Button_Click;
            btnMpCapture.Click += Button_Click;

            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;

            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
        }
        private void NotifyHelper_SelectTreeViewChanged(SelctTreeViewItemChangedEventArgs e)
        {
            if (e.TreeViewItem == null)
            {
                Clear();
            }
            else
            {
                var model = e.TreeViewItem.DataContext<EventTriggerModel>();
                btnDelete.Visibility = Visibility.Visible;
                btnAddSameContent.Visibility = Visibility.Visible;

                _bitmap = model.Image;
                canvasCaptureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
        }
        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs e)
        {
            if (e.CaptureViewMode == CaptureViewMode.Common || e.CaptureViewMode == CaptureViewMode.Max)
            {
                return;
            }
            foreach (var item in _captureViews)
            {
                item.Hide();
            }
            if (e.CaptureImage == null)
                return;

            var capture = e.CaptureImage;

            if (e.CaptureViewMode == CaptureViewMode.Game)
            {
                canvasCaptureImage.Background = new ImageBrush(capture.ToBitmapSource());
                _bitmap = new Bitmap(capture, capture.Width, capture.Height);
            }
            else if(e.CaptureViewMode == CaptureViewMode.HP)
            {
                _hpRoiPosition = e.Position;
                canvasCaptureHp.Background = new ImageBrush(capture.ToBitmapSource());
            }
           else if(e.CaptureViewMode == CaptureViewMode.Mp)
            {
                _mpRoiPosition = e.Position;
                canvasCaptureMp.Background = new ImageBrush(capture.ToBitmapSource());
            }
            
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            _captureViews.Clear();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture(CaptureViewMode.Game);
            }
            if(btn.Equals(btnHpCapture))
            {
                Capture(CaptureViewMode.HP);
            }
            if(btn.Equals(btnMpCapture))
            {
                Capture(CaptureViewMode.Mp);
            }
            else if (btn.Equals(btnSave))
            {
                var model = gameConfigView.CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                model.Image = _bitmap;
                if (model.EventType == EventType.RelativeToImage)
                {
                    model.MouseTriggerInfo.StartPoint = new Point(gameConfigView.RelativePosition.X, gameConfigView.RelativePosition.Y);
                }

                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if (btn.Equals(btnDelete))
            {
                var model = gameConfigView.CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                NotifyHelper.InvokeNotify(NotifyEventType.Delete, new DeleteEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if (btn.Equals(btnAddSameContent))
            {
                var item = gameConfigView.CopyCurrentItem();
                if (item == null)
                    return;
                var model = item.DataContext<GameEventTriggerModel>();
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
        }
    }
}
