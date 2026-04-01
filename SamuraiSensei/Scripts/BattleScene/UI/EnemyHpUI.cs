using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    Transform target;
    public Transform Target 
    { 
        set 
        { 
            target = value;
            enemy = target.GetComponent<Enemy>();
        }
    }
    Enemy enemy;

    [SerializeField]
    Image hpGauge;
    [SerializeField]
    Image logo;
       

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = target.position;
        pos.y += enemy.SR.sprite.rect.height * 0.01f;
        transform.position = pos;
        hpGauge.fillAmount = enemy.HP / enemy.Info.maxHP;
    }

    public void SetBossInfo(int enemyID)
    {
        if (logo == null) return;
        logo.sprite = Resources.Load<Sprite>("Sprites/UI/Logo/Boss/" + enemyID + "_BossLogo");
    }
}
