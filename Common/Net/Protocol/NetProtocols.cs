﻿using System.Collections;

public class NetProtocols
{
    public static int TEST_CONNECT = 0x00000001; //连接测试
    public static int SEND_CLIENT_ID = 0x00000002; //发送客户端ID：1. 注册用户ID，2. 申请开始匹配
    public static int INFO_NUMBER = 0x00000003; //服务端信息
    public static int WARNING_NUMBER = 0x00000004; //服务端警示

    public static int GAME_BEGIN = 0x00000100; //游戏开始
    public static int PLAYER = 0x00000101; //英雄信息
    public static int PLAYER_COST_CHANGE = 0x00000102; //英雄费用信息
    public static int DRAW_CARD = 0x00000103; //抽一张牌
    public static int SUMMON_RETINUE = 0x00000105; //召唤随从
    public static int PLAYER_TURN = 0x00000106; //切换玩家
    public static int CLIENT_END_ROUND = 0x00000107; //切换玩家
    public static int CARD_DECK_INFO = 0x00000108; //卡组信息
}