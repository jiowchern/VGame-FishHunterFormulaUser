using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaUserExample
{
    class Program
    {
        static void Main(string[] args)
        {

            // 初始化
            var client = VGame.Project.FishHunter.Formula.RemotingClient.Create();
            client.UserEvent += _User;

            // 建立loop迴圈以便接收封包
            var updater = new Regulus.Utility.Updater();
            updater.Add(client);
            while(client.Enable)
            {
                updater.Update();
            }
            updater.Shutdown();

        }

        // 取得User
        static void _User(VGame.Project.FishHunter.Formula.IUser user)
        {
            // 註冊相關元件
            user.Remoting.ConnectProvider.Supply += _Connect;
            user.VerifyProvider.Supply += _Verify;
            user.FishStageQueryerProvider.Supply += _FishStageQueryer;
            user.FishStageProvider.Supply += _FishStage;
        }

        static void _FishStage(VGame.Project.FishHunter.IFishStage obj)
        {
            // 註冊攻擊回傳
            obj.HitResponseEvent += _HitResponse;
            // 註冊例外訊息
            obj.HitExceptionEvent += (message) => 
            {
                System.Console.WriteLine(message);
            };

            // 攻擊測試
            _Attack(obj);
        }

        private static void _Attack(VGame.Project.FishHunter.IFishStage obj)
        {
            // 攻擊判定請求
            var request = new VGame.Project.FishHunter.HitRequest();

            request.FishID = 1;
            request.FishOdds = 1;
            request.FishStatus = VGame.Project.FishHunter.FISH_STATUS.NORMAL | VGame.Project.FishHunter.FISH_STATUS.KING;
            request.FishType = 1;
            request.HitCnt  = 1 ;
            request.TotalHitOdds = 1;
            request.TotalHits = 1;
            request.WepBet = 1;
            request.WepID = 1;
            request.WepOdds = 1;
            request.WepType = 1;

            System.Console.WriteLine("攻擊測試");
            obj.Hit(request);
        }

        static void _HitResponse(VGame.Project.FishHunter.HitResponse obj)
        {
            // TODO : 取得攻擊傳回結果
            if(obj.DieResult == VGame.Project.FishHunter.FISH_DETERMINATION.DEATH)
                System.Console.WriteLine("死亡");
            else
                System.Console.WriteLine("存活");

        }

        private static void _FishStageQueryer(VGame.Project.FishHunter.IFishStageQueryer obj)
        {
            // 請求開啟魚場
            var result = obj.Query(12345, 1);
            result.OnValue += (success) =>
            {
                if (success)
                    System.Console.WriteLine("魚場開啟成功");
                else
                    System.Console.WriteLine("魚場開啟失敗");
            };
        }

        

        static void _Connect(Regulus.Utility.IConnect obj)
        {
            // 與伺服器連線
            var result = obj.Connect("210.65.10.160", 38971);
            result.OnValue += (success)=>
            {
                if (success)
                    System.Console.WriteLine("連線成功");
                else
                    System.Console.WriteLine("連線失敗");
            };
        }
        // 驗證登入
        static void _Verify(VGame.Project.FishHunter.IVerify obj)
        {
            var result = obj.Login("id", "pw");
            result.OnValue += (success) =>
            {
                if (success)
                    System.Console.WriteLine("登入成功");
                else
                    System.Console.WriteLine("登入失敗");
            };
        }

        
    }
}
