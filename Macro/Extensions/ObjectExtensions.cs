using Macro.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Unity;
using Utils;
using Point = System.Windows.Point;

namespace Macro.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<T> FindChildren<T>(this DependencyObject source) where T : DependencyObject
        {
            if (source != null)
            {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs)
                {
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    foreach (T descendant in FindChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
        private static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
        {
            if (parent == null)
                yield break;
            if (parent is ContentElement || parent is FrameworkElement)
            {
                foreach (object obj in LogicalTreeHelper.GetChildren(parent))
                {
                    if (obj is DependencyObject dep)
                        yield return (DependencyObject)obj;
                }
            }
            else
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }

        public static MessageDialogResult MessageShow(this MetroWindow @object, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return @object.ShowModalMessageExternal(title,
                                                message,
                                                style,
                                                new MetroDialogSettings()
                                                {
                                                    ColorScheme = MetroDialogColorScheme.Inverted,
                                                });
        }
        public static Border Clone(this Border source)
        {
            var item = new Border
            {
                BorderBrush = source.BorderBrush,
                BorderThickness = source.BorderThickness,
                Background = source.Background,
                Opacity = source.Opacity,
                CornerRadius = source.CornerRadius,
                SnapsToDevicePixels = source.SnapsToDevicePixels
            };
            return item;
        }
        
        public static T GetInstance<T>()
        {
            if(Singleton<UnityContainer>.Instance.IsRegistered<T>())
            {
                return Singleton<UnityContainer>.Instance.Resolve<T>();
            }
            else
            {
                return Singleton<T>.Instance;
            }
        }

        public static Task ProgressbarShow(this MetroWindow @object, Func<Task> action)
        {
            return Task.Factory.StartNew(() =>
            {
                @object.Dispatcher.Invoke(() =>
                {
                    var progress = new ProgressView
                    {
                        Owner = @object,
                        Left = @object.Left / 2,
                        Width = @object.Width / 2,
                        Top = @object.Top / 2,
                        Height = @object.Height / 2
                    };
                    progress.Loaded += (s, e) =>
                    {
                        action().ContinueWith(task =>
                        {
                            progress.Dispatcher.Invoke(() => {
                                progress.Close();
                            });
                        });
                    };
                    progress.ShowDialog();
                });
            });
        }

        public static int MakeWParam(int low, int high) => (low & 0xFFFF) | (high << 16);

        public static int ToLParam(this Point point) => (int)point.X & 0xFFFF | ((int)point.Y << 0x10);

        public static Bitmap Resize(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(source, 0, 0, width, height);
            }
            return bmp;
        }
        public static T TryFindFromPoint<T>(this UIElement element, Point point) where T : class
        {
            if (!(element.InputHitTest(point) is DependencyObject @object))
                return default(T);
            else if (@object is T)
                return @object as T;
            else
                return TryFindParent<T>(@object);
        }
        private static T TryFindParent<T>(DependencyObject child) where T : class
        {
            var parentObject = GetParentObject(child);
            if (parentObject == null)
                return null;
            if (parentObject is T)
                return parentObject as T;
            else
                return TryFindParent<T>(parentObject);
        }
        private static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null)
                return null;
            if (child is ContentElement contentElement)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null)
                    return parent;
                return contentElement is FrameworkContentElement ce ? ce.Parent : null;
            }
            return VisualTreeHelper.GetParent(child);
        }
        public static void Swap<T>(this ObservableCollection<T> collection, int index1, int index2)
        {
            var temp = collection[index1];
            collection[index1] = collection[index2];
            collection[index2] = temp;
        }
        public static T GetSelectItemFromObject<T>(this ItemsControl control, object item) where T : ItemsControl
        {
            if (control.ItemContainerGenerator.ContainerFromItem(item) is T target)
                return target;
            for (int i=0; i< control.Items.Count; ++i)
            {
                if (control.ItemContainerGenerator.ContainerFromIndex(i) is T subControl)
                {
                    target = GetSelectItemFromObject<T>(subControl, item);
                    if (target != null)
                    {
                        return target;
                    }
                }
            }
            return null;
        }
        public static T DataContext<T>(this FrameworkElement control) where T : class
        {
            if (control.DataContext is T context)
                return context;
            else
                return null;
        }
        public static T DataContext<T>(this ItemsControl control) where T : class
        {
            if(control.DataContext is T context)
                return context;
            else
                return null;
        }
    }
}
