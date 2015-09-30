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
            var result = wheel_service.Find(Guid.Empty, 1);
            result.OnValue += _SupplyWheel;
        }

        

        private static void _SupplyWheel(IWheel wheel)
        {

            var result = wheel.Spin(new Random().Next(), new Random().Next());
            result.OnValue += _SpinResult;
        }

        private static void _SpinResult(SpinResult spin_result)
        {
            foreach(var symbol in spin_result.Symbols)
            {
                Console.Write("符號{0}\n" , symbol);
            }

            Console.WriteLine("分數{0}", spin_result.Score);

            Quit = true;
        }

        private static void _SupplyVerify(IVerify verify)
        {
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
