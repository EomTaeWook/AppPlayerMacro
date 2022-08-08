using Macro.Infrastructure.Serialize;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Macro.Infrastructure.Manager
{
    public class CacheDataManager
    {
        private ulong _maxIndex;
        private int _atomic = 0;
        private readonly Dictionary<ulong, EventTriggerModel> _indexTriggerModelToMap;
        public CacheDataManager()
        {
            _indexTriggerModelToMap = new Dictionary<ulong, EventTriggerModel>();

            NotifyHelper.EventTriggerInserted += NotifyHelper_EventTriggerInserted;

            NotifyHelper.EventTriggerRemoved += NotifyHelper_EventTriggerRemoved;
        }

        //public bool CheckAndMakeCacheFile(List<EventTriggerModel> saves, string path)
        //{
        //    var isNewCreated = false;
        //    var isExists = File.Exists(path);
        //    if (isExists && saves.Count > 0)
        //    {
        //        var bytes = File.ReadAllBytes(path);
        //        commonCacheData = ObjectSerializer.DeserializeObject<CacheModel>(bytes).FirstOrDefault();
        //    }
        //    else
        //    {
        //        isNewCreated = true;
        //    }
        //    foreach (var save in saves)
        //    {
        //        MakeIndexTriggerModel(save);
        //        InsertIndexTriggerModel(save);
        //    }
        //    UpdateCacheData(path, commonCacheData);
        //    return isNewCreated;
        //}
        public void UpdateCacheData(string path, CacheModel cacheModel)
        {
            cacheModel.LatestCheckDateTime = DateTime.Now.Ticks;
            var bytes = ObjectSerializer.SerializeObject(cacheModel);
            File.WriteAllBytes(path, bytes);
        }
        //public bool IsUpdated()
        //{
        //    return TimeSpan.FromTicks(DateTime.Now.Ticks - commonCacheData.LatestCheckDateTime).TotalDays > 1;
        //}
        //public ulong GetMaxIndex()
        //{
        //    return commonCacheData.MaxIndex;
        //}
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
            RemoveIndexTriggerModel(obj.TriggerModel as EventTriggerModel);
        }

        private void NotifyHelper_EventTriggerInserted(EventTriggerEventArgs obj)
        {
            InsertIndexTriggerModel(obj.TriggerModel as EventTriggerModel);
        }
        public void MakeIndexTriggerModel(EventTriggerModel model)
        {
            if (model.TriggerIndex == 0)
            {
                model.TriggerIndex = IncreaseIndex();
            }
            else if (model.TriggerIndex > _maxIndex)
            {
                _maxIndex = model.TriggerIndex;
            }
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
    }
}
