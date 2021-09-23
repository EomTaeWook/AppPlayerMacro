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
        private CacheModel _commonCacheData;
        private CacheModel _gameCacheData;
        private int _atomic = 0;
        private readonly Dictionary<ulong, EventTriggerModel> _indexTriggerModels;
        private readonly Dictionary<ulong, GameEventTriggerModel> _indexGameTriggerModels;
        public CacheDataManager()
        {
            _indexTriggerModels = new Dictionary<ulong, EventTriggerModel>();
            _indexGameTriggerModels = new Dictionary<ulong, GameEventTriggerModel>();

            _commonCacheData = new CacheModel(0);

            _gameCacheData = new CacheModel(0);

            NotifyHelper.EventTriggerInserted += NotifyHelper_EventTriggerInserted;

            NotifyHelper.EventTriggerRemoved += NotifyHelper_EventTriggerRemoved;
        }

        public bool CheckAndMakeCacheFile(List<EventTriggerModel> saves, string path)
        {
            var isNewCreated = false;
            var isExists = File.Exists(path);
            if (isExists && saves.Count > 0)
            {
                var bytes = File.ReadAllBytes(path);
                _commonCacheData = ObjectSerializer.DeserializeObject<CacheModel>(bytes).FirstOrDefault();
            }
            else
            {
                isNewCreated = true;
            }
            foreach (var save in saves)
            {
                MakeIndexTriggerModel(save);
                InsertIndexTriggerModel(save);
            }
            UpdateCacheData(path, _commonCacheData);
            return isNewCreated;
        }
        public bool CheckAndMakeCacheFile(List<GameEventTriggerModel> saves, string path)
        {
            var isNewCreated = false;
            var isExists = File.Exists(path);
            if (isExists && saves.Count > 0)
            {
                var bytes = File.ReadAllBytes(path);
                _commonCacheData = ObjectSerializer.DeserializeObject<CacheModel>(bytes).FirstOrDefault();
            }
            else
            {
                isNewCreated = true;
            }
            foreach (var save in saves)
            {
                MakeIndexTriggerModel(save);
                InsertIndexTriggerModel(save);
            }

            UpdateCacheData(path, _gameCacheData);
            return isNewCreated;
        }
        public void UpdateCacheData(string path, CacheModel cacheModel)
        {
            cacheModel.LatestCheckDateTime = DateTime.Now.Ticks;
            var bytes = ObjectSerializer.SerializeObject(cacheModel);
            File.WriteAllBytes(path, bytes);
        }
        public bool IsUpdated()
        {
            return TimeSpan.FromTicks(DateTime.Now.Ticks - _commonCacheData.LatestCheckDateTime).TotalDays > 1;
        }
        public ulong GetMaxIndex()
        {
            return _commonCacheData.MaxIndex;
        }
        public ulong IncreaseIndex(CacheModel cacheModel)
        {
            if(Interlocked.Exchange(ref _atomic, 1) == 0)
            {
                cacheModel.MaxIndex++;
                Interlocked.Exchange(ref _atomic, 0);
            }
            return cacheModel.MaxIndex;
        }

        public EventTriggerModel GetEventTriggerModel(ulong index)
        {
            if(_indexTriggerModels.ContainsKey(index))
            {
                return _indexTriggerModels[index];
            }
            else
            {
                return null;
            }
        }
        public GameEventTriggerModel GetGameEventTriggerModel(ulong index)
        {
            if (_indexGameTriggerModels.ContainsKey(index))
            {
                return _indexGameTriggerModels[index];
            }
            else
            {
                return null;
            }
        }

        private void NotifyHelper_EventTriggerRemoved(EventTriggerEventArgs obj)
        {
            if(obj.TriggerModel is GameEventTriggerModel)
            {
                RemoveIndexTriggerModel(obj.TriggerModel as GameEventTriggerModel);
            }
            else if(obj.TriggerModel is EventTriggerModel)
            {
                RemoveIndexTriggerModel(obj.TriggerModel as EventTriggerModel );
            }
        }

        private void NotifyHelper_EventTriggerInserted(EventTriggerEventArgs obj)
        {
            if (obj.TriggerModel is GameEventTriggerModel)
            {
                InsertIndexTriggerModel(obj.TriggerModel as GameEventTriggerModel);
            }
            else if (obj.TriggerModel is EventTriggerModel)
            {
                InsertIndexTriggerModel(obj.TriggerModel as EventTriggerModel);
            }
        }
        public void MakeIndexTriggerModel<T>(T model) where T : BaseEventTriggerModel<T>
        {
            CacheModel cacheModel = null;
            if(model is GameEventTriggerModel)
            {
                cacheModel = _gameCacheData;
            }
            else if(model is EventTriggerModel)
            {
                cacheModel = _commonCacheData;
            }

            if (model.TriggerIndex == 0)
            {
                model.TriggerIndex = IncreaseIndex(cacheModel);
            }
            else if (model.TriggerIndex > cacheModel.MaxIndex)
            {
                cacheModel.MaxIndex = model.TriggerIndex;
            }
            foreach (var child in model.SubEventTriggers)
            {
                MakeIndexTriggerModel(child);
            }
        }

        private void InsertIndexTriggerModel(GameEventTriggerModel model)
        {
            if (_indexTriggerModels.ContainsKey(model.TriggerIndex) == false)
            {
                _indexGameTriggerModels.Add(model.TriggerIndex, model);
            }
            foreach (var child in model.SubEventTriggers)
            {
                InsertIndexTriggerModel(child);
            }
        }

        private void InsertIndexTriggerModel(EventTriggerModel model)
        {
            if (_indexTriggerModels.ContainsKey(model.TriggerIndex) == false)
            {
                _indexTriggerModels.Add(model.TriggerIndex, model);
            }
            foreach (var child in model.SubEventTriggers)
            {
                InsertIndexTriggerModel(child);
            }
        }
        
        private void RemoveIndexTriggerModel(EventTriggerModel model)
        {
            if (_indexTriggerModels.ContainsKey(model.TriggerIndex))
            {
                _indexTriggerModels.Remove(model.TriggerIndex);
            }
            foreach (var child in model.SubEventTriggers)
            {
                RemoveIndexTriggerModel(child);
            }
        }
        private void RemoveIndexTriggerModel(GameEventTriggerModel model)
        {
            if (_indexGameTriggerModels.ContainsKey(model.TriggerIndex))
            {
                _indexGameTriggerModels.Remove(model.TriggerIndex);
            }
            foreach (var child in model.SubEventTriggers)
            {
                RemoveIndexTriggerModel(child);
            }
        }
    }
}
