using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    public enum SytuacjaNaPlanszy 
    {
        RuchJestMozliwy,
        BiezacyGraczNieMozeWykonacRuchu,
        ObajGraczeNieMogaWykonacRuchu,
        WszystkiePolaPlanszySaZajete
    }

    public interface ISilnikGryDlaDwochGraczy
    {
        int SzerokoscPlanszy { get; }
        int WysokoscPlanszy { get; }
        int NumerGraczaWykonujacegoNastepnyRuch { get; }
        int NumerGraczaMajacegoPrzewage { get; }
        int PobierzStanPola(int poziomo, int pionowo);
        bool PolozKamien(int poziomo, int pionowo);
        
        int LiczbaPustychPol { get; }
        int LiczbaPolGracz1 { get; }
        int LiczbaPolGracz2 { get; }

        void Pasuj();

        SytuacjaNaPlanszy ZbadajSytuacjeNaPlanszy();
    }

    public interface ISilnikGryDlaJednegoGracza : ISilnikGryDlaDwochGraczy
    {
        void ProponujNajlepszyRuch(out int najlepszyRuchPoziomo, out int najlepszyRuchPionowo);
    }

}