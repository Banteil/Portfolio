using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public enum TypeFailed
    {
        NONE,                   // 알 수 없음

        COOLDOWN,               // 쿨다운

        NOT_FOUND_CHARACTER,    // 캐릭터 찾지 못함
        NOT_FOUND_TABLE_ID,     // 테이블에서 해당 아이디의 데이터를 찾지 못함
        NOT_FOUNT_SAVEDATA,     // 세이브데이터를 불러오지 못함

        NOT_ENOUGH_AMOUNT,      // 수량 부족
    }

    //public delegate void FailedEvent(object sender, TypeFailed messageType);

    [CreateAssetMenu(menuName = "Zeus/Events/Failed Event")]

    public class FailedEventSO : GameEventSO<object, TypeFailed> { }
}