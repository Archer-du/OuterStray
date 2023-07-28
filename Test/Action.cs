using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    public class Action
    {
        public int player_index;

        internal virtual void Exec() { }
    }

    public class MoveAction : Action
    {
        //谁要移动
        public BattleElement Target;
        public int from;
        public int to;

        internal override void Exec()
        {
            base.Exec();

            //调用
            if (Target is Battle_Soldier s)
            {
                s.Move(from, to);
            }
        }
    }
}
