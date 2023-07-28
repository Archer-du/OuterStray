using System.Collections;
using System.Collections.Generic;
using DataCore.CultivateItems;
using DataCore.TacticalItems;

namespace SystemEventHandler
{

	public delegate void TacticalInitHandler();

	public delegate void UnlockHandler();

	internal delegate void DeckImportHandler(Pack pack);

	internal delegate void BattleStartHandler(Deck deck, Deck testDeck);
}
