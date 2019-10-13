using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using Utils.Document;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.View
{
    public partial class GameContentView : BaseContentView
    {
        private Bitmap _bitmap;
        private Rect _hpRoiPosition;
        private Rect _mpRoiPosition;
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
            return Task.CompletedTask;
        }

        public override IEnumerable<IBaseEventTriggerModel> GetEnumerator()
        {
            throw new NotImplementedException();
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

        public override void SaveDataBind(List<IBaseEventTriggerModel> saves)
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
        public override async Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel)
        {
            if (processEventTriggerModel.Token.IsCancellationRequested)
                return null;
            var nextModel = await TriggerProcess(saveModel as GameEventTriggerModel, processEventTriggerModel);
            return nextModel.Item2;
        }
        public override bool Validate(IBaseEventTriggerModel model, out Message error)
        {
            error = Message.Success;

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
