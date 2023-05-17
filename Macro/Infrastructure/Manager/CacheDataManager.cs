using Kosher.Framework;
using Macro.Models;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Windows.Controls;

namespace Macro.Infrastructure.Manager
{
    public class CacheDataManager : Singleton<CacheDataManager>
    {
        private ulong _maxIndex;
        private int _atomic = 0;
        private readonly Dictionary<ulong, EventTriggerModel> _indexTriggerModelToMap;
        private readonly Dictionary<object, object> _cacheDataToMap = new Dictionary<object, object>();
        public CacheDataManager()
        {
            _indexTriggerModelToMap = new Dictionary<ulong, EventTriggerModel>();

            NotifyHelper.EventTriggerInserted += NotifyHelper_EventTriggerInserted;
            NotifyHelper.EventTriggerRemoved += NotifyHelper_EventTriggerRemoved;
        }

        public void InitMaxIndex(List<EventTriggerModel> eventTriggerDatas)
        {
            _indexTriggerModelToMap.Clear();
            foreach (var item in eventTriggerDatas)
            {
                if(item.TriggerIndex > _maxIndex)
                {
                    _maxIndex = item.TriggerIndex;
                }
            }
        }

        public ulong IncreaseIndex()
        {
            if(Interlocked.Exchange(ref _atomic, 1) == 0)
            {
                _maxIndex++;
                Interlocked.Exchange(ref _atomic, 0);
            }
            return _maxIndex;
        }

        public EventTriggerModel GetEventTriggerModel(ulong index)
        {
            if(_indexTriggerModelToMap.ContainsKey(index))
            {
                return _indexTriggerModelToMap[index];
            }
            else
            {
                return null;
            }
        }

        private void NotifyHelper_EventTriggerRemoved(EventTriggerEventArgs obj)
        {
            RemoveIndexTriggerModel(obj.TriggerModel);
        }

        private void NotifyHelper_EventTriggerInserted(EventTriggerEventArgs obj)
        {
            InsertIndexTriggerModel(obj.TriggerModel);
        }
        public void MakeIndexTriggerModel(EventTriggerModel model)
        {
            model.TriggerIndex = IncreaseIndex();

            foreach (var child in model.SubEventTriggers)
            {
                MakeIndexTriggerModel(child);
            }
        }

        private void InsertIndexTriggerModel(EventTriggerModel model)
        {
            if (_indexTriggerModelToMap.ContainsKey(model.TriggerIndex) == false)
            {
                _indexTriggerModelToMap.Add(model.TriggerIndex, model);
            }
            foreach (var child in model.SubEventTriggers)
            {
                InsertIndexTriggerModel(child);
            }
        }
        
        private void RemoveIndexTriggerModel(EventTriggerModel model)
        {
            if (_indexTriggerModelToMap.ContainsKey(model.TriggerIndex))
            {
                _indexTriggerModelToMap.Remove(model.TriggerIndex);
            }
            foreach (var child in model.SubEventTriggers)
            {
                RemoveIndexTriggerModel(child);
            }
        }
        public void AddData(object key, object value)
        {
            if(_cacheDataToMap.ContainsKey(key) == false)
            {
                _cacheDataToMap.Add(key, value);
                return;
            }
            _cacheDataToMap[key] = value;
        }
        public object GetData(object key)
        {
            _cacheDataToMap.TryGetValue(key, out object value);

            return value;
        }
        public T GetData<T>(object key)
        {
            if(_cacheDataToMap.TryGetValue(key, out object value) == false)
            {
                return default;
            }
            return (T)value;
        }
        public void DeleteData(object key)
        {
            _cacheDataToMap.Remove(key);
        }
    }
}
