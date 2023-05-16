namespace Zeus
{
    public class ResetZone : ZoneEventData
    {
        public override void Initialize()
        {
            ZoneDataManager.Get().ZoneInitializedCallback += CheckCondition;
        }

        public override void CheckCondition()
        {
            if (!_zoneData.CharacterClear && CheckBeforeZoneClear())
                base.CheckCondition();
            else
                ZoneDataManager.Get().ZoneInitializedCallback -= CheckCondition;
        }

        bool CheckBeforeZoneClear()
        {
            foreach (var data in _zoneData.BeforeZones)
            {
                if (TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs.Contains(data.Info.ZoneID))
                    return true;
            }
            return false;
        }
    }
}
