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
            //���������һЩ��ʾ�ϵĳ�ʼ������
        }

        // Update is called once per frame
        void Update()
        {
            //����Ҳ���Ը���Soldier_data����ʾ�����
        }
    }
}
