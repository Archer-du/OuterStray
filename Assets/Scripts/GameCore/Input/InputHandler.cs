using DataCore.TacticalItems;
using DisplayInterface;
using System.Collections;
using System.Collections.Generic;

namespace InputHandler
{
	public interface ICultivationSystemInput
	{
		/// <summary>
		/// console version
		/// </summary>
		public void EnterTacticalSystem();



		/// <summary>
		/// 选择指定建筑物的指定卡包，将卡包导入到卡组中
		/// </summary>
		/// <param name="buildingID"></param>
		/// <param name="packID"></param>
		public void FromPackImportDeck(int buildingID, int packID);

		/// <summary>
		/// start expedition<br/>
		/// 开始远征
		/// </summary>
		public void EnterExpedition();
	}


	public interface ITacticalSystemInput
	{
		/// <summary>
		/// 检测合法性，进入index或point索引的节点
		/// </summary>
		/// <param name="hrztIdx"></param>
		/// <param name="vtcIdx"></param>
		public void EnterNode(int hrztIdx, int vtcIdx);
		/// <summary>
		/// 检测合法性，进入index或point索引的节点
		/// </summary>
		/// <param name="targetPoint"></param>
		public void EnterNode(string targetPoint);

		/// <summary>
		/// 检测合法性，进入下一层terrain
		/// </summary>
		public void EnterNextTerrain();
	}


	public interface IBattleSystemInput
	{
		/// <summary>
		/// 从手牌部署单位到指定战线指定位置，合法性判定
		/// </summary>
		/// <param name="handicapIdx"></param>
		/// <param name="dstLineIdx"></param>
		/// <param name="dstPos"></param>
		public void Deploy(int handicapIdx, int dstLineIdx, int dstPos);

		/// <summary>
		/// 对指定目标释放指令卡(非指向性指令卡兼容）
		/// </summary>
		/// <param name="handicapIdx"></param>
		/// <param name="dstLineIdx"></param>
		/// <param name="dstPos"></param>
		public void Cast(int handicapIdx, int dstLineIdx, int dstPos);

		/// <summary>
		/// 从战场指定战线指定位置移动元素到目标点
		/// </summary>
		/// <param name="resLineIdx"></param>
		/// <param name="Idx"></param>
		/// <param name="dstLineIdx"></param>
		/// <param name="dstPos"></param>
		public void Move(int resLineIdx, int resIdx, int dstLineIdx, int dstPos);

		public void VerticalMove(int resLineIdx, int resIdx, int dstPos);

		/// <summary>
		/// 从战场指定战线指定位置撤退单位卡
		/// </summary>
		/// <param name="resLineIdx"></param>
		/// <param name="resPos"></param>
		public void Retreat(int resLineIdx, int resPos);

		/// <summary>
		/// 跳过或结束此回合
		/// </summary>
		public void Skip();



		public void Exit();



		//ML interface
		//TODO
	}







	//test
	public interface IUnitInput
	{
		public void UpdateManual();
		public void UpdateTargetManual();
	}
}