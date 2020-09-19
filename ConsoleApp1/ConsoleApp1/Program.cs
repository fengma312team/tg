using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Reptile reptile = new Reptile();
            KeyValuePair<string,byte[]> b=reptile.GetImage();
            string a=reptile.GetVerification(b.Value, "d://a.png");
            string userinfo= reptile.Login("GNA119", "aa1234", a,b.Key);
            List<Match> matches= reptile.GetAllHead();
            foreach (var item in matches)
            {
                List<Info> infos= reptile.GetInfo(item);
            }


        }
    }
}
