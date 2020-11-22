using Macro.Extensions;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Interface;
using Macro.Infrastructure.Manager;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Extensions;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Infrastructure.Controller
{
    public class BaseContentController : IContentController
    {
        private readonly Random _random;
        private BaseContentView baseContentView;
        public BaseContentController()
        {
            _random = new Random();
        }
        public void SetContentView(BaseContentView baseContentView)
        {
            this.baseContentView = baseContentView;
        }
        public async Task<Tuple<bool, IBaseEventTriggerModel>> TriggerProcess<T>(T model, ProcessConfigModel processConfigModel) where T : BaseEventTriggerModel<T>
        {
            var isExcute = false;

            var hWnd = IntPtr.Zero;
            var applciationData = ObjectExtensions.GetInstance<ApplicationDataManager>().Find(model.ProcessInfo.ProcessName) ?? new ApplicationDataModel();
            for (int i = 0; i < processConfigModel.Processes.Count; ++i)
            {
                var factor = CalculateFactor(processConfigModel.Processes[i].MainWindowHandle, model, applciationData.IsDynamic);

                if (string.IsNullOrEmpty(applciationData.HandleName))
                {
                    hWnd = processConfigModel.Processes[i].MainWindowHandle;
                }
                else
                {
                    var item = NativeHelper.GetChildHandles(processConfigModel.Processes[i].MainWindowHandle).Where(r => r.Item1.Equals(applciationData.HandleName)).FirstOrDefault();

                    if (item != null)
                        hWnd = item.Item2;
                    else
                        hWnd = processConfigModel.Processes[i].MainWindowHandle;
                }

                if (model.RepeatInfo.RepeatType == RepeatType.Search && model.SubEventTriggers.Count > 0)
                {
                    var count = model.RepeatInfo.Count;
                    while (DisplayHelper.ProcessCapture(processConfigModel.Processes[i], out Bitmap bmp, applciationData.IsDynamic) && count-- > 0)
                    {
                        var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item1));
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
                        LogHelper.Debug($"RepeatType[Search : {count}] : >>>> Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        this.baseContentView.CaptureImage(bmp);
                        if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, processConfigModel.Token) || similarity > processConfigModel.Similarity)
                            break;
                        for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                        {
                            await TriggerProcess(model.SubEventTriggers[ii], processConfigModel);
                            if (processConfigModel.Token.IsCancellationRequested)
                                break;
                        }
                        factor = CalculateFactor(processConfigModel.Processes[i].MainWindowHandle, model, applciationData.IsDynamic);
                    }
                }
                else
                {
                    var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item2));

                    if (model.SameImageDrag == true)
                    {
                        if (DisplayHelper.ProcessCapture(processConfigModel.Processes[i], out Bitmap bmp, applciationData.IsDynamic))
                        {
                            //Todo
                            for (int ii = 0; ii < model.MaxSameImageCount; ++ii)
                            {
                                var locations = OpenCVHelper.MultipleSearch(bmp, targetBmp, processConfigModel.Similarity, 2, processConfigModel.SearchImageResultDisplay);
                                if (locations.Count > 1)
                                {
                                    this.baseContentView.CaptureImage(bmp);
                                    SameImageMouseDragTriggerProcess(hWnd, locations[0], locations[1], model, factor.Item2);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (DisplayHelper.ProcessCapture(processConfigModel.Processes[i], out Bitmap bmp, applciationData.IsDynamic))
                    {
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, processConfigModel.SearchImageResultDisplay);
                        LogHelper.Debug($"Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        if (model.SameImageDrag == false)
                        {
                            this.baseContentView.CaptureImage(bmp);
                        }
                        if (similarity > processConfigModel.Similarity)
                        {
                            if (model.SubEventTriggers.Count > 0)
                            {
                                if (model.RepeatInfo.RepeatType == RepeatType.Count || model.RepeatInfo.RepeatType == RepeatType.Once)
                                {
                                    for (int ii = 0; ii < model.RepeatInfo.Count; ++ii)
                                    {
                                        if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, processConfigModel.Token))
                                            break;
                                        for (int iii = 0; iii < model.SubEventTriggers.Count; ++iii)
                                        {
                                            await TriggerProcess(model.SubEventTriggers[iii], processConfigModel);
                                            if (processConfigModel.Token.IsCancellationRequested)
                                                break;
                                        }
                                    }
                                }
                                else if (model.RepeatInfo.RepeatType == RepeatType.NoSearch)
                                {
                                    while (await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, processConfigModel.Token))
                                    {
                                        isExcute = false;
                                        for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                                        {
                                            var childResult = await TriggerProcess(model.SubEventTriggers[ii], processConfigModel);
                                            if (processConfigModel.Token.IsCancellationRequested)
                                            {
                                                break;
                                            }
                                            if (isExcute == false && childResult.Item1)
                                            {
                                                isExcute = childResult.Item1;
                                            }
                                        }
                                        if (!isExcute)
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                isExcute = true;
                                if (model.EventType == EventType.Mouse)
                                {
                                    location.X = applciationData.OffsetX;
                                    location.Y = applciationData.OffsetY;
                                    MouseTriggerProcess(hWnd, location, model, factor.Item2, processConfigModel);
                                }
                                else if (model.EventType == EventType.Image)
                                {
                                    var percentageX = _random.NextDouble();
                                    var percentageY = _random.NextDouble();

                                    location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 * percentageX);
                                    location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item1 * percentageY);
                                    ImageTriggerProcess(hWnd, location, model);
                                }
                                else if (model.EventType == EventType.RelativeToImage)
                                {
                                    location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 / 2);
                                    location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item2 / 2);
                                    ImageTriggerProcess(hWnd, location, model);
                                }

                                else if (model.EventType == EventType.Keyboard)
                                {
                                    KeyboardTriggerProcess(processConfigModel.Processes[i].MainWindowHandle, model);
                                }
                                if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, processConfigModel.Token))
                                    break;

                                if (model.EventToNext > 0 && model.TriggerIndex != model.EventToNext)
                                {
                                    IBaseEventTriggerModel nextModel = null;
                                    if (model is GameEventTriggerModel)
                                    {
                                        nextModel = ObjectExtensions.GetInstance<CacheDataManager>().GetGameEventTriggerModel(model.EventToNext);
                                    }
                                    else if (model is EventTriggerModel)
                                    {
                                        nextModel = ObjectExtensions.GetInstance<CacheDataManager>().GetEventTriggerModel(model.EventToNext);
                                    }

                                    if (nextModel != null)
                                    {
                                        LogHelper.Debug($">>>>Next Move Event : CurrentIndex [ {model.TriggerIndex} ] NextIndex [ {nextModel.TriggerIndex} ] ");
                                        return Tuple.Create(isExcute, nextModel);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            await TaskHelper.TokenCheckDelayAsync(processConfigModel.ItemDelay, processConfigModel.Token);
            return Tuple.Create<bool, IBaseEventTriggerModel>(isExcute, null);
        }

        private void KeyboardTriggerProcess(IntPtr hWnd, IBaseEventTriggerModel model)
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
                            command.Add(input[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < input.Count(); i++)
                        command.Add(input[i]);
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

            ObjectExtensions.GetInstance<InputManager>().Keyboard.ModifiedKeyStroke(modifiedKey, keys);
            Task.Delay(100).Wait();
            LogHelper.Debug($">>>>Keyboard Event");
            NativeHelper.SetForegroundWindow(hWndActive);
        }
        private void MouseTriggerProcess(IntPtr hWnd, Point location, IBaseEventTriggerModel model, Tuple<float, float> factor, ProcessConfigModel config)
        {
            var mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + (model.MouseTriggerInfo.StartPoint.X + location.X) * -1) * factor.Item1,
                Y = Math.Abs(model.ProcessInfo.Position.Top + (model.MouseTriggerInfo.StartPoint.Y + location.Y) * -1) * factor.Item2
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
                        X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.MiddlePoint[i].X * -1) * factor.Item1,
                        Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.MiddlePoint[i].Y * -1) * factor.Item2
                    };
                    NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                    Task.Delay(config.DragDelay).Wait();
                }
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.EndPoint.X * -1) * factor.Item1,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.EndPoint.Y * -1) * factor.Item2
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
        private void SameImageMouseDragTriggerProcess(IntPtr hWnd, Point start, Point arrive, IBaseEventTriggerModel model, Tuple<float, float> factor)
        {
            LogHelper.Debug($">>>>Drag Mouse Save Position X : {start.X} Save Position Y : {start.Y} Target X : { arrive.X } Target Y : { arrive.Y }");
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, start.ToLParam());
            var interval = 3;
            var middlePoints = this.GetIntevalDragMiddlePoint(start, arrive, interval);
            Task.Delay(100).Wait();

            Point mousePosition;
            for (int i = 0; i < middlePoints.Count; ++i)
            {
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + middlePoints[i].X * -1) * factor.Item1,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + middlePoints[i].Y * -1) * factor.Item2
                };
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
            }
            mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + arrive.X * -1) * factor.Item1,
                Y = Math.Abs(model.ProcessInfo.Position.Top + arrive.Y * -1) * factor.Item2
            };
            NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
            Task.Delay(100).Wait();
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
            LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.EndPoint.X} Save Position Y : {model.MouseTriggerInfo.EndPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
        }
        private void ImageTriggerProcess(IntPtr hWnd, Point location, IBaseEventTriggerModel model)
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
        protected Tuple<Tuple<float, float>, Tuple<float, float>> CalculateFactor(IntPtr hWnd, IBaseEventTriggerModel model, bool isDynamic)
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
            return Tuple.Create(Tuple.Create(factorX, factorY), Tuple.Create(positionFactorX, positionFactorY));
        }
        private List<Point> GetIntevalDragMiddlePoint(Point start, Point arrive, int interval)
        {
            var middleX = 0D;
            var middleY = 0D;

            List<Point> middlePosition = new List<Point>();

            Point recent = new Point(start.X, start.Y);
            middlePosition.Add(recent);

            while (Point.Subtract(recent, arrive).Length > interval)
            {
                LogHelper.Debug($">>> Get Middle Interval Drag Mouse : {Point.Subtract(recent, arrive).Length}");
                if (recent.X > arrive.X)
                {
                    middleX = recent.X - interval;
                }
                else if (recent.X < arrive.X)
                {
                    middleX = recent.X + interval;
                }

                if (recent.Y > arrive.Y)
                {
                    middleY = recent.Y - interval;
                }
                else if (recent.Y < arrive.Y)
                {
                    middleY = recent.Y + interval;
                }

                recent = new Point(middleX, middleY);
                middlePosition.Add(recent);
            }

            return middlePosition;
        }
    }

}
