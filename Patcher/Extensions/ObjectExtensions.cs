using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Patcher.Extensions
{
    public static class ObjectExtensions
    {
        public static MessageDialogResult ShowMessageDialog(this MetroWindow @object, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            if(settings == null)
            {
                settings = new MetroDialogSettings()
                {
                    ColorScheme = MetroDialogColorScheme.Inverted,
                    DialogTitleFontSize = 0.1F,
                    MaximumBodyHeight = 500,
                };
            }
            return @object.ShowModalMessageExternal(title,
                                                message,
                                                style,
                                                settings);
        }
    }
}
