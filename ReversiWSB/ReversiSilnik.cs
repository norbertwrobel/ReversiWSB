using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    public class ReversiSilnik : ISilnikGryDlaDwochGraczy
    {
        public int SzerokoscPlanszy { get; private set; }
        public int WysokoscPlanszy { get; private set; }

        protected int[,] plansza;
        public int NumerGraczaWykonujacegoNastepnyRuch { get; private set; } = 1;

        private static int numerPrzeciwnika(int numerGracza)
        {
            return (numerGracza == 1) ? 2 : 1;
        }

        private bool czyWspolrzednePolaPrawidlowe(int poziom, int pionowo)
        {
            return poziom >= 0 && poziom < SzerokoscPlanszy && pionowo >= 0 && pionowo < WysokoscPlanszy;
        }

        public int PobierzStanPola(int poziomo, int pionowo)
        {
            if (!czyWspolrzednePolaPrawidlowe(poziomo, pionowo))
                throw new Exception("Nieprawidłowe wspórzędne pola");
            return plansza[poziomo, pionowo];
        }
        private void czyscPlansze()
        {
            for (int i = 0; i < SzerokoscPlanszy; i++)
                for (int j = 0; j < WysokoscPlanszy; j++)
                    plansza[i, j] = 0;

            int srodekSzer = SzerokoscPlanszy / 2;
            int srodekWys = WysokoscPlanszy / 2;
            plansza[srodekSzer - 1, srodekWys - 1] = plansza[srodekSzer, srodekWys] = 1;
            plansza[srodekSzer - 1, srodekWys] = plansza[srodekSzer, srodekWys - 1] = 2;
        }

        public ReversiSilnik(int numerGraczaRozpoczynajacego, int szerokoscPlanszy = 8, int wysokoscPlanszy = 8)
        {
            if (numerGraczaRozpoczynajacego < 1 || numerGraczaRozpoczynajacego > 2)
                throw new Exception("Nieprawidłowy numer gracza rozpoczynajacego gre");

            SzerokoscPlanszy = szerokoscPlanszy;
            WysokoscPlanszy = wysokoscPlanszy;
            plansza = new int[SzerokoscPlanszy, WysokoscPlanszy];

            czyscPlansze();

            NumerGraczaWykonujacegoNastepnyRuch = numerGraczaRozpoczynajacego;
            obliczLiczbyPol();
        }

        private void zmienBiezacegoGracza()
        {
            NumerGraczaWykonujacegoNastepnyRuch = numerPrzeciwnika(NumerGraczaWykonujacegoNastepnyRuch);
        }

        protected int PolozKamien(int poziomo, int pionowo, bool tylkoTekst)
        {
            // czy współrzędne są prawdziwe
            if (!czyWspolrzednePolaPrawidlowe(poziomo, pionowo))
                throw new Exception("Nieprawidłowe współrzędne pola");

            // czy pole nie jest już zajęte
            if (plansza[poziomo, pionowo] != 0) return -1;

            int ilePolPrzyjetych = 0;

            // pętla po 8 kierunkach
            for (int kierunekPoziomo = -1; kierunekPoziomo <= 1; kierunekPoziomo++)
                for (int kierunekPionowo = -1; kierunekPionowo <= 1; kierunekPionowo++)
                {
                    // wymuszanie pominięcia przypadku, gdy obie zmienne są równe 0
                    if (kierunekPoziomo == 0 && kierunekPionowo == 0) continue;

                    // szukanie kamieni gracza w jednym z 8 kierunków
                    int i = poziomo;
                    int j = pionowo;
                    bool znalezionyKamienPrzeciwnika = false;
                    bool znalezionyKamienGraczaWykonujacegoRuch = false;
                    bool znalezionePustePole = false;
                    bool osiagnietaKrawedzPlanszy = false;
                    do
                    {
                        i += kierunekPoziomo;
                        j += kierunekPionowo;
                        if (!czyWspolrzednePolaPrawidlowe(i, j))
                            osiagnietaKrawedzPlanszy = true;
                        if (!osiagnietaKrawedzPlanszy)
                        {
                            if (plansza[i, j] == NumerGraczaWykonujacegoNastepnyRuch)
                                znalezionyKamienGraczaWykonujacegoRuch = true;
                            if (plansza[i, j] == 0) znalezionePustePole = true;
                            if (plansza[i, j] == numerPrzeciwnika(NumerGraczaWykonujacegoNastepnyRuch))
                                znalezionyKamienPrzeciwnika = true;
                        }                        
                    }
                    while (!(osiagnietaKrawedzPlanszy || znalezionyKamienGraczaWykonujacegoRuch || znalezionePustePole));

                    //sprawdzanie warunków poprawności ruchu
                    bool polozenieKamieniaJakJestMozliwe = znalezionyKamienPrzeciwnika && znalezionyKamienGraczaWykonujacegoRuch && !znalezionePustePole;

                    // odwrócenie kamieni w przypadku spelnionego warunku
                    if (polozenieKamieniaJakJestMozliwe)
                    {
                        int maks_indeks = Math.Max(Math.Abs(i - poziomo), Math.Abs(j - pionowo));

                        if (!tylkoTekst)
                        {
                            for (int indeks = 0; indeks < maks_indeks; indeks++)
                                plansza[poziomo + indeks * kierunekPoziomo, pionowo + indeks * kierunekPionowo] = NumerGraczaWykonujacegoNastepnyRuch;
                        }

                        ilePolPrzyjetych += maks_indeks - 1;
                    }
                }// koniec pętli po kierunkach
            // zmiana gracza, jeżeli ruch został wykonany
            if (ilePolPrzyjetych > 0 && !tylkoTekst)
                zmienBiezacegoGracza();
            obliczLiczbyPol();
            // zmienna ilePolPrzyjetych nie uwzględnia dostawionego kamienie
            return ilePolPrzyjetych;
        }

        public bool PolozKamien(int poziomo, int pionowo)
        {
            return PolozKamien(poziomo, pionowo, false) > 0;
        }

        private int[] liczbyPol = new int[3]; // puste, gracz 1, gracz 2

        private void obliczLiczbyPol()
        {
            for (int i = 0; i < liczbyPol.Length; ++i) liczbyPol[i] = 0;

            for (int i = 0; i < SzerokoscPlanszy; ++i)
                for (int j = 0; j < WysokoscPlanszy; ++j)
                    liczbyPol[plansza[i, j]]++;
        }

        public int LiczbaPustychPol { get { return liczbyPol[0]; } }
        public int LiczbaPolGracz1 { get { return liczbyPol[1]; } }
        public int LiczbaPolGracz2 { get { return liczbyPol[2]; } }

        private bool czyBiezacyGraczMozeWykonacRuch()
        {
            int liczbaPoprawnychPol = 0;
            for (int i = 0; i < SzerokoscPlanszy; ++i)
                for (int j = 0; j < WysokoscPlanszy; ++j)
                    if (plansza[i, j] == 0 && PolozKamien(i, j, true) > 0)
                        liczbaPoprawnychPol++;
            return liczbaPoprawnychPol > 0;
        }
        public void Pasuj()
        {
            if (czyBiezacyGraczMozeWykonacRuch())
                throw new Exception("Gracz nie może oddać ruchu, jeżeli wykonanie ruchu jest możliwe");
            zmienBiezacegoGracza();
        }
        
        public SytuacjaNaPlanszy ZbadajSytuacjeNaPlanszy()
        {
            if (LiczbaPustychPol == 0) return SytuacjaNaPlanszy.WszystkiePolaPlanszySaZajete;

            // badanie możliwości ruchu bieżącego gracza
            bool czyMozliwyRuch = czyBiezacyGraczMozeWykonacRuch();
            if (czyMozliwyRuch) return SytuacjaNaPlanszy.RuchJestMozliwy;
            else
            {
                // badanie możliwości ruchu przeciwnika
                zmienBiezacegoGracza();
                bool czyMozliwyRuchOponenta = czyBiezacyGraczMozeWykonacRuch();
                zmienBiezacegoGracza();
                if (czyMozliwyRuchOponenta) return SytuacjaNaPlanszy.BiezacyGraczNieMozeWykonacRuchu;
                else return SytuacjaNaPlanszy.ObajGraczeNieMogaWykonacRuchu;
            }
        }

        public int NumerGraczaMajacegoPrzewage
        {
            get
            {
                if (LiczbaPolGracz1 == LiczbaPolGracz2) return 0;
                else return (LiczbaPolGracz1 > LiczbaPolGracz2) ? 1 : 2;
            }
        }

    }
}