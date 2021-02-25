using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Controller;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Interface;
using Macro.Infrastructure.Manager;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Utils;
using Utils.Document;

namespace Macro.View
{
    public partial class CommonContentView : BaseContentView
    {
        private Bitmap _bitmap;
        private IContentController contentController;
        public override void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            btnAddSameContent.Visibility = Visibility.Collapsed;
            _bitmap = null;
            canvasCaptureImage.Background = System.Windows.Media.Brushes.White;
            configView.Clear();
        }
        public void Capture()
        {
            Clear();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            foreach (var item in _captureViews)
            {
                item.Setting(CaptureViewMode.Common);
                item.ShowActivate();
            }
        }
        public override Task Delete(object state)
        {
            if (state is string path)
            {
                configView.RemoveCurrentItem();

                if (File.Exists(path))
                {
                    File.Delete(path);
                    using (var fs = new FileStream(path, FileMode.CreateNew))
                    {
                        foreach (var data in configView.DataContext<Models.ViewModel.CommonEventConfigViewModel>().TriggerSaves)
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
        public override Task Save(object state)
        {
            if (state is string path)
            {
                configView.InsertCurrentItem();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    var saves = (configView.DataContext as Models.ViewModel.CommonEventConfigViewModel).TriggerSaves;
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
                    var saveFiles = ObjectSerializer.DeserializeObject<EventTriggerModel>(File.ReadAllBytes(model.SaveFilePath));

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
        public override IEnumerable<IBaseEventTriggerModel> GetEnumerator()
        {
            return configView.TriggerSaves;
        }
        public override async Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel)
        {
            if (processEventTriggerModel.Token.IsCancellationRequested)
                return null;
            var nextModel = await contentController.TriggerProcess(saveModel as EventTriggerModel, processEventTriggerModel);
            return nextModel.Item2;
        }
        public override void CaptureImage(Bitmap bmp)
        {
            Dispatcher.Invoke(() =>
            {
                canvasCaptureImage.Background = new ImageBrush(bmp.ToBitmapSource());
            });
        }
        public override bool Validate(IBaseEventTriggerModel model, out Message error)
        {
            error = Message.Success;
            
            model.KeyboardCmd = model.KeyboardCmd.Replace(" ", "");

            if (model.Image == null)
            {
                error = Message.FailedImageValidate;
                return false;
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
            
            return true;
        }
        private void SaveDataBind(List<EventTriggerModel> saves)
        {
            Dispatcher.Invoke(() =>
            {
                (configView.DataContext as Models.ViewModel.CommonEventConfigViewModel).TriggerSaves.Clear();
                foreach (var item in saves)
                {
                    (configView.DataContext as Models.ViewModel.CommonEventConfigViewModel).TriggerSaves.Add(item as EventTriggerModel);
                }
            });
        }
        private void Init()
        {
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }
            var controller = new BaseContentController();
            controller.SetContentView(this);
            this.contentController = controller;
            Clear();
        }
    }
}
