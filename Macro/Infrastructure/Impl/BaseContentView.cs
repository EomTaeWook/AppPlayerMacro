using System.Threading.Tasks;
using System.Windows.Controls;

namespace Macro.Infrastructure.Impl
{
    public abstract class BaseContentView : UserControl
    {
        public abstract Task Save(object state);

        public abstract Task Delete(object state);

        public abstract void Clear();
    }
}
