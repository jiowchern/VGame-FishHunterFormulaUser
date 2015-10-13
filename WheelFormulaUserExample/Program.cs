using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using VGame.Project.SuperComplexWheel.Common;
using VGame.Project.SuperComplexWheel.FormulaUser;
namespace WheelFormulaUserExample
{
    class Program
    {
        public static bool Quit;

        static IWheelService _IWheelService;
        static void Main(string[] args)
        {
            Quit = false;
            var agent = VGame.Project.SuperComplexWheel.FormulaUser.Agent.Create();

            agent.Launch();
            agent.VerifyProvider().Supply += Program._SupplyVerify;
            
            agent.WheelServiceProvider().Supply += _SupplyWheelService;
            var result = agent.Connect("210.65.10.160", 17928);
            //var result = agent.Connect("127.0.0.1", 17928);
            result.OnValue += _ConnectResult;
            while (Quit == false)
            {
                agent.Update();
            }
            agent.WheelServiceProvider().Supply -= _SupplyWheelService;
            agent.VerifyProvider().Supply -= Program._SupplyVerify;
            agent.Shutdown();

            Console.ReadKey();
        }

        private static void _SupplyWheelService(IWheelService wheel_service)
        {
            _IWheelService = wheel_service;
            Console.WriteLine("取得轉輪...");
            var result = _IWheelService.Find(Guid.Empty, 1);
            result.OnValue += Program._SupplyFreeWheel;
        }

        

        private static void _SupplyFreeWheel(IWheel wheel)
        {
            Console.WriteLine("\n免費轉動...");
            var result = wheel.SpinFree(new Random().Next(), new Random().Next());
            result.OnValue += (spin_result) =>
            {
                Program._SpinFreeResult(spin_result);
                Program._SupplyNormalWheel(wheel);
            };
        }

        private static void _SpinFreeResult(SpinResult spin_result)
        {
            Program._ShowScore(spin_result);            
        }

        private static void _SupplyNormalWheel(IWheel wheel)
        {
            Console.WriteLine("\n一般轉動...");
            var result = wheel.SpinNormal(new Random().Next(), new Random().Next());
            result.OnValue += (spin_result) =>
            {
                Program._SpinMainResult(spin_result);
                Program._SupplyRatioWheel(wheel);
            };
        }

        private static void _SpinMainResult(SpinResult spin_result)
        {
            Program._ShowScore(spin_result);            
        }

        private static void _ShowScore(SpinResult spin_result)
        {
            foreach(var symbol in spin_result.Symbols)
            {
                Console.Write("符號{0}\n", symbol);
            }

            Console.WriteLine("分數{0}", spin_result.Score);

            Console.WriteLine("預期分數{0}", spin_result.ExpectedScore);
        }

        private static void _SupplyRatioWheel(IWheel wheel)
        {            
            var val = new Random().Next();
            Console.WriteLine("\n比倍轉動...倍率:{0}", val);            
            var result = wheel.SpinRatio(val);
            result.OnValue += (spin_result) =>
            {
                Program._SpinRatioResult(spin_result);
                Program._SupplyLittleGameWheel(wheel);
            };
        }

        private static void _SpinRatioResult(SpinResultRatio spin_result)
        {            
            Console.WriteLine("符號{1},預期分數{0}", spin_result.ExpectedScore , spin_result.Symbol);            
        }

        private static void _SupplyLittleGameWheel(IWheel wheel)
        {
            Console.WriteLine("\n小遊戲轉動...");
            var result = wheel.SpinLittle(new Random().Next());
            result.OnValue += Program._SpinLittleGameResult;
        }

        private static void _SpinLittleGameResult(SpinResultLittleGame spin_result)
        {
            Console.WriteLine("符號{1},預期分數{0}", spin_result.ExpectedScore, spin_result.Symbol);

            Console.WriteLine("結束...");
            Quit = true;
        }

        private static void _SupplyVerify(IVerify verify)
        {
            Console.WriteLine("開始驗證...");
            verify.Login("Guest" , "guest").OnValue += OnVerifyResult;
        }

        private static void OnVerifyResult(bool success)
        {
            if(success)
                Console.WriteLine("驗證成功.");
            else
            {
                Console.WriteLine("驗證失敗.");
            }
        }

        private static void _ConnectResult(bool success)
        {
            if (success)
                Console.WriteLine("連線成功.");
            else
            {
                Console.WriteLine("連線失敗.");
            }
        }
    }
}
