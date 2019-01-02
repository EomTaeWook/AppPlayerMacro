using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Macro.Extensions
{
    public static class DependencyObjectExtensions
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

        public static void MessageShow(this MetroWindow @object, string title, string message)
        {
            @object.ShowModalMessageExternal(title,
                                                message,
                                                MessageDialogStyle.Affirmative,
                                                new MetroDialogSettings()
                                                {
                                                    ColorScheme = MetroDialogColorScheme.Inverted,
                                                });
        }
    }
}
