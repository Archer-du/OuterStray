using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    //�߻����ö���
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
            //���Լ�����--�������չ�
        }

        internal void Move(int x, int y)
        {
            //�ı��߼�ս��
            //������


            //����ʾ������ƶ�
            m_handler.MoveTo(x,y);
        }
    }

    public interface IDBattle_Soldier
    {
        void MoveTo(int x,int y);
    }
}
