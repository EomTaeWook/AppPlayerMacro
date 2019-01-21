using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Macro.UI
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
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }
    }

    public class TreeListViewItem : TreeViewItem
    {
        public TreeViewItem ParentItem { get; set; }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (!(element is TreeListViewItem treeViewItem))
                return;
            treeViewItem.ParentItem = ItemsControlFromItemContainer(element) as TreeListViewItem;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
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

            var level = -1;
            if (targetType == typeof(double) && typeof(DependencyObject).IsAssignableFrom(value.GetType()))
            {
                var element = value as DependencyObject;
                while ((element = VisualTreeHelper.GetParent(element)) != null)
                {
                    if (typeof(TreeViewItem).IsAssignableFrom(element.GetType()))
                        level++;
                }
            }
            return Indentation * level;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
