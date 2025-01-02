using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    public class ReversiSilnikAI : ReversiSilnik, ISilnikGryDlaJednegoGracza
    {
        public ReversiSilnikAI(int numerGraczaRozpoczynajacego, int szerokoscPlanszy = 8, int wysokoscPlanszy = 8) : base(numerGraczaRozpoczynajacego, szerokoscPlanszy, wysokoscPlanszy)
        {

        }

        private struct MozliwyRuch : IComparable<MozliwyRuch>
        {
            public int poziomo;
            public int pionowo;
            public int priorytet;

            public MozliwyRuch (int poziomo, int pionowo, int priorytet)
            {
                this.poziomo = poziomo;
                this.pionowo = pionowo;
                this.priorytet = priorytet;
            }
            public int CompareTo(MozliwyRuch innyRuch)
            {
                return innyRuch.priorytet - this.priorytet;
            }
            
        }

        public void ProponujNajlepszyRuch(out int najlepszyRuchPoziomo, out int najlepszyRuchPionowo)
        {
            // deklaracja tablicy możliwych ruchów
            List<MozliwyRuch> mozliweRuchy = new List<MozliwyRuch>();

            int skokPiorytetu = SzerokoscPlanszy * WysokoscPlanszy;

            // poszukiwanie możliwych ruchów
            for (int poziomo = 0; poziomo < SzerokoscPlanszy; poziomo++)
                for (int pionowo = 0; pionowo < WysokoscPlanszy; pionowo++)
                {
                    // liczba zajętych pól
                    int priorytet = PolozKamien(poziomo, pionowo, true);
                    if (priorytet > 0)
                    {
                        MozliwyRuch mr = new MozliwyRuch(poziomo, pionowo, priorytet);

                        // pole w rogu +
                        if ((mr.poziomo == 0 || mr.poziomo == SzerokoscPlanszy - 1) && (mr.pionowo == 0 || mr.pionowo == WysokoscPlanszy - 1))
                            mr.priorytet += skokPiorytetu * skokPiorytetu;

                        // pole sąsiadujące z rogiem na przekątnych -
                        if ((mr.poziomo == 1 || mr.poziomo == SzerokoscPlanszy - 2) && (mr.pionowo == 1 || mr.pionowo == WysokoscPlanszy - 2))
                            mr.priorytet -= skokPiorytetu * skokPiorytetu;

                        // pole sąsiadujące z rogiem w pionie -
                        if ((mr.poziomo == 0 || mr.poziomo == SzerokoscPlanszy - 1) && (mr.pionowo == 1 || mr.pionowo == WysokoscPlanszy - 2))
                            mr.priorytet -= skokPiorytetu * skokPiorytetu;

                        // pole sąsiadujące z rogiem w poziomie -
                        if ((mr.poziomo == 1 || mr.poziomo == SzerokoscPlanszy - 2) && (mr.pionowo == 0 || mr.pionowo == WysokoscPlanszy - 1))
                            mr.priorytet -= skokPiorytetu * skokPiorytetu;

                        // pole na brzegu +
                        if (mr.poziomo == 0 || mr.poziomo == SzerokoscPlanszy - 1 || mr.pionowo == 0 || mr.pionowo == WysokoscPlanszy - 1)
                            mr.priorytet += skokPiorytetu;

                        // pole sasiadujące z brzegiem - 
                        if (mr.poziomo == 1 || mr.poziomo == SzerokoscPlanszy - 2 || mr.pionowo == 1 || mr.pionowo == WysokoscPlanszy - 2)
                            mr.priorytet -= skokPiorytetu;

                        // dodanie do listy możliwych ruchów
                        mozliweRuchy.Add(mr);
                    }
                }
            // wybór pola o największym priorytecie
            if (mozliweRuchy.Count > 0)
            {
                mozliweRuchy.Sort();
                najlepszyRuchPoziomo = mozliweRuchy[0].poziomo;
                najlepszyRuchPionowo = mozliweRuchy[0].pionowo;
            }
            else
            {
                throw new Exception("Brak możliwych ruchów");
            }
        }


    }
}