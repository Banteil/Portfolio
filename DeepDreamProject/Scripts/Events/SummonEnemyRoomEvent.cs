using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SummonEnemyRoomEvent", menuName = "Data/RoomEventData/SummonEnemyRoomEvent")]
public class SummonEnemyRoomEvent : RoomEventData
{
    public List<GameObject> EnemyPrefabs;
    public int MinSummonCount = 1;
    public int MaxSummonCount = 1;

    public override void InitializationEvent(Room room)
    {
        base.InitializationEvent(room);
        room.CurrentCount = Random.Range(MinSummonCount, MaxSummonCount + 1);
        for (int i = 0; i < room.CurrentCount; i++)
        {
            GameObject enemy = Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Count)], room.BindObjects, false);

            float range_X = room.RoomBoundCollider.bounds.size.x;
            float range_Y = room.RoomBoundCollider.bounds.size.y;

            range_X = Random.Range(((range_X / 2) * -1) + 5f, (range_X / 2) - 5f);
            range_Y = Random.Range(((range_Y / 2) * -1) + 5f, (range_Y / 2) - 5f);
            Vector3 spawnPosition = new Vector3(range_X, range_Y);
            enemy.transform.position = room.transform.position + spawnPosition;

            enemy.GetComponent<Health>().OnDeath += () =>
            {
                room.CurrentCount--;
                if (room.CurrentCount <= 0)
                {
                    room.OpenDoors();
                    room.IsClear = true;
                }
            };
            room.Objects.Add(enemy);
            enemy.SetActive(false);
        }
    }

    public override void MeetingEvent(Room room)
    {
        for (int i = 0; i < room.Objects.Count; i++)
        {
            room.Objects[i].SetActive(true);
        }
    }
}
