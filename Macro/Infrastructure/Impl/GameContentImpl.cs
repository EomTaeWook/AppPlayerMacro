using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Manager;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Utils;
using Utils.Document;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.View
{
    public partial class GameContentView : BaseContentView
    {
        private Bitmap _bitmap;

        private RoiPositionModel _hpRoiPosition;
        private RoiPositionModel _mpRoiPosition;
        private Process _currentProcess;
        public void Capture(CaptureViewMode captureViewMode)
        {
            Clear();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            foreach (var item in _captureViews)
            {
                item.Setting(captureViewMode);
                item.ShowActivate();
            }
        }
        public override void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            btnAddSameContent.Visibility = Visibility.Collapsed;
            _bitmap = null;
            canvasCaptureImage.Background = System.Windows.Media.Brushes.White;
            gameConfigView.Clear();
        }

        public override Task Delete(object state)
        {
            if (state is string path)
            {
                gameConfigView.RemoveCurrentItem();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    using (var fs = new FileStream(path, FileMode.CreateNew))
                    {
                        foreach (var data in gameConfigView.DataContext<Models.ViewModel.GameEventConfigViewModel>().TriggerSaves)
                        {
                            var bytes = ObjectSerializer.SerializeObject(data);
                            fs.Write(bytes, 0, bytes.Count());
                        }
                        fs.Close();
                    }
                }
            }
            return Task.CompletedTask;
        }

        public override IEnumerable<IBaseEventTriggerModel> GetEnumerator()
        {
            return gameConfigView.TriggerSaves;
        }

        public override Task Save(object state)
        {
            if (state is string path)
            {
                gameConfigView.InsertCurrentItem();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    var saves = (gameConfigView.DataContext as Models.ViewModel.GameEventConfigViewModel).TriggerSaves;
                    foreach (var data in saves)
                    {
                        var bytes = ObjectSerializer.SerializeObject(data);
                        fs.Write(bytes, 0, bytes.Count());
                    }
                    fs.Close();
                }
            }
            return Task.CompletedTask;
        }
        public override Task Load(object state)
        {
            if(state is SaveFileLoadModel model)
            {
                try
                {
                    var saveFiles = ObjectSerializer.DeserializeObject<GameEventTriggerModel>(File.ReadAllBytes(model.SaveFilePath));

                    if (ObjectExtensions.GetInstance<CacheDataManager>().CheckAndMakeCacheFile(saveFiles, model.CacheFilePath))
                    {
                        Save(saveFiles);
                    }
                    SaveDataBind(saveFiles);
                }
                catch (Exception ex)
                {
                    File.Delete(model.SaveFilePath);
                    LogHelper.Warning(ex);
                    Task.FromException(new FileLoadException(DocumentHelper.Get(Message.FailedLoadSaveFile)));
                }
            }
            
            return Task.CompletedTask;
        }
        public override async Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel)
        {
            if (processEventTriggerModel.Token.IsCancellationRequested)
                return null;
            int hpPercent = 100;
            int mpPercent = 100;
            Dispatcher.Invoke(() =>
            {
                if(_hpRoiPosition != null)
                {
                    var lower = Tuple.Create(_colorDatas["HP"].Lower.R, _colorDatas["HP"].Lower.G, _colorDatas["HP"].Lower.B);

                    var upper = Tuple.Create(_colorDatas["HP"].Upper.R, _colorDatas["HP"].Upper.G, _colorDatas["HP"].Upper.B);

                    foreach(var process in processEventTriggerModel.Processes)
                    {
                        CheckPercentageImage(_hpRoiPosition, process, lower, upper);
                    }
                }
                
            });

            var nextModel = await TriggerProcess(saveModel as GameEventTriggerModel, processEventTriggerModel);
            return nextModel.Item2;
        }

        public override bool Validate(IBaseEventTriggerModel model, out Message error)
        {
            error = Message.Success;

            if (model is GameEventTriggerModel == false)
            {
                error = Message.FailedInvalidateData;
                return false;
            }

            model.KeyboardCmd = model.KeyboardCmd.Replace(" ", "");

            if (model.EventType == EventType.Image || model.EventType == EventType.RelativeToImage || model.IsImageSearchRequired == true)
            {
                if (model.Image == null)
                {
                    error = Message.FailedImageValidate;
                    return false;
                }
            }
            if (model.EventType == EventType.Mouse && model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.None)
            {
                error = Message.FailedMouseCoordinatesValidate;
                return false;
            }

            if (string.IsNullOrEmpty(model.KeyboardCmd) && model.EventType == EventType.Keyboard)
            {
                error = Message.FailedKeyboardCommandValidate;
                return false;
            }

            if (string.IsNullOrEmpty(model.ProcessInfo.ProcessName))
            {
                error = Message.FailedProcessValidate;
                return false;
            }
#if !DEBUG
            var gameModel = model as GameEventTriggerModel;
            if (gameModel.HpCondition == null)
            {
                error = Message.FailedHpConditionValidate;
                return false;
            }
            else if (gameModel.MpCondition == null)
            {
                error = Message.FailedMpConditionValidate;
                return false;
            }
#endif
            return true;
        }

        private int CheckPercentageImage(RoiPositionModel roiPositionModel, Process process, Tuple<double, double, double> lower, Tuple<double, double, double> upper)
        {
            var applciationData = ObjectExtensions.GetInstance<ApplicationDataManager>().Find(process.ProcessName) ?? new ApplicationDataModel();
            if (DisplayHelper.ProcessCapture(process, out Bitmap bitmap, applciationData.IsDynamic))
            {
                var processPosition = new Rect();
                NativeHelper.GetWindowRect(process.MainWindowHandle, ref processPosition);

                var factor = NativeHelper.GetSystemDPI();
                var factorX = 1.0F * factor.X / ConstHelper.DefaultDPI;
                var factorY = 1.0F * factor.Y / ConstHelper.DefaultDPI;

                var realLeft = roiPositionModel.RoiPosition.Left - ((processPosition.Left - roiPositionModel.MonitorInfo.Rect.Left));
                var realTop = roiPositionModel.RoiPosition.Top - (processPosition.Top - roiPositionModel.MonitorInfo.Rect.Top);

                var realWidth = (int)Math.Truncate(roiPositionModel.RoiPosition.Width * factorX);
                var realHeight = (int)Math.Truncate(roiPositionModel.RoiPosition.Height * factorY);

                //var realWidth = realLeft + roiPositionModel.RoiPosition.Width;
                //var realHeight = realTop + roiPositionModel.RoiPosition.Height;


                var roi = new Rect()
                {
                    Left = 327,
                    Right = 1563,
                    Top = 548,
                    Bottom = 773,
                };
                
                var roiBitmap = OpenCVHelper.MakeRoiImage(bitmap, roi);

                roiBitmap.Save("crop.jpg");

                canvasCaptureImage.Background = new ImageBrush(roiBitmap.ToBitmapSource());

                var percent = OpenCVHelper.SearchImagePercentage(roiBitmap, lower, upper);

            }
            return 0;
        }
        private void SaveDataBind(List<GameEventTriggerModel> saves)
        {
            Dispatcher.Invoke(() =>
            {
                (gameConfigView.DataContext as Models.ViewModel.GameEventConfigViewModel).TriggerSaves.Clear();
                foreach (var item in saves)
                {
                    (gameConfigView.DataContext as Models.ViewModel.GameEventConfigViewModel).TriggerSaves.Add(item as GameEventTriggerModel);
                }
            });
        }
        private void Init()
        {
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }

            var colorDatas = JsonHelper.Load<List<ColorModel>>($@"{ConstHelper.DefaultDatasFilePath}\ColorData.json");

            _colorDatas = colorDatas.ToDictionary(k => k.Code, r => r);
            Clear();
        }
    }
}
