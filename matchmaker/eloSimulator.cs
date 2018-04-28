using System;
using Moserware.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.Elo
{
    public class GaussianEloCalculator : TwoPlayerEloCalculator
    {
        // From the paper
        private static readonly KFactor StableKFactor = new KFactor(24);

        public GaussianEloCalculator()
            : base(StableKFactor)
        {
        }

        protected override double GetPlayerWinProbability(GameInfo gameInfo, double playerRating, double opponentRating)
        {
            double ratingDifference = playerRating - opponentRating;

            // See equation 1.1 in the TrueSkill paper
            return GaussianDistribution.CumulativeTo(
                ratingDifference
                /
                (Math.Sqrt(2) * gameInfo.Beta));
        }
    }

    public abstract class TwoPlayerEloCalculator : SkillCalculator
    {
        protected readonly KFactor _KFactor;

        protected TwoPlayerEloCalculator(KFactor kFactor)
            : base(SupportedOptions.None, TeamsRange.Exactly(2), PlayersRange.Exactly(1))
        {
            _KFactor = kFactor;
        }

        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams);
            RankSorter.Sort(ref teams, ref teamRanks);

            var result = new Dictionary<TPlayer, Rating>();
            bool isDraw = (teamRanks[0] == teamRanks[1]);

            var player1 = teams.First().First();
            var player2 = teams.Last().First();

            var player1Rating = player1.Value.Mean;
            var player2Rating = player2.Value.Mean;

            result[player1.Key] = CalculateNewRating(gameInfo, player1Rating, player2Rating, isDraw ? PairwiseComparison.Draw : PairwiseComparison.Win);
            result[player2.Key] = CalculateNewRating(gameInfo, player2Rating, player1Rating, isDraw ? PairwiseComparison.Draw : PairwiseComparison.Lose);

            return result;
        }

        protected virtual EloRating CalculateNewRating(GameInfo gameInfo, double selfRating, double opponentRating, PairwiseComparison selfToOpponentComparison)
        {
            double expectedProbability = GetPlayerWinProbability(gameInfo, selfRating, opponentRating);
            double actualProbability = GetScoreFromComparison(selfToOpponentComparison);
            double k = _KFactor.GetValueForRating(selfRating);
            double ratingChange = k * (actualProbability - expectedProbability);
            double newRating = selfRating + ratingChange;

            return new EloRating(newRating);
        }

        private static double GetScoreFromComparison(PairwiseComparison comparison)
        {
            switch (comparison)
            {
                case PairwiseComparison.Win:
                    return 1;
                case PairwiseComparison.Draw:
                    return 0.5;
                case PairwiseComparison.Lose:
                    return 0;
                default:
                    throw new NotSupportedException();
            }
        }

        protected abstract double GetPlayerWinProbability(GameInfo gameInfo, double playerRating, double opponentRating);

        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams);
            double player1Rating = teams.First().First().Value.Mean;
            double player2Rating = teams.Last().First().Value.Mean;
            double ratingDifference = player1Rating - player2Rating;

            // The TrueSkill paper mentions that they used s1 - s2 (rating difference) to
            // determine match quality. I convert that to a percentage as a delta from 50%
            // using the cumulative density function of the specific curve being used
            double deltaFrom50Percent = Math.Abs(GetPlayerWinProbability(gameInfo, player1Rating, player2Rating) - 0.5);
            return (0.5 - deltaFrom50Percent) / 0.5;
        }
    }

    /// <summary>
    /// Base class for all skill calculator implementations.
    /// </summary>
    public abstract class SkillCalculator
    {
        [Flags]
        public enum SupportedOptions
        {
            None = 0x00,
            PartialPlay = 0x01,
            PartialUpdate = 0x02,
        }

        private readonly SupportedOptions _SupportedOptions;
        private readonly PlayersRange _PlayersPerTeamAllowed;
        private readonly TeamsRange _TotalTeamsAllowed;

        protected SkillCalculator(SupportedOptions supportedOptions, TeamsRange totalTeamsAllowed, PlayersRange playerPerTeamAllowed)
        {
            _SupportedOptions = supportedOptions;
            _TotalTeamsAllowed = totalTeamsAllowed;
            _PlayersPerTeamAllowed = playerPerTeamAllowed;
        }

        /// <summary>
        /// Calculates new ratings based on the prior ratings and team ranks.
        /// </summary>
        /// <typeparam name="TPlayer">The underlying type of the player.</typeparam>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <param name="teamRanks">The ranks of the teams where 1 is first place. For a tie, repeat the number (e.g. 1, 2, 2)</param>
        /// <returns>All the players and their new ratings.</returns>
        public abstract IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                  IEnumerable
                                                                                      <IDictionary<TPlayer, Rating>>
                                                                                      teams,
                                                                                  params int[] teamRanks);

        /// <summary>
        /// Calculates the match quality as the likelihood of all teams drawing.
        /// </summary>
        /// <typeparam name="TPlayer">The underlying type of the player.</typeparam>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <returns>The quality of the match between the teams as a percentage (0% = bad, 100% = well matched).</returns>
        public abstract double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                              IEnumerable<IDictionary<TPlayer, Rating>> teams);

        public bool IsSupported(SupportedOptions option)
        {
            return (_SupportedOptions & option) == option;
        }

        /// <summary>
        /// Helper function to square the <paramref name="value"/>.
        /// </summary>        
        /// <returns><param name="value"/> * <param name="value"/></returns>
        protected static double Square(double value)
        {
            return value * value;
        }

        protected void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams, _TotalTeamsAllowed, _PlayersPerTeamAllowed);
        }

        private static void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(
            IEnumerable<IDictionary<TPlayer, Rating>> teams, TeamsRange totalTeams, PlayersRange playersPerTeam)
        {
            int countOfTeams = 0;
            foreach (var currentTeam in teams)
            {
                if (!playersPerTeam.IsInRange(currentTeam.Count))
                {
                    throw new ArgumentException();
                }
                countOfTeams++;
            }

            if (!totalTeams.IsInRange(countOfTeams))
            {
                throw new ArgumentException();
            }
        }
    }
}