namespace Zeus
{
    public class CheckClearZone : ZoneEventData
    {
        public override void Initialize()
        {
            ZoneDataManager.Get().ZoneClearCallback += CheckCondition;
        }

        public override void CheckCondition()
        {
            if (!_zoneData.CharacterClear) return;

            if (!CheckInterlockingZoneClear())
                base.CheckCondition();
            ZoneDataManager.Get().ZoneClearCallback -= CheckCondition;
        }

        bool CheckInterlockingZoneClear()
        {
            foreach (var data in _zoneData.AfterZones)
            {
                if (TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs.Contains(data.Info.ZoneID))
                    return true;
            }
            return false;
        }
    }
}
