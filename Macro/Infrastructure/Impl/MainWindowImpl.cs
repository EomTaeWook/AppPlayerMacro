using KosherUtils.Coroutine;
using KosherUtils.Log;
using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utils;
using Utils.Extensions;
using Utils.Infrastructure;
using Message = Utils.Document.Message;
using Rect = Utils.Infrastructure.Rect;
using Version = Macro.Infrastructure.Version;

namespace Macro
{
    public partial class MainWindow : MetroWindow
    {

        
        
        
        
 
        
        
        private async Task ProcessStartAsync(object state)
        {
            //if (state is CancellationToken token)
            //{
            //    if (token.IsCancellationRequested == true)
            //        return;

            //    List<EventTriggerModel> models = new List<EventTriggerModel>();
            //    ContentView view = null;
            //    Dispatcher.Invoke(() => {
            //        var selectView = viewMap[(tab_content.SelectedContent as ContentView).Tag.ToString()];
            //        if (selectView != null)
            //        {
            //            models = selectView.View.GetEnumerator().ToList();
            //            view = selectView.View;
            //        }
            //    });

            //    if (view == null)
            //        return;

            //    foreach (var iter in models)
            //    {
            //        await _taskQueue.Enqueue(async () =>
            //        {
            //            await InvokeNextEventTriggerAsync(view, iter, token);
            //        });

            //        if (token.IsCancellationRequested)
            //        {
            //            break;
            //        }
            //    }

            //    await TaskHelper.TokenCheckDelayAsync(_config.Period, token);

            //    await _taskQueue.Enqueue(ProcessStartAsync, token);
            //}
        }
    }
}
