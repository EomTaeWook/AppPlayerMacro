using Macro.Extensions;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Serialize;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Utils;

namespace Macro.View
{
    public partial class CommonContentView : BaseContentView
    {
        private Bitmap _bitmap;
        private readonly List<CaptureView> _captureViews = new List<CaptureView>();
        private void Init()
        {
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }
        }
        public override void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            btnAddSameContent.Visibility = Visibility.Collapsed;
            _bitmap = null;
            captureImage.Background = System.Windows.Media.Brushes.White;
            configView.Clear();
        }
        public void Capture()
        {
            Clear();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            foreach (var item in _captureViews)
            {
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
                        foreach (var data in configView.DataContext<Models.ViewModel.ConfigEventViewModel>().TriggerSaves)
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
            configView.InsertCurrentItem();

            if (state is string path)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    var saves = (configView.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves;
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
    }
}
