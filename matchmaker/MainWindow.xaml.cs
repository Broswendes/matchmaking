using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;

namespace matchmaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public class PlayersList : ObservableCollection<Player>
        {

        }

        public class Player
        {

            public int EstimateSkill { get; set; }
            public int RealSkill { get; set; }
            public string Id { get; set; }

            public Player(int _estimate, int _real, string _id)
            {
                EstimateSkill = _estimate;
                RealSkill = _real;
                Id = _id;
            }
        }

        PlayersList players;
        Random rnd = new Random();
        private void generate_Click(object sender, RoutedEventArgs e)
        {
            XDocument doc = new XDocument();

            XElement root = new XElement("players");
           
            players = new PlayersList();
            for (int i = 0; i < 10; ++i)
                players.Add(new Player(1000, rnd.Next(800, 1200), i.ToString()));

            dgridPlayers.ItemsSource = players;
        }

        private void btnSimulate_Click(object sender, RoutedEventArgs e)
        {
            foreach(Player curPlayer in players)
            {
                for(int index = 0; index < 100; ++index)
                {
                    int opponentIndex = index % players.Count;

                    Player opponent = players[opponentIndex];
                    if (opponent == curPlayer)
                        opponent = players[(opponentIndex + 1) % players.Count];
                    simulateMatch(curPlayer, opponent);
                }
            }
            dgridPlayers.Items.Refresh();
        }

        private void simulateMatch(Player _player1, Player _player2)
        {
            Player winner = null;
            Player loser = null;
           
            if (rnd.Next(0,2) > 0 )
            {
                winner = _player1;
                loser = _player2;
            }
            else
            {
                winner = _player2;
                loser = _player1;
            }

            winner.EstimateSkill += 12;
            loser.EstimateSkill -= 12;
        }
    }
}
