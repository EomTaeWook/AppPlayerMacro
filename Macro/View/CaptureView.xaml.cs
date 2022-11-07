﻿using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;
using Utils.Infrastructure;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.View
{
    /// <summary>
    /// CaptureView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CaptureView : Window
    {
        private bool _isDrag;
        private Point _originPoint;
        private readonly MonitorInfo _monitorInfo;
        private readonly Border _dummyCaptureBorder;
        private readonly Border _dummyRoiBorder;
        private Border _dragBorder;
        private Point _factor;

        private CaptureModeType _captureMode;

        public CaptureView(MonitorInfo monitorInfo)
        {
            _monitorInfo = monitorInfo;

            _dummyCaptureBorder = new Border
            {
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightBlue,
                SnapsToDevicePixels = true,
                Opacity = 1,
                CornerRadius = new CornerRadius(1)
            };
            _dummyRoiBorder = new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightPink,
                SnapsToDevicePixels = true,
                Opacity = 1,
                CornerRadius = new CornerRadius(1)
            };

            var systemDPI = NativeHelper.GetSystemDPI();
            _factor.X = 1.0F * monitorInfo.Dpi.X / systemDPI.X;
            _factor.Y = 1.0F *monitorInfo.Dpi.Y / systemDPI.Y;
            InitializeComponent();
            Loaded += CaptureView_Loaded;
        }

        public void ShowActivate(CaptureModeType captureModeType)
        {
            this._captureMode = captureModeType;
            SetPosition();
            Show();
            Activate();
            Clear();
        }
        private void CaptureView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {
            Clear();

#if !DEBUG
            Topmost = true;
#endif

            SetPosition();
        }
        private void SetPosition()
        {
            Left = _monitorInfo.Rect.Left;
            Width = _monitorInfo.Rect.Width;
            Top = _monitorInfo.Rect.Top;
            Height = _monitorInfo.Rect.Height;
        }
        private void Clear()
        {
            captureZone.Children.Clear();
            if(this._captureMode == CaptureModeType.ImageCapture)
            {
                _dragBorder = _dummyCaptureBorder.Clone();
            }
            else if(this._captureMode == CaptureModeType.ROICapture)
            {
                _dragBorder = _dummyRoiBorder.Clone();
            }
            
            captureZone.Children.Add(_dragBorder);
            WindowState = WindowState.Normal;
        }
        private void EventInit()
        {
            captureZone.MouseLeftButtonDown += CaptureZone_MouseLeftButtonDown;
            captureZone.MouseMove += CaptureZone_MouseMove;
            captureZone.MouseLeave += CaptureZone_MouseLeave;
            captureZone.MouseLeftButtonUp += CaptureZone_MouseLeave;

            PreviewKeyDown += CaptureView_PreviewKeyDown;
        }
        private void CaptureView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                e.Handled = true;
                if(this._captureMode == CaptureModeType.ImageCapture)
                {
                    NotifyHelper.InvokeNotify(NotifyEventType.ScreenCaptureDataBind, new CaptureEventArgs()
                    {
                        MonitorInfo = _monitorInfo,
                        CaptureImage = null
                    });
                }
                else
                {
                    NotifyHelper.InvokeNotify(NotifyEventType.ROICaptureDataBind, new ROICaptureEventArgs()
                    {
                        MonitorInfo = _monitorInfo,
                        RoiRect = null
                    });
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private void CaptureZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = true;
            _originPoint = e.GetPosition(captureZone);
            e.Handled = true;
        }
        private void CaptureZone_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrag && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(captureZone);
                UpdateDragSelectionRect(_originPoint, currentPoint);
                e.Handled = true;
            }
        }
        private void UpdateDragSelectionRect(Point origin, Point current)
        {
            if (origin.X - current.X > 0)
            {
                Canvas.SetLeft(_dragBorder, current.X);
            }
            else
            {
                Canvas.SetLeft(_dragBorder, origin.X);
            }
                

            if (origin.Y - current.Y > 0)
            {
                Canvas.SetTop(_dragBorder, current.Y);
            }
            else
            {
                Canvas.SetTop(_dragBorder, origin.Y);
            }
                

            if (current.X > origin.X)
            {
                _dragBorder.Width = current.X - origin.X;
            }
            else
            {
                _dragBorder.Width = origin.X - current.X;
            }
                

            if (current.Y > origin.Y)
            {
                _dragBorder.Height = current.Y - origin.Y;
            }
            else
            {
                _dragBorder.Height = origin.Y - current.Y;
            }
        }

        private void CaptureZone_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDrag && IsVisible)
            {
                WindowState = WindowState.Minimized;
                int left = (int)(Canvas.GetLeft(_dragBorder) * _factor.X);
                int top = (int)(Canvas.GetTop(_dragBorder) * _factor.Y);
                int width = (int)(_dragBorder.Width *  _factor.X);
                int height = (int)(_dragBorder.Height * _factor.Y);
                var rect = new Rect
                {
                    Left = left,
                    Right = width + left,
                    Bottom = top + height,
                    Top = top
                };
                if(_captureMode == CaptureModeType.ImageCapture)
                {
                    var image = DisplayHelper.Capture(_monitorInfo, rect);
                    NotifyHelper.InvokeNotify(NotifyEventType.ScreenCaptureDataBind, new CaptureEventArgs()
                    {
                        MonitorInfo = _monitorInfo,
                        CaptureImage = image,
                        Position = rect
                    });
                }
                else if(_captureMode == CaptureModeType.ROICapture)
                {
                    NotifyHelper.InvokeNotify(NotifyEventType.ROICaptureDataBind, new ROICaptureEventArgs()
                    {
                        MonitorInfo = _monitorInfo,
                        RoiRect = rect
                    });
                }
                
                e.Handled = true;
            }
            _isDrag = false;
        }
    }
}
