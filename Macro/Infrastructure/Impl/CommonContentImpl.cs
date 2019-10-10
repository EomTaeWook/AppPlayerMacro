using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Serialize;
using Macro.Models;
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
                configView.CurrentRemove();

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
        public override void SaveDataBind(List<IBaseEventTriggerModel> saves)
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
        public override IEnumerable<IBaseEventTriggerModel> GetEnumerator()
        {
            return configView.TriggerSaves;
        }
        public override async Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel)
        {
            if (processEventTriggerModel.Token.IsCancellationRequested)
                return null;
            var nextModel = await TriggerProcess(saveModel as EventTriggerModel, processEventTriggerModel);
            return nextModel.Item2;
        }
        protected override void CaptureImage(Bitmap bmp)
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
        private void Init()
        {
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }
            Clear();
        }
    }
}
