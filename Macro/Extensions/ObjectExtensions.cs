using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Unity;
using Utils;

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
                CornerRadius = source.CornerRadius
            };
            return item;
        }

        public static bool Remove(this ObservableCollection<EventTriggerModel> collection, int key)
        {
            foreach(var item in collection)
            {
                if(item.Index == key)
                {
                    collection.Remove(item);
                    return true;
                }
            }
            return false;
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
                        action().ContinueWith((task) =>
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
        public static int ToLParam(this Point point)
        {
            return (int)point.X & 0xFFFF | ((int)point.Y << 0x10);
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
    }
}
