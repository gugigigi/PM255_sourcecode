/*-----------------------------------------------------------------------
Playfabからのデイリーボーナス実装(通常)
-----------------------------------------------------------------------*/
handlers.DailyBonus = function (args, context) { 
　　var dailyCount_N = parseInt(args.inputValue);
    var rewords1 = server.GetTitleData({
         "Keys":["DailyReward"]
    });
    var Datas = JSON.parse(rewords1.Data["DailyReward"]);
    log.debug(Datas.DailyReward)
    var c10 = Datas.DailyReward.filter(function (o) { return o.DayCount === dailyCount_N });//fillterの成功構文
    var rew = c10.shift();
    log.debug(rew.Reward)
     // requestに全て詰め込む
    var request = {
        "ItemGrants": [
            {
                "PlayFabId": currentPlayerId,
                "ItemId": rew.Reward
            }         
        ]
    };
        var playerStatResult = server.GrantItemsToUsers(request);  
    //日数が最終日の場合、数値初期化
    if (dailyCount_N === 7){dailyCount_N = 0;}
    //日数の上書き
    server.UpdateUserData({
    PlayFabId: currentPlayerId,
    Data: {
        DailyCount_Normal: ++dailyCount_N,
        LoginBool: true,
        }
    });
    // 全て詰め込んだ request を引数にserver.GrantItemsToUsersを呼び出す。
    var playerStatResult = server.GrantItemsToUsers(request);   
}
