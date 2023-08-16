using DataCore.BattleElements;
using EventEffectModels;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WeightUpdaterHash
{
	internal delegate void WeightUpdate(BattleElement source, BattleSystem system);
	internal class WeightUpdaterTable
	{
		internal BattleElement source;
		internal BattleSystem system
		{
			get => source.battleSystem;
		}
		internal static Hashtable updaterTable;

		internal WeightUpdaterTable(BattleElement source)
		{
			this.source = source;
			updaterTable = new Hashtable()
			{
				{ "mush_00", (Action)mush_00Updater },

			};
		}
		///// <summary>
		///// 将委托注册到事件
		///// </summary>
		///// <param name="eventName"></param>
		///// <param name="handler"></param>
		//internal void RegisterHandler(string eventName, Action handler)
		//{
		//	if (!updaterTable.ContainsKey(eventName))
		//	{
		//		updaterTable.Add(eventName, null);
		//	}
		//	updaterTable[eventName] = Delegate.Combine((Action)updaterTable[eventName], handler);
		//}
		///// <summary>
		///// 将委托从事件中解除
		///// </summary>
		///// <param name="eventName"></param>
		///// <param name="handler"></param>
		//internal void UnloadHandler(string eventName, Action handler)
		//{
		//	if (updaterTable.ContainsKey(eventName))
		//	{
		//		updaterTable[eventName] = Delegate.Remove((Action)updaterTable[eventName], handler);
		//	}
		//	else return;
		//}
		internal void RaiseEvent(string ID)
		{
			if (!updaterTable.ContainsKey(ID))
			{
				throw new Exception("invalid updater");
			}
			Action method = (Action)updaterTable[ID];
			method?.Invoke();
		}



		internal void mush_00Updater()
		{

		}
	}
}
