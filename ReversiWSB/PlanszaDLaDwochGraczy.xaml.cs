using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reversi
{
    /// <summary>
    /// Logika interakcji dla klasy PlanszaDLaDwochGraczy.xaml
    /// </summary>
    public partial class PlanszaDLaDwochGraczy : UserControl
    {
        #region Typy pomocnicze
        public enum StanPola { Puste = 0, Gracz1 = 1, Gracz2 = 2 }

        public struct WspolrzednePola
        {
            public int Poziomo, Pionowo;
            public WspolrzednePola(int poziomo, int pionowo)
            {
                this.Poziomo = poziomo;
                this.Pionowo = pionowo;
            }
        }
        #endregion

        #region Tworzenie Planszy
        private int szerokosc, wysokosc;
        private StanPola[,] planszaStany;
        private Button[,] planszaPrzyciski;

        private void tworzPlansze(int szerokosc, int wysokosc)
        {
            this.szerokosc = szerokosc;
            this.wysokosc = wysokosc;

            // podział siatki na wiesze i kolumny
            planszaSiatka.ColumnDefinitions.Clear();
            for (int i = 0; i < szerokosc; i++)
                planszaSiatka.ColumnDefinitions.Add(new ColumnDefinition());
            planszaSiatka.RowDefinitions.Clear();
            for (int j = 0; j < wysokosc; j++)
                planszaSiatka.RowDefinitions.Add(new RowDefinition());

            // tworzenie tablic stanów
            planszaStany = new StanPola[szerokosc, wysokosc];
            for (int i = 0; i < szerokosc; i++)
                for (int j = 0; j < wysokosc; j++)
                    planszaStany[i, j] = StanPola.Puste;

            // tworzenie przycisków
            planszaPrzyciski = new Button[szerokosc, wysokosc];
            for (int i = 0; i < szerokosc; i++)
                for (int j = 0; j < wysokosc; j++)
                {
                    Button przycisk = new Button();
                    przycisk.Margin = new Thickness(0);
                    planszaSiatka.Children.Add(przycisk);
                    Grid.SetColumn(przycisk, i);
                    Grid.SetRow(przycisk, j);
                    przycisk.Tag = new WspolrzednePola { Poziomo = i, Pionowo = j };
                    przycisk.Click += new RoutedEventHandler(
                        (s, e) =>
                    {
                        Button kliknietyPrzycisk = s as Button;
                        WspolrzednePola wspolrzedne = (WspolrzednePola)kliknietyPrzycisk.Tag;
                        int kliknietePoziomo = wspolrzedne.Poziomo;
                        int kliknietePionowo = wspolrzedne.Pionowo;
                        onKliknieciePola(wspolrzedne);
                    });
                    planszaPrzyciski[i, j] = przycisk;
                }
            zmienKoloryWszystkichPrzyciskow();
        }
        #endregion

        #region Właściwości zmiennych Szerokosc - Wysokość
        public int Szerokosc
        {
            get { return szerokosc; }
            set { tworzPlansze(value, wysokosc); }
        }
        public int Wysokosc
        {
            get { return wysokosc; }
            set { tworzPlansze(szerokosc, value); }
        }
        #endregion

        #region Kolory
        private SolidColorBrush pedzelPustegoPola = Brushes.Ivory;
        private SolidColorBrush pedzelGracza1 = Brushes.Green;
        private SolidColorBrush pedzelGracza2 = Brushes.Sienna;

        public SolidColorBrush PedzelDlaStanu(StanPola stanPola)
        {
            switch (stanPola)
            {
                default:
                case StanPola.Puste: return pedzelPustegoPola;
                case StanPola.Gracz1: return pedzelGracza1;
                case StanPola.Gracz2: return pedzelGracza2;
            }
        }

        private void zmienKoloryWszystkichPrzyciskow()
        {
            for (int i = 0; i < szerokosc; i++)
                for (int j = 0; j < wysokosc; j++)
                {
                    planszaPrzyciski[i, j].Background = PedzelDlaStanu(planszaStany[i, j]);
                }
        }

        public Color KolorPustegoPola
        {
            get { return pedzelPustegoPola.Color; }
            set
            {
                pedzelPustegoPola = new SolidColorBrush(value);
                zmienKoloryWszystkichPrzyciskow();
            }
        }

        public Color KolorGracza1
        {
            get { return pedzelGracza1.Color; }
            set
            {
                pedzelGracza1 = new SolidColorBrush(value);
                zmienKoloryWszystkichPrzyciskow();
            }
        }

        public Color KolorGracza2
        {
            get { return pedzelGracza2.Color; }
            set
            {
                pedzelGracza2 = new SolidColorBrush(value);
                zmienKoloryWszystkichPrzyciskow();
            }
        }
        #endregion

        #region Zmiana Stanu pól
        public void ZaznaczRuch(WspolrzednePola wspolrzednePola, StanPola stanPola)
        {
            planszaStany[wspolrzednePola.Poziomo, wspolrzednePola.Pionowo] = stanPola;
            planszaPrzyciski[wspolrzednePola.Poziomo, wspolrzednePola.Pionowo].Background = PedzelDlaStanu(stanPola);
        }

        public void ZaznaczPodpowiedz(WspolrzednePola wspolrzednePola, StanPola stanPola)
        {
            if (stanPola == StanPola.Puste)
                throw new Exception("Nie można zaznaczyc podpowiedzi dla stanu pustego pola");
            SolidColorBrush pedzelPodpowiedzi = PedzelDlaStanu(stanPola).Lerp(pedzelPustegoPola, 0.5f);
            planszaPrzyciski[wspolrzednePola.Pionowo, wspolrzednePola.Poziomo].Background = pedzelPodpowiedzi;
        }
        #endregion

        #region Zdarzenie
        public class PlanszaEventArgs : RoutedEventArgs
        {
            public WspolrzednePola WspolrzednePola;
        }

        public delegate void PlanszaEventHandler(object sender, PlanszaEventArgs e);
        public event PlanszaEventHandler KliknieciePola;
        protected virtual void onKliknieciePola(WspolrzednePola wspolrzednePola)
        {
            if (KliknieciePola != null)
                KliknieciePola(this, new PlanszaEventArgs { WspolrzednePola = wspolrzednePola });
        }                                  
        #endregion

        public PlanszaDLaDwochGraczy()
        {
            InitializeComponent();

            tworzPlansze(8, 8);
        }
    }
}