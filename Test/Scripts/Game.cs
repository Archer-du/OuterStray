using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;

public class Game : MonoBehaviour,IDBattle
{
    Battle m_battle;

    [SerializeField]
    DBattle_Soldier m_battle_soldier_prefab;

    IDBattle_Soldier IDBattle.CreateDisplay_Soldier(Battle_Soldier soldier)
    {
        var s = GameObject.Instantiate(m_battle_soldier_prefab);
        s.Soldier_data = soldier;
        return s;
    }

    // Start is called before the first frame update
    void Start()
    {

        var player1 = new BattlePlayer();
        player1.cardConfigs.Add(new SoldierConfig());

        var player2 = new BattlePlayer();
        player2.cardConfigs.Add(new SoldierConfig());
        player2.cardConfigs.Add(new SoldierConfig());

        m_battle = new Battle(this,player1,player2);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.S))
        {
            //MoveAction moveAction = new MoveAction() { player_index = 1, Target= m_battle.get from = 2, to = 3 };
            //m_battle.AddInput(moveAction);
        }

        m_battle.Update();
    }
}
