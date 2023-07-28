using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class DBattle_Soldier : MonoBehaviour,IDBattle_Soldier
    {
        public Battle_Soldier Soldier_data;

        void IDBattle_Soldier.MoveTo(int x, int y)
        {
            transform.position += new Vector3(x*10, y*10, 0);
        }

        // Start is called before the first frame update
        void Start()
        {
            //这里可以做一些显示上的初始化操作
        }

        // Update is called once per frame
        void Update()
        {
            //这里也可以根据Soldier_data做显示层更新
        }
    }
}
