using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Macro.View
{
    public class TreeListView : TreeView
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(TreeListView), new UIPropertyMetadata(null));

        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public TreeListView()
        {
            Columns = new GridViewColumnCollection();
            Application.Current.MainWindow.SizeChanged += (s, e) =>
            {
                foreach (var column in Columns)
                {
                    BindingOperations.GetBindingExpression(column, WidthProperty).UpdateTarget();
                }
            };
        }
    }

    internal class TreeListViewExpander : ToggleButton
    {
    }

    internal class TreeListViewConverter : IValueConverter
    {
        private readonly double Indentation = 10.0;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            if (targetType == typeof(double) && typeof(DependencyObject).IsAssignableFrom(value.GetType()))
            {
                DependencyObject element = value as DependencyObject;
                var level = -1;
                while (element != null)
                {
                    element = VisualTreeHelper.GetParent(element);
                    if (typeof(TreeViewItem).IsAssignableFrom(element.GetType()))
                        level++;
                }
                return Indentation * level;
            }
            throw new NotSupportedException(
                string.Format("Cannot convert from <{0}> to <{1}> using <TreeListViewConverter>.",
                value.GetType(), targetType));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This method is not supported.");
        }
    }
}
