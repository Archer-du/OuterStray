using Codice.Client.BaseCommands.Merge;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Bson;

namespace GameCore
{
    public interface IDBattle
    {
        IDBattle_Soldier CreateDisplay_Soldier(Battle_Soldier soldier);

    }

    public interface IDBattleElement
    {
    }

    public class BattleElement
    {
        IDBattleElement m_handler;

        public virtual void Update() { }
    }


    public class Battle
    {
        public class BattleResult
        {
            public enum BattleState {Running,Finiesh };

            public BattleState State = BattleState.Running;
        }

        private int m_score;
        List<Action> m_actions = new List<Action>();
        IDBattle m_handler;
        List<BattleElement> m_elements = new List<BattleElement>();

        BattleResult m_result = new BattleResult();

        public Battle(IDBattle handler, BattlePlayer player1, BattlePlayer player2)
        {
            m_handler = handler;

            CreateBattleElements(player1);
            CreateBattleElements(player2);
        }

        void CreateBattleElements(BattlePlayer player)
        {
            foreach (var cardConfig in player.cardConfigs)
            {
                if (cardConfig is SoldierConfig config)
                {
                    var be = new Battle_Soldier(config);


                    var display_handler = m_handler.CreateDisplay_Soldier(be);
                    be.Init(display_handler);

                    m_elements.Add(be);

                }

            }
        }

        public void AddInput(Action action)
        {
            m_actions.Add(action);
        }

        //战斗核心主循环
        public void Update()
        {
            if (m_result.State == BattleResult.BattleState.Finiesh)
                return;

            //处理actions

            foreach (var action in m_actions)
            {

            }

            m_actions.Clear();

            //BattleElement 更新
            foreach (var be in m_elements)
            {
                be.Update();
            }


            //判断结局胜负
            //m_result = 
        }

        public int Score { get { return m_score; } }
    }
}
