using ControlzEx.Standard;
using Kosher.Coroutine;
using Kosher.Coroutine.Generic;
using Kosher.Coroutine.Interface;
using Kosher.Log;
using Macro.Extensions;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using Utils.Document;
using Utils.Extensions;
using Utils.Infrastructure;
using Point = System.Windows.Point;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.Infrastructure.Controller
{
    public class ContentController
    {
        private readonly SeedRandom _random;
        private ContentView _contentView;
        private ApplicationDataHelper _applicationDataHelper;
        private InputManager _inputManager;
        private int _delay = 0;
        private ProcessConfigModel _processConfig;
        private CancellationTokenSource _cts;
        private CancellationToken _token;
        private CoroutineWoker coroutineWoker = new CoroutineWoker();

        public ContentController()
        {
            _random = new SeedRandom();
            _applicationDataHelper = ServiceProviderManager.Instance.GetService<ApplicationDataHelper>();
            _inputManager = ServiceProviderManager.Instance.GetService<InputManager>();

            SetConfig(ServiceProviderManager.Instance.GetService<Config>());

            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.UpdatedTime += NotifyHelper_UpdatedTime;
        }

        private void NotifyHelper_UpdatedTime(UpdatedTimeArgs obj)
        {
            coroutineWoker.WorksUpdate(obj.DeltaTime);
        }

        private void SetConfig(Config config)
        {
            _processConfig = new ProcessConfigModel()
            {
                ItemDelay = config.ItemDelay,
                SearchImageResultDisplay = config.SearchImageResultDisplay,
                Similarity = config.Similarity,
                DragDelay = config.DragDelay
            };
            _delay = config.Period;
        }
        private void NotifyHelper_ConfigChanged(ConfigEventArgs obj)
        {
            SetConfig(obj.Config);
        }
        
        public void SetContentView(ContentView baseContentView)
        {
            this._contentView = baseContentView;
        }

        public bool Validate(EventTriggerModel model, out Message error)
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
        public void Start()
        {
            coroutineWoker.Start(Running());
        }
        private IEnumerator Running()
        {
            List<EventTriggerModel> models = new List<EventTriggerModel>();
            models.AddRange(_contentView.eventConfigView.GetDataContext().TriggerSaves);

            while (true)
            {
                foreach (var model in models)
                {
                    var result = ProcessEvents(model, _processConfig);
                    if (result.Current is float)
                    {
                        yield return result.Current;
                    }
                    else if(result.Current is ProcessResultModel)
                    {

                    }
                }

                yield return _delay;
            }
        }
        //public async Task ProcessStart()
        //{
        //    List<EventTriggerModel> models = new List<EventTriggerModel>();
        //    models.AddRange(_contentView.eventConfigView.GetDataContext().TriggerSaves);

        //    while (_token.IsCancellationRequested == false)
        //    {
        //        foreach (var model in models)
        //        {
        //             await TriggerProcess(model, _processConfig);
        //        }
        //        await Task.Delay(_delay);
        //    }
        //}
        public void Stop()
        {
            coroutineWoker.StopAll();
        }
        
        
        internal IEnumerator ProcessEvents(EventTriggerModel model, ProcessConfigModel processConfigModel)
        {
            var resultModel = new ProcessResultModel()
            {
                Excuted = false,
            };
            var hWnd = IntPtr.Zero;
            var processDatas = Process.GetProcessesByName(model.ProcessInfo.ProcessName);
            var applciationData = _applicationDataHelper.Find(model.ProcessInfo.ProcessName) ?? new ApplicationDataModel();

            for (int i = 0; i < processDatas.Length; ++i)
            {
                var factorModel = CalculateFactor(processDatas[i].MainWindowHandle, model, applciationData.IsDynamic);

                if (string.IsNullOrEmpty(applciationData.HandleName))
                {
                    hWnd = processDatas[i].MainWindowHandle;
                }
                else
                {
                    var item = NativeHelper.GetChildHandles(processDatas[i].MainWindowHandle).Where(r => r.Item1.Equals(applciationData.HandleName)).FirstOrDefault();

                    hWnd = item != null ? item.Item2 : processDatas[i].MainWindowHandle;
                }

                if(DisplayHelper.ProcessCapture(processDatas[i], out Bitmap bmp, applciationData.IsDynamic) == false)
                {
                    yield return null;
                    continue;
                }

                var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factorModel.Factor.FactorX),
                                                            (int)Math.Truncate(model.Image.Height * factorModel.Factor.FactorY));

                if (model.RepeatInfo.RepeatType == RepeatType.Search && model.SubEventTriggers.Count > 0)
                {
                    var count = model.RepeatInfo.Count;
                    while (count-- > 0)
                    {
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
                        LogHelper.Debug($"RepeatType[Search : {count}] : >>>> Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        this._contentView.DrawCaptureImage(bmp);

                        if(similarity > processConfigModel.Similarity)
                        {
                            break;
                        }

                        yield return model.AfterDelay / 1000F;

                        for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                        {
                            var result = ProcessEvents(model.SubEventTriggers[ii], processConfigModel);

                            yield return result.Current;
                        }

                        if (DisplayHelper.ProcessCapture(processDatas[i], out bmp, applciationData.IsDynamic) == false)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if(model.SameImageDrag == true)
                    {
                        //Todo
                        for (int ii = 0; ii < model.MaxSameImageCount; ++ii)
                        {
                            var locations = OpenCVHelper.MultipleSearch(bmp, targetBmp, processConfigModel.Similarity, 2, processConfigModel.SearchImageResultDisplay);
                            if (locations.Count > 1)
                            {
                                this._contentView.DrawCaptureImage(bmp);
                                var startPoint = new Point(locations[0].X + targetBmp.Width / 2, locations[0].Y + targetBmp.Height / 2);

                                startPoint.X += this.GetRandomValue(0, targetBmp.Width / 2);
                                startPoint.Y += this.GetRandomValue(0, targetBmp.Height / 2);

                                var endPoint = new Point(locations[1].X + targetBmp.Width / 2, locations[1].Y + targetBmp.Width / 2);
                                endPoint.X += this.GetRandomValue(0, targetBmp.Width / 2);
                                endPoint.Y += this.GetRandomValue(0, targetBmp.Height / 2);

                                SameImageMouseDragTriggerProcess(hWnd, startPoint, endPoint, model, factorModel.PositionFactor, processConfigModel);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
                        LogHelper.Debug($"Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        this._contentView.DrawCaptureImage(bmp);

                        if (similarity <= processConfigModel.Similarity)
                        {
                            yield return model.AfterDelay / 1000F;
                            continue;
                        }

                        if (model.RepeatInfo.RepeatType == RepeatType.Count || model.RepeatInfo.RepeatType == RepeatType.Once)
                        {
                            for (int ii = 0; ii < model.RepeatInfo.Count; ++ii)
                            {
                                yield return model.AfterDelay / 1000F;

                                for (int iii = 0; iii < model.SubEventTriggers.Count; ++iii)
                                {
                                    var result = ProcessEvents(model.SubEventTriggers[iii], processConfigModel);

                                    yield return result.Current;
                                }
                            }
                        }
                        else if (model.RepeatInfo.RepeatType == RepeatType.NoSearch)
                        {
                            while (true)
                            {
                                yield return model.AfterDelay / 1000F;

                                var isExcuted = false;
                                for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                                {
                                    var childResult = ProcessEvents(model.SubEventTriggers[ii], processConfigModel);
                                    if (childResult.Current is float)
                                    {
                                        yield return childResult.Current;
                                    }
                                    else if (childResult.Current is ProcessResultModel result)
                                    {
                                        if (result.Excuted == false)
                                        {
                                            isExcuted = result.Excuted;
                                            break;
                                        }
                                    }
                                }

                                if (isExcuted == false)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (model.EventType == EventType.Mouse)
                            {
                                location.X = applciationData.OffsetX;
                                location.Y = applciationData.OffsetY;
                                MouseTriggerProcess(hWnd, location, model, factorModel.PositionFactor, processConfigModel);
                            }
                            else if (model.EventType == EventType.Image)
                            {
                                var percentageX = _random.NextDouble();
                                var percentageY = _random.NextDouble();

                                location.X = ((location.X + applciationData.OffsetX) / factorModel.PositionFactor.FactorX) + (targetBmp.Width / factorModel.PositionFactor.FactorX * percentageX);
                                location.Y = ((location.Y + applciationData.OffsetY) / factorModel.PositionFactor.FactorY) + (targetBmp.Height / factorModel.PositionFactor.FactorY * percentageY);
                                ImageTriggerProcess(hWnd, location, model);
                            }
                            else if (model.EventType == EventType.RelativeToImage)
                            {
                                location.X = ((location.X + applciationData.OffsetX) / factorModel.PositionFactor.FactorX) + (targetBmp.Width / factorModel.PositionFactor.FactorX / 2);
                                location.Y = ((location.Y + applciationData.OffsetY) / factorModel.PositionFactor.FactorY) + (targetBmp.Height / factorModel.PositionFactor.FactorY / 2);
                                ImageTriggerProcess(hWnd, location, model);
                            }
                            else if (model.EventType == EventType.Keyboard)
                            {
                                KeyboardTriggerProcess(processDatas[i].MainWindowHandle, model);
                            }

                            yield return model.AfterDelay / 1000F;

                            resultModel.Excuted = true;

                            if (model.EventToNext > 0 && model.TriggerIndex != model.EventToNext)
                            {
                                var nextModel = CacheDataManager.Instance.GetEventTriggerModel(model.EventToNext);

                                if (nextModel != null)
                                {
                                    LogHelper.Debug($">>>>Next Move Event : CurrentIndex [ {model.TriggerIndex} ] NextIndex [ {nextModel.TriggerIndex} ] ");

                                    resultModel.NextExcuteModel = nextModel;
                                }
                                yield return resultModel;
                            }
                        }
                    }
                }
            }

            yield return processConfigModel.ItemDelay / 1000F;

            yield return resultModel;
        }






        //public async Task<Tuple<bool, EventTriggerModel>> TriggerProcess(EventTriggerModel model, ProcessConfigModel processConfigModel)
        //{
        //    var isExcute = false;
        //    var hWnd = IntPtr.Zero;

        //    var processDatas = Process.GetProcessesByName(model.ProcessInfo.ProcessName);
        //    var applciationData = _applicationDataHelper.Find(model.ProcessInfo.ProcessName) ?? new ApplicationDataModel();
        //    for (int i = 0; i < processDatas.Length; ++i)
        //    {
        //        var factor = CalculateFactor(processDatas[i].MainWindowHandle, model, applciationData.IsDynamic);

        //        if (string.IsNullOrEmpty(applciationData.HandleName))
        //        {
        //            hWnd = processDatas[i].MainWindowHandle;
        //        }
        //        else
        //        {
        //            var item = NativeHelper.GetChildHandles(processDatas[i].MainWindowHandle).Where(r => r.Item1.Equals(applciationData.HandleName)).FirstOrDefault();

        //            hWnd = item != null ? item.Item2 : processDatas[i].MainWindowHandle;
        //        }

        //        if (model.RepeatInfo.RepeatType == RepeatType.Search && model.SubEventTriggers.Count > 0)
        //        {
        //            var count = model.RepeatInfo.Count;
        //            while (DisplayHelper.ProcessCapture(processDatas[i], out Bitmap bmp, applciationData.IsDynamic) && count-- > 0)
        //            {
        //                var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Factor.FactorX), (int)Math.Truncate(model.Image.Height * factor.Factor.FactorY));
        //                var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
        //                LogHelper.Debug($"RepeatType[Search : {count}] : >>>> Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
        //                this._contentView.DrawCaptureImage(bmp);

        //                if(await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, _token) == false || similarity > processConfigModel.Similarity)
        //                {
        //                    break;
        //                }

        //                for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
        //                {
        //                    await TriggerProcess(model.SubEventTriggers[ii], processConfigModel);

        //                    if(_token.IsCancellationRequested == true)
        //                    {
        //                        break;
        //                    }
        //                }

        //                factor = CalculateFactor(processDatas[i].MainWindowHandle, model, applciationData.IsDynamic);
        //            }
        //        }
        //        else
        //        {
        //            var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.FactorX), (int)Math.Truncate(model.Image.Height * factor.FactorY));

        //            if (model.SameImageDrag == true)
        //            {
        //                if (DisplayHelper.ProcessCapture(processDatas[i], out Bitmap bmp, applciationData.IsDynamic))
        //                {
        //                    //Todo
        //                    for (int ii = 0; ii < model.MaxSameImageCount; ++ii)
        //                    {
        //                        var locations = OpenCVHelper.MultipleSearch(bmp, targetBmp, processConfigModel.Similarity, 2, processConfigModel.SearchImageResultDisplay);
        //                        if (locations.Count > 1)
        //                        {
        //                            this._contentView.DrawCaptureImage(bmp);
        //                            var startPoint = new Point(locations[0].X + targetBmp.Width / 2, locations[0].Y + targetBmp.Height / 2);

        //                            startPoint.X += this.GetRandomValue(0, targetBmp.Width / 2);
        //                            startPoint.Y += this.GetRandomValue(0, targetBmp.Height / 2);

        //                            var endPoint = new Point(locations[1].X + targetBmp.Width / 2, locations[1].Y + targetBmp.Width / 2);
        //                            endPoint.X += this.GetRandomValue(0, targetBmp.Width / 2);
        //                            endPoint.Y += this.GetRandomValue(0, targetBmp.Height / 2);

        //                            SameImageMouseDragTriggerProcess(hWnd, startPoint, endPoint, model, factor.Item2, processConfigModel);
        //                        }
        //                        else
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            else if (DisplayHelper.ProcessCapture(processDatas[i], out Bitmap bmp, applciationData.IsDynamic))
        //            {
        //                var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
        //                LogHelper.Debug($"Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
        //                if (model.SameImageDrag == false)
        //                {
        //                    this._contentView.DrawCaptureImage(bmp);
        //                }
        //                if (similarity > processConfigModel.Similarity)
        //                {
        //                    if (model.SubEventTriggers.Count > 0)
        //                    {
        //                        if (model.RepeatInfo.RepeatType == RepeatType.Count || model.RepeatInfo.RepeatType == RepeatType.Once)
        //                        {
        //                            for (int ii = 0; ii < model.RepeatInfo.Count; ++ii)
        //                            {
        //                                if (await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, _token) == true)
        //                                {
        //                                    break;
        //                                }

        //                                for (int iii = 0; iii < model.SubEventTriggers.Count; ++iii)
        //                                {
        //                                    await TriggerProcess(model.SubEventTriggers[iii], processConfigModel);

        //                                    if (_token.IsCancellationRequested == true)
        //                                    {
        //                                        break;
        //                                    }

        //                                }
        //                            }
        //                        }
        //                        else if (model.RepeatInfo.RepeatType == RepeatType.NoSearch)
        //                        {
        //                            while (await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, _token) == true)
        //                            {
        //                                isExcute = false;
        //                                for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
        //                                {
        //                                    var childResult = await TriggerProcess(model.SubEventTriggers[ii], processConfigModel);
        //                                    if (_token.IsCancellationRequested)
        //                                    {
        //                                        break;
        //                                    }
        //                                    if (isExcute == false && childResult.Item1)
        //                                    {
        //                                        isExcute = childResult.Item1;
        //                                    }
        //                                }
        //                                if (!isExcute)
        //                                {
        //                                    break;
        //                                }

        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        isExcute = true;
        //                        if (model.EventType == EventType.Mouse)
        //                        {
        //                            location.X = applciationData.OffsetX;
        //                            location.Y = applciationData.OffsetY;
        //                            MouseTriggerProcess(hWnd, location, model, factor.Item2, processConfigModel);
        //                        }
        //                        else if (model.EventType == EventType.Image)
        //                        {
        //                            var percentageX = _random.NextDouble();
        //                            var percentageY = _random.NextDouble();

        //                            location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 * percentageX);
        //                            location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item1 * percentageY);
        //                            ImageTriggerProcess(hWnd, location, model);
        //                        }
        //                        else if (model.EventType == EventType.RelativeToImage)
        //                        {
        //                            location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 / 2);
        //                            location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item2 / 2);
        //                            ImageTriggerProcess(hWnd, location, model);
        //                        }
        //                        else if (model.EventType == EventType.Keyboard)
        //                        {
        //                            KeyboardTriggerProcess(processDatas[i].MainWindowHandle, model);
        //                        }

        //                        if (await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, _token) == false)
        //                        {
        //                            break;
        //                        }

        //                        if (model.EventToNext > 0 && model.TriggerIndex != model.EventToNext)
        //                        {
        //                            EventTriggerModel nextModel = null;

        //                            nextModel = CacheDataManager.Instance.GetEventTriggerModel(model.EventToNext);

        //                            if (nextModel != null)
        //                            {
        //                                LogHelper.Debug($">>>>Next Move Event : CurrentIndex [ {model.TriggerIndex} ] NextIndex [ {nextModel.TriggerIndex} ] ");
        //                                return Tuple.Create(isExcute, nextModel);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    await TaskHelper.TokenCheckDelayAsync(processConfigModel.ItemDelay, _token);

        //    return Tuple.Create<bool, EventTriggerModel>(isExcute, null);
        //}

        private void KeyboardTriggerProcess(IntPtr hWnd, EventTriggerModel model)
        {
            var hWndActive = NativeHelper.GetForegroundWindow();
            Task.Delay(100).Wait();
            NativeHelper.SetForegroundWindow(hWnd);
            var inputs = model.KeyboardCmd.ToUpper().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var modifiedKey = inputs.Where(r =>
            {
                if (Enum.TryParse($"{r}", out KeyCode keyCode))
                    return keyCode.IsExtendedKey();
                return false;
            }).Select(r =>
            {
                Enum.TryParse($"{r}", out KeyCode keyCode);
                return keyCode;
            }).ToArray();

            var command = new List<char>();
            foreach (var input in inputs)
            {
                if (Enum.TryParse(input, out KeyCode keyCode))
                {
                    if (!keyCode.IsExtendedKey())
                    {
                        for (int i = 0; i < input.Count(); i++)
                        {
                            command.Add(input[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < input.Count(); i++)
                    {
                        command.Add(input[i]);
                    }
                        
                }
            }
            var keys = command.Where(r =>
            {
                if (Enum.TryParse($"KEY_{r}", out KeyCode keyCode))
                    return !keyCode.IsExtendedKey();
                return false;
            }).Select(r =>
            {
                Enum.TryParse($"KEY_{r}", out KeyCode keyCode);
                return keyCode;
            }).ToArray();

            _inputManager.Keyboard.ModifiedKeyStroke(modifiedKey, keys);
            Task.Delay(100).Wait();
            LogHelper.Debug($">>>>Keyboard Event");
            NativeHelper.SetForegroundWindow(hWndActive);
        }

        private int GetRandomValue(int minValue, int maxValue)
        {
            var random = new SeedRandom();
            var choice = random.Next(0, 2);
            if(choice == 0)
            {
                return -random.Next(minValue, maxValue);
            }
            else
            {
                return random.Next(minValue, maxValue);
            }
        }
        private void MouseTriggerProcess(IntPtr hWnd, Point location, EventTriggerModel model, FactorModel factor, ProcessConfigModel config)
        {
            var mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + (model.MouseTriggerInfo.StartPoint.X + location.X) * -1) * factor.FactorX,
                Y = Math.Abs(model.ProcessInfo.Position.Top + (model.MouseTriggerInfo.StartPoint.Y + location.Y) * -1) * factor.FactorY
            };

            if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.LeftClick)
            {
                LogHelper.Debug($">>>>LMouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");

                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
            }
            else if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.RightClick)
            {
                LogHelper.Debug($">>>>RMouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.RButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.RButtonDown, 0, mousePosition.ToLParam());
            }
            else if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.Drag)
            {
                LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(10).Wait();
                for (int i = 0; i < model.MouseTriggerInfo.MiddlePoint.Count; ++i)
                {
                    mousePosition = new Point()
                    {
                        X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.MiddlePoint[i].X * -1) * factor.FactorX,
                        Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.MiddlePoint[i].Y * -1) * factor.FactorY
                    };
                    NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                    Task.Delay(config.DragDelay).Wait();
                }
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.EndPoint.X * -1) * factor.FactorX,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.EndPoint.Y * -1) * factor.FactorY
                };
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                Task.Delay(10).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
                LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.EndPoint.X} Save Position Y : {model.MouseTriggerInfo.EndPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
            }
            else if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.Wheel)
            {
                LogHelper.Debug($">>>>Wheel Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                //NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                //Task.Delay(100).Wait();
                //NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
                //NativeHelper.PostMessage(hWnd, WindowMessage.MouseWheel, ObjectExtensions.MakeWParam((uint)WindowMessage.MKControl, (uint)(model.MouseTriggerInfo.WheelData * -1)), 0);
                //var hwnd = NativeHelper.FindWindowEx(NativeHelper.FindWindow(null, "Test.txt - 메모장"), IntPtr.Zero, "Edit", null);
                //var p = new System.Drawing.Point(0, 0);
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseWheel, ObjectExtensions.MakeWParam(0, model.MouseTriggerInfo.WheelData * ConstHelper.WheelDelta), mousePosition.ToLParam());
            }
        }
        private void SameImageMouseDragTriggerProcess(IntPtr hWnd,
                                                    Point start,
                                                    Point arrive,
                                                    EventTriggerModel model,
                                                    FactorModel factor,
                                                    ProcessConfigModel config)
        {
            LogHelper.Debug($">>>>Same Drag Mouse Start Target X : { arrive.X } Target Y : { arrive.Y }");
            var interval = 3;
            var middlePoints = this.GetIntevalDragMiddlePoint(start, arrive, interval);

            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, start.ToLParam());
            Task.Delay(10).Wait();

            Point mousePosition;
            for (int i = 0; i < middlePoints.Count; ++i)
            {
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + middlePoints[i].X * -1) * factor.FactorX,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + middlePoints[i].Y * -1) * factor.FactorY
                };
                LogHelper.Debug($">>>>Same Drag Move Mouse Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                Task.Delay(config.DragDelay).Wait();
            }
            mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + arrive.X * -1) * factor.FactorX,
                Y = Math.Abs(model.ProcessInfo.Position.Top + arrive.Y * -1) * factor.FactorY
            };
            NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
            Task.Delay(10).Wait();
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
            LogHelper.Debug($">>>>Same Drag End Mouse Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
        }
        private void ImageTriggerProcess(IntPtr hWnd,
                                        Point location,
                                        EventTriggerModel model)
        {
            var position = new Point()
            {
                X = location.X + model.MouseTriggerInfo.StartPoint.X,
                Y = location.Y + model.MouseTriggerInfo.StartPoint.Y
            };

            LogHelper.Debug($">>>>Image Location X : {position.X} Location Y : {position.Y}");

            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, position.ToLParam());
            Task.Delay(100).Wait();
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, position.ToLParam());

        }
        protected EventFactorModel CalculateFactor(IntPtr hWnd, EventTriggerModel model, bool isDynamic)
        {
            var currentPosition = new Rect();
            NativeHelper.GetWindowRect(hWnd, ref currentPosition);
            var factor = NativeHelper.GetSystemDPI();
            var factorX = 1.0F;
            var factorY = 1.0F;
            var positionFactorX = 1.0F;
            var positionFactorY = 1.0F;
            if (isDynamic)
            {
                foreach (var monitor in DisplayHelper.MonitorInfo())
                {
                    if (monitor.Rect.IsContain(currentPosition))
                    {
                        factorX = factor.X * factorX / model.MonitorInfo.Dpi.X;
                        factorY = factor.Y * factorY / model.MonitorInfo.Dpi.Y;

                        if (model.EventType == EventType.Mouse)
                        {
                            positionFactorX = positionFactorX * monitor.Dpi.X / model.MonitorInfo.Dpi.X;
                            positionFactorY = positionFactorY * monitor.Dpi.Y / model.MonitorInfo.Dpi.Y;
                        }
                        else
                        {
                            positionFactorX = positionFactorX * factor.X / monitor.Dpi.X;
                            positionFactorY = positionFactorY * factor.Y / monitor.Dpi.Y;
                        }
                        break;
                    }
                }
            }
            return new EventFactorModel()
            {
                Factor = new FactorModel() 
                {
                    FactorX = factorX,
                    FactorY = factorY,
                },
                PositionFactor = new FactorModel() 
                {
                    FactorX = positionFactorX,
                    FactorY = positionFactorY
                }
            };
        }
        private List<Point> GetIntevalDragMiddlePoint(Point start, Point arrive, int interval)
        {
            List<Point> middlePosition = new List<Point>();

            Point recent = new Point(start.X, start.Y);
            middlePosition.Add(recent);

            while (Point.Subtract(recent, arrive).Length > interval)
            {
                LogHelper.Debug($">>> Get Middle Interval Drag Mouse : {Point.Subtract(recent, arrive).Length}");
                double middleX;
                if (recent.X > arrive.X)
                {
                    middleX = recent.X - interval;
                }
                else if (recent.X < arrive.X)
                {
                    middleX = recent.X + interval;
                }
                else
                {
                    middleX = recent.X;
                }

                double middleY;
                if (recent.Y > arrive.Y)
                {
                    middleY = recent.Y - interval;
                }
                else if (recent.Y < arrive.Y)
                {
                    middleY = recent.Y + interval;
                }
                else
                {
                    middleY = recent.Y;
                }

                recent = new Point(middleX, middleY);
                middlePosition.Add(recent);
            }

            return middlePosition;
        }
    }

}
