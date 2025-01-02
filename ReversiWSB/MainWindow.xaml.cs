using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Reversi.PlanszaDLaDwochGraczy;

namespace Reversi
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Tworzenie planszy
        private ISilnikGryDlaJednegoGracza silnik = new ReversiSilnikAI(1, 8, 8);
        private bool graPrzeciwkoKomputerowi = true;
        private DispatcherTimer timer;
        
        string[] nazwyGraczy = { "", "zielony", "brązowy" };
        
        private void uzgodnijZawartoscPlanszy()
        {
            for (int i = 0; i < silnik.SzerokoscPlanszy; i++)
                for (int j = 0; j < silnik.WysokoscPlanszy; j++)
                {
                    planszaKontrolka.ZaznaczRuch(new PlanszaDLaDwochGraczy.WspolrzednePola(i, j), (PlanszaDLaDwochGraczy.StanPola)silnik.PobierzStanPola(i, j));
                }

            przyciskKolorGracza.Background = planszaKontrolka.PedzelDlaStanu((PlanszaDLaDwochGraczy.StanPola)silnik.NumerGraczaWykonujacegoNastepnyRuch); 
            liczbaPolZielonych.Text = silnik.LiczbaPolGracz1.ToString();
            LiczbaPolBrazowych.Text = silnik.LiczbaPolGracz2.ToString();
        }
        
        private static string symbolPola(int poziomo, int pionowo)
        {
            if (poziomo > 25 || pionowo > 8) return "(" + poziomo.ToString() + "," + pionowo.ToString() + ")";
            return "" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[poziomo] + "123456789"[pionowo];
        }
        private void planszaKontrolka_KliknieciePola(object sender, PlanszaDLaDwochGraczy.PlanszaEventArgs e)
        {
            
            int kliknietePoziomo = e.WspolrzednePola.Poziomo; //wspolrzedne.Poziomo;
            int kliknietePionowo = e.WspolrzednePola.Pionowo; //wspolrzedne.Pionowo;

            // wykonywanie ruchu
            int zapamietanyNumerGracza = silnik.NumerGraczaWykonujacegoNastepnyRuch;
            if (silnik.PolozKamien(kliknietePoziomo, kliknietePionowo))
            {
                uzgodnijZawartoscPlanszy();

                // lista ruchów
                switch (zapamietanyNumerGracza)
                {
                    case 1:
                        listaRuchowZielony.Items.Add(symbolPola(kliknietePoziomo, kliknietePionowo));
                        break;
                    case 2:
                        listaRuchowBrazowy.Items.Add(symbolPola(kliknietePoziomo, kliknietePionowo));
                        break;
                }
                listaRuchowZielony.SelectedIndex = listaRuchowZielony.Items.Count - 1;
                listaRuchowBrazowy.SelectedIndex = listaRuchowBrazowy.Items.Count - 1;

                // sytuacje specjalne
                SytuacjaNaPlanszy sytuacjaNaPlanszy = silnik.ZbadajSytuacjeNaPlanszy();
                bool koniecGry = false;
                switch (sytuacjaNaPlanszy)
                {
                    case SytuacjaNaPlanszy.BiezacyGraczNieMozeWykonacRuchu:
                        MessageBox.Show("Gracz " + nazwyGraczy[silnik.NumerGraczaWykonujacegoNastepnyRuch] + " zmuszony jest do oddania ruchu");
                        silnik.Pasuj();
                        uzgodnijZawartoscPlanszy();
                        break;
                    case SytuacjaNaPlanszy.ObajGraczeNieMogaWykonacRuchu:
                        MessageBox.Show("Obaj gracze nie mogą wykonać ruchu");
                        koniecGry = true;
                        break;
                    case SytuacjaNaPlanszy.WszystkiePolaPlanszySaZajete:
                        koniecGry = true;
                        break;
                }

                // koniec gry -- informacje o wyniku
                if (koniecGry)
                {
                    int numerZwyciezcy = silnik.NumerGraczaMajacegoPrzewage;
                    if (numerZwyciezcy != 0) MessageBox.Show("Wygrał gracz " + nazwyGraczy[numerZwyciezcy], Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    else MessageBox.Show("Remis", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    if(MessageBox.Show("Czy rozpocząć grę od nowa?", "Reversi", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        przygotowaniePlanszyDoNowejGry(1, silnik.SzerokoscPlanszy, silnik.WysokoscPlanszy);
                    }
                    else
                    {
                        planszaKontrolka.IsEnabled = false;
                        przyciskKolorGracza.IsEnabled = false;
                    }
                }
                else
                {
                    if(graPrzeciwkoKomputerowi && silnik.NumerGraczaWykonujacegoNastepnyRuch == 2)
                    {
                        //wykonajNajlepszyRuch();
                        if (timer == null)
                        {
                            timer = new DispatcherTimer();
                            timer.Interval = new System.TimeSpan(0, 0, 0, 0, 300);
                            timer.Tick += (_sender, _e) => { timer.IsEnabled = false; wykonajNajlepszyRuch(); };
                        }
                        timer.Start();
                    }
                }
            }
        }

        private void przygotowaniePlanszyDoNowejGry(int numerGraczaRozpoczynajacego, int szerokoscPlanszy = 8, int wysokoscPlanszy = 8)
        {
            silnik = new ReversiSilnikAI(numerGraczaRozpoczynajacego, szerokoscPlanszy, wysokoscPlanszy);
            listaRuchowZielony.Items.Clear();
            listaRuchowBrazowy.Items.Clear();
            uzgodnijZawartoscPlanszy();
            planszaKontrolka.IsEnabled = true;
            przyciskKolorGracza.IsEnabled = true;
        }
        #endregion

        #region Ruch Gracza
        private PlanszaDLaDwochGraczy.WspolrzednePola? ustalNajlepszyRuch()
        {
            if (!planszaKontrolka.IsEnabled) return null;

            if(silnik.LiczbaPustychPol == 0)
            {
                MessageBox.Show("Nie ma już wolnych pól na planszy", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            try
            {
                int poziomo, pionowo;
                silnik.ProponujNajlepszyRuch(out poziomo, out pionowo);
                return new PlanszaDLaDwochGraczy.WspolrzednePola() { Poziomo = poziomo, Pionowo = pionowo };
            }
            catch
            {
                MessageBox.Show("Bieżący gracz nie może wykonać ruchu", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private void zaznaczNajlepszyRuch()
        {
            PlanszaDLaDwochGraczy.WspolrzednePola? wspolrzednePola = ustalNajlepszyRuch();
            if (wspolrzednePola.HasValue)
            {
                planszaKontrolka.ZaznaczPodpowiedz(wspolrzednePola.Value, (PlanszaDLaDwochGraczy.StanPola)silnik.NumerGraczaWykonujacegoNastepnyRuch);
            }
        }

        private void wykonajNajlepszyRuch()
        {
            PlanszaDLaDwochGraczy.WspolrzednePola? wspolrzednePola = ustalNajlepszyRuch();
            if (wspolrzednePola.HasValue)
            {
                planszaKontrolka_KliknieciePola(planszaKontrolka, new PlanszaDLaDwochGraczy.PlanszaEventArgs() { WspolrzednePola = wspolrzednePola.Value });
            }
        }
        #endregion
        
        private void przyciskKolorGracza_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) wykonajNajlepszyRuch();
            else zaznaczNajlepszyRuch();
        }

        #region Metody zdarzeniowe menu głównego
        //Gra, Nowa gra dla jednego gracza, Rozpoczyna komputer (brązowy)
        private void MenuItem_NowaGraDlaGracza_RozpoczynaKomputer_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwkoKomputerowi = true;
            Title = "Reversi - 1 gracz";
            przygotowaniePlanszyDoNowejGry(2);
            wykonajNajlepszyRuch(); // oddanie pierwszego ruchu komputerowi
        }
        //Gra, Nowa gra dla jednego gracza, Rozpoczynasz Ty (zielony)
        private void MenuItem_NowaGraDlaGracza_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwkoKomputerowi = true;
            Title = "Reversi - 1 gracz";
            przygotowaniePlanszyDoNowejGry(1);
        }
        //Gra, Nowa gra dla dwóch graczy
        private void MenuItem_NowaGraDla2Graczy_Click(object sender, RoutedEventArgs e)
        {
            Title = "Reversi - 2 graczy";
            graPrzeciwkoKomputerowi = false;
            przygotowaniePlanszyDoNowejGry(1);
        }
        //Gra, Zamknij
        private void MenuItem_Zamknij_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //Pomoc, Podpowiedź ruchu
        private void MenuItem_PodpowiedzRuchu_Click(object sender, RoutedEventArgs e)
        {
            zaznaczNajlepszyRuch();
        }
        //Pomoc, Ruch wykonany przez komputer
        private void MenuItem_RuchWykonanyPrzezKomputer_Click(object sender, RoutedEventArgs e)
        {
            wykonajNajlepszyRuch();
        }
        private void MenuItem_ZasadyGry_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "W grze Reversi gracze zajmują na przemian pola planszy, przejmując przy tym wszystkie pola przeciwnika znajujące sie między nowo zajętym polem, a innymi polami gracza wykonującego ruch. Celem gry jest zdobycie większej liczby pól niż przeciwnik. \n" +
                "Gracz może zająć jedynie takie pole, które pozwoli mu przejąć przynajmniej jedno pole przeciwnika. Jeżeli takiego pola nie ma, musi oddać ruch.\n" +
                "Gra kończy się w momencie zajęcia wszystkich pól lub gdy żaden z graczy nie może wykonać ruchu. \n",
                "Reversi - Zasady gry");

        }
        private void MenuItem_StrategiaKomputera_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Komputer kieruje się nastepującymi priorytetami (od najwyższego): \n" +
                "1. Ustawić pionek w rogu.\n" +
                "2. Unikać ustawienia pionka tuż przy rogu. \n" +
                "3. Ustawić pionek przy krawędzi planszy. \n" +
                "4. Unikać ustawienia pionka w wierszu lub kolumnie oddalonej o jedno pole od krawędzi planszy. \n" +
                "5. Wybierać pole, w wyniku którego zdobyta zostanie największa liczba pól przeciwnika. \n",
                "Reversi - Strategia Komputera");
        }

        private void MenuItem_OProgramie_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Program stworzony na zaliczenie laboratorium \n" +
                "Tworzenie aplikacji w WPF \n \n" +
                "Autor: Norbert Wróbel");
        }
        
        #endregion
                
        public MainWindow()
        {
            InitializeComponent();
            uzgodnijZawartoscPlanszy();

            
        }


    }
}