using System;
using System.Collections.Generic;


using Regulus.Remoting;
using Regulus.Utility;


using VGame.Project.FishHunter.Common.Data;
using VGame.Project.FishHunter.Common.GPI;
using VGame.Project.FishHunter.Formula;


using Console = System.Console;
using SpinWait = System.Threading.SpinWait;

namespace FormulaUserExample
{
	using System.IO;
	using System.Net;
	using System.Net.Mime;
	using System.Reflection;
	using System.Timers;
	using System.Windows.Forms;


	using Microsoft.Win32.SafeHandles;

	internal class Program
	{
		private static IFishStage _FishStage;

		private static IOnline _Online;

		private static IUser _User;

		[STAThread]
		private static void Main(string[] args)
		{
			var sw = new SpinWait();

			// 初始化
			var client = RemotingClient.Create();
			client.UserEvent += Program._OnUser;

			// 建立loop迴圈以便接收封包
			var updater = new Updater();
			updater.Add(client);
			while(client.Enable)
			{
				updater.Working();

				sw.SpinOnce();
			}

			updater.Shutdown();
		}

		// 取得User
		private static void _OnUser(IUser user)
		{
			Program._User = user;

			// 註冊相關元件            
			user.Remoting.ConnectProvider.Supply += Program._Connect;
			user.VerifyProvider.Supply += Program._Verify;
			user.FishStageQueryerProvider.Supply += Program._FishStageQueryer;

			// 註冊取得連線成功狀態物件            
			user.Remoting.OnlineProvider.Supply += Program._BeginOnlineStatus;

			// 註冊斷線事件
			user.Remoting.OnlineProvider.Unsupply += Program._EndOnlineStatus;

			// 如果有錯誤的方法呼叫則會發生此事件
			// 通常原因可能是版本有誤
			// 請到 https://codeload.github.com/jiowchern/VGame-FishHunterFormulaUser/zip/master 更新版本
			Program._User.ErrorMethodEvent += Program._ErrorMethodEvent;

			Program._User.VersionErrorEvent += Program._User_VersionErrorEvent;
		}

		private static void _User_VersionErrorEvent()
		{
			var url = "https://codeload.github.com/jiowchern/VGame-FishHunterFormulaUser/zip/master";

			using(var s = new SaveFileDialog())
			{
				s.Title = "版本已變動，請選擇存放位置";
				s.FileName = "FormulaUserExample";
				s.DefaultExt = "zip";

				if(s.ShowDialog() == DialogResult.OK)
				{
					var webClient = new WebClient();

					webClient.DownloadFile(url, s.FileName);
				}

				Environment.Exit(Environment.ExitCode);
			}
		}

		private static void _ErrorMethodEvent(string method, string message)
		{
			Console.WriteLine(@"錯誤的方法呼叫{0} \n 
			如果有錯誤的方法呼叫則會發生此事件\n 
			通常原因可能是版本有誤\n 
			請到 https://codeload.github.com/jiowchern/VGame-FishHunterFormulaUser/zip/master 更新版本\n 
			{1} 
			", method, message);
		}

		private static void _BeginOnlineStatus(IOnline online)
		{
			// 連線成功處理工作...
			Program._Online = online;
		}

		private static void _EndOnlineStatus(IOnline online)
		{
			// 在這裡處理斷線工作...
			Console.WriteLine("斷線");
		}

		private static void _GetFishStage(IFishStage obj)
		{
			// 註冊例外訊息
			obj.OnHitExceptionEvent += Console.WriteLine;

			// 攻擊測試
			Program._Attack(obj);
		}

		/// <summary>
		///     public RequsetFishData[] FishDatas
		///     public RequestWeaponData WeaponData
		/// </summary>
		/// <param name="fish_stage"></param>
		private static void _Attack(IFishStage fish_stage)
		{
			var tt = new RequsetFishData[0];

			var graveGoods = new[]
				                 {
					                 new RequsetFishData
						                 {
							                 FishId = 20, 
							                 FishOdds = 5, 
							                 FishStatus = FISH_STATUS.NORMAL, 
							                 FishType = FISH_TYPE.ANGEL_FISH, 
							                 GraveGoods = new RequsetFishData[0]
						                 }, 
					                 new RequsetFishData
						                 {
							                 FishId = 21, 
							                 FishOdds = 5, 
							                 FishStatus = FISH_STATUS.NORMAL, 
							                 FishType = FISH_TYPE.ANGEL_FISH, 
							                 GraveGoods = new RequsetFishData[0]
						                 }
				                 };

			var fishs = new[]
				            {
					            new RequsetFishData
						            {
							            FishId = 1, 
							            FishOdds = 1, 
							            FishStatus = FISH_STATUS.NORMAL, 
							            FishType = FISH_TYPE.ANGEL_FISH, 
							            GraveGoods = null
						            }
				            };

			var weapdaData = new RequestWeaponData
				                 {
					                 BulletId = 1, 
					                 WeaponType = WEAPON_TYPE.NORMAL, 
					                 WeaponBet = 1000, 
					                 WeaponOdds = 1, 
					                 TotalHits = fishs.Length
				                 };

			// 攻擊判定請求
			Console.WriteLine("攻擊測試");

			var hitRequest = new HitRequest(fishs, weapdaData);

			fish_stage.Hit(hitRequest);

			// 註冊攻擊回傳
			fish_stage.OnTotalHitResponseEvent += Program.Obj_OnTotalHitResponseEvent;

			// 需要接下回傳的變數
			Program._FishStage = fish_stage;
		}

		private static void Obj_OnTotalHitResponseEvent(HitResponse[] hit_responses)
		{
			foreach(var response in hit_responses)
			{
				Console.WriteLine("押注金額 = {0}", response.WeaponBet);

				Console.WriteLine("擊中魚ID = {0}，子彈ID = {1}", response.FishId, response.WepId);

				switch(response.DieResult)
				{
					// case FISH_DETERMINATION.DEATH:
					// case FISH_DETERMINATION.SURVIVAL:
				}

				Console.WriteLine("擊中結果 = {0}", response.DieResult == FISH_DETERMINATION.DEATH ? "死亡" : "存活");

				Console.WriteLine("翻倍結果 = {0}", response.OddsResult);

				if(response.FeedbackWeapons != null)
				{
					foreach(var weaponType in response.FeedbackWeapons)
					{
						Console.WriteLine("得到的道具是" + weaponType);
					}
				}
			}

			// Program._Online.Disconnect();
		}

		/// <summary>
		///     目前算法漁場ID是1 跟 100
		///     100是新算法
		///     player id 改成 GUID
		/// </summary>
		/// <param name="obj"></param>
		private static void _FishStageQueryer(IFishStageQueryer obj)
		{
			// var id = Guid.NewGuid();

			// var id  =new Guid("a0d0b42c-1293-4bbf-b6c6-02476818a59c"); 
			var id = new Guid();

			// 請求開啟魚場
			var result = obj.Query(id, 100);
			result.OnValue += fish_stage =>
				{
					if(fish_stage != null)
					{
						Console.WriteLine("魚場開啟成功");

						Program._GetFishStage(fish_stage);
					}
					else
					{
						Console.WriteLine("魚場開啟失敗");
					}
				};
		}

		private static void _Connect(IConnect obj)
		{
			Program._User.Remoting.ConnectProvider.Supply -= Program._Connect;

			// 與伺服器連線
			// var result = obj.Connect("210.65.10.160", 38971);
			var result = obj.Connect("127.0.0.1", 38971);
			result.OnValue += success =>
				{
					if(success)
					{
						Console.WriteLine("連線成功");
					}
					else
					{
						Console.WriteLine("連線失敗");
					}
				};
		}

		// 驗證登入
		private static void _Verify(IVerify obj)
		{
			var result = obj.Login("Guest", "vgame");
			result.OnValue += success =>
				{
					if(success)
					{
						Console.WriteLine("登入成功");
					}
					else
					{
						Console.WriteLine("登入失敗");
					}
				};
		}
	}
}
