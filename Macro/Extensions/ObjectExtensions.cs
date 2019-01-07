using Macro.Models;
using System.Collections.ObjectModel;
using Unity;
using Utils;

namespace Macro.Extensions
{
    public static class ObjectExtensions
    {
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
    }
}
