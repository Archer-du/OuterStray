using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    //策划配置对象
    public class CardConfig
    {
        public string name;
    }

    public class Battle_Card
    {
        IDBattle_Card m_handler;
    }

    public interface IDBattle_Card
    {
    }


    public class SoldierConfig : CardConfig
    {
        public int attack;
    }

    public class Battle_Soldier : BattleElement
    {
        SoldierConfig m_config;
        public Battle_Soldier(SoldierConfig config)
        {
            m_config = config;
        }

        IDBattle_Soldier m_handler;

        internal void Init(IDBattle_Soldier display_handler)
        {
            m_handler = display_handler;
        }

        public override void Update()
        {
            //可以计数器--，触发普攻
        }

        internal void Move(int x, int y)
        {
            //改变逻辑战线
            //。。。


            //让显示层对象移动
            m_handler.MoveTo(x,y);
        }
    }

    public interface IDBattle_Soldier
    {
        void MoveTo(int x,int y);
    }
}
