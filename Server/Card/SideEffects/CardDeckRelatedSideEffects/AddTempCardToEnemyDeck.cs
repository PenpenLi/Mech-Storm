﻿namespace SideEffects
{
    public class AddTempCardToEnemyDeck : AddTempCardToEnemyDeck_Base
    {
        public AddTempCardToEnemyDeck()
        {
        }

        public override void Execute(ExecuterInfo executerInfo)
        {
            ServerPlayer player = (ServerPlayer) Player;
            player.MyEnemyPlayer.MyCardDeckManager.RandomInsertTempCard(CardId);
        }
    }
}