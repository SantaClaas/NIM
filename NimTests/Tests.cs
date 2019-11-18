using System;
using System.Collections.Generic;
using NIM;
using NUnit.Framework;

namespace NimTests
{
    public class Tests
    {
        [Test]
        public void Test_Move()
        {
            Move moveA = new Move(new[] { 1, 2, 3, 4 });

            Assert.AreEqual(4, moveA.ChangesPerRow.Count);
            Assert.AreEqual(1, moveA.ChangesPerRow[0]);
            Assert.AreEqual(2, moveA.ChangesPerRow[1]);
            Assert.AreEqual(3, moveA.ChangesPerRow[2]);
            Assert.AreEqual(4, moveA.ChangesPerRow[3]);


            Move moveB = new Move(new[] { 1, 2, 3, 4 });

            Assert.True(moveA.Equals(moveB));
        }

        [Test]
        public void Test_Playground()
        {
            Playground playgroundA = new Playground(new[] { 5, 6 });

            Assert.AreEqual(2, playgroundA.Rows.Count);
            Assert.AreEqual(5, playgroundA.Rows[0]);
            Assert.AreEqual(6, playgroundA.Rows[1]);


            Playground playgroundB = new Playground(playgroundA);

            Assert.AreEqual(2, playgroundB.Rows.Count);
            Assert.AreEqual(5, playgroundB.Rows[0]);
            Assert.AreEqual(6, playgroundB.Rows[1]);


            Move move = new Move(new[] { 1, 3 });

            Playground playgroundC = playgroundA.ApplyMove(move);

            Assert.AreEqual(2, playgroundA.Rows.Count);
            Assert.AreEqual(5, playgroundA.Rows[0]);
            Assert.AreEqual(6, playgroundA.Rows[1]);

            Assert.AreEqual(2, playgroundB.Rows.Count);
            Assert.AreEqual(5, playgroundB.Rows[0]);
            Assert.AreEqual(6, playgroundB.Rows[1]);

            Assert.AreEqual(2, playgroundC.Rows.Count);
            Assert.AreEqual(4, playgroundC.Rows[0]);
            Assert.AreEqual(3, playgroundC.Rows[1]);
        }

        [Test]
        public void Test_Rules_Builder_Create()
        {
            Assert.NotNull(Rules.Default.ValidMoves);
            Assert.NotNull(Rules.Default.StartingField);

            Rules.Builder builder = Rules.Build(new[] { 10, 15 });

            Rules rules = builder.Create();

            Assert.AreEqual(Rules.Default.LastMoveWins, rules.LastMoveWins);
            Assert.AreEqual(Rules.Default.PlayerCount, rules.PlayerCount);
            Assert.AreEqual(Rules.Default.ValidMoves.Count, rules.ValidMoves.Count);
            for (int i = 0; i < Rules.Default.ValidMoves.Count; ++i)
                Assert.True(Rules.Default.ValidMoves[i].Equals(rules.ValidMoves[i]));
            Assert.AreEqual(2, rules.StartingField.Rows.Count);
            Assert.AreEqual(10, rules.StartingField.Rows[0]);
            Assert.AreEqual(15, rules.StartingField.Rows[1]);


            builder = Rules.Build(new[] { 20, 21 });

            builder.AddRules(new Move(new[] { 7, 9 }));

            Assert.Throws<Exception>(() => builder.AddRules(new Move(new[] { -1, 0 })));
            Assert.Throws<Exception>(() => builder.AddRules(new Move(new[] { 1, 0, 0 })));

            builder.LastMoveWins();

            builder.Players(3);

            rules = builder.Create();

            Assert.AreEqual(true, rules.LastMoveWins);
            Assert.AreEqual(3, rules.PlayerCount);
            Assert.AreEqual(1, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves[0].Equals(new Move(new[] { 7, 9 })));
            Assert.AreEqual(2, rules.StartingField.Rows.Count);
            Assert.AreEqual(20, rules.StartingField.Rows[0]);
            Assert.AreEqual(21, rules.StartingField.Rows[1]);
        }

        [Test]
        public void Test_Rules_Builder_Moves()
        {
            Rules.Builder builder = Rules.Build(new[] { 10, 20 });
            builder.AddSingleRowRules(1, 3);
            Rules rules = builder.Create();

            Assert.AreEqual(6, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 0 })));


            builder = Rules.Build(new[] { 30, 40, 50 });
            builder.AddMultiRowRules(1, 2);
            rules = builder.Create();

            Assert.AreEqual(6, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 1, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 1, 0 })));
        }

        [Test]
        public void Test_Rules_Builder_ParseMoves()
        {
            Rules.Builder builder = Rules.Build(new[] { 10, 20 });
            builder.ParseMoveRules("1");
            Rules rules = builder.Create();

            Assert.AreEqual(2, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 0 })));


            builder = Rules.Build(new[] { 10, 20 });
            builder.ParseMoveRules("2-3");
            rules = builder.Create();

            Assert.AreEqual(4, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 0 })));


            builder = Rules.Build(new[] { 10, 20 });
            builder.ParseMoveRules("2-3,0-1");
            rules = builder.Create();

            Assert.AreEqual(8, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 1 })));


            builder = Rules.Build(new[] { 10, 20 });
            builder.ParseMoveRules("2-3;1,1");
            rules = builder.Create();

            Assert.AreEqual(5, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 1 })));


            builder = Rules.Build(new[] { 10, 20 });
            builder.ParseMoveRules("2-3;1,1;3-4,2-3");
            rules = builder.Create();

            Assert.AreEqual(12, rules.ValidMoves.Count);
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 0, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 0 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 1, 1 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 4, 2 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 4, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 3 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 2, 4 })));
            Assert.True(rules.ValidMoves.Contains(new Move(new[] { 3, 4 })));
        }

        [Test]
        public void Test_Rules_ValidMoves()
        {
            Rules rules = Rules.Build(new[] { 1, 2, 3 }).AddSingleRowRules(1, 1).Create();

            Assert.True(rules.IsMoveValid(new Move(new[] { 0, 0, 1 }), new Playground(new[] { 2, 2, 2 })));
            Assert.False(rules.IsMoveValid(new Move(new[] { 0, 0, 2 }), new Playground(new[] { 2, 2, 2 })));
            Assert.False(rules.IsMoveValid(new Move(new[] { 0, 0, 1 }), new Playground(new[] { 2, 2, 0 })));

            List<Move> validMoves = rules.GetValidMoves(new Playground(new[] { 0, 1, 2 }));

            Assert.AreEqual(2, validMoves.Count);
            Assert.True(validMoves.Contains(new Move(new[] { 0, 1, 0 })));
            Assert.True(validMoves.Contains(new Move(new[] { 0, 0, 1 })));
        }

        [Test]
        public void Test_Game_Init()
        {
            Assert.Throws<ArgumentNullException>(() => new Game(null, new Player[2]));
            Assert.Throws<ArgumentNullException>(() => new Game(Rules.Default, null));
            Assert.Throws<ArgumentException>(() => new Game(Rules.Default, new Player[0]));
            Assert.Throws<Exception>(() => new Game(Rules.Default, new[] { new AiPlayerRandom("A"), new AiPlayerRandom("A") }));

            Game game = new Game(Rules.Default, new[] { new AiPlayerRandom("A"), new AiPlayerRandom("B") });

            Assert.AreEqual(GameState.ChoosePlayer, game.State);
        }

        [Test]
        public void Test_Game_Loop()
        {
            Game game = new Game(Rules.Default, new[] { new AiPlayerRandom("A"), new AiPlayerRandom("B") });

            List<Player> players = game.Loop();

            Assert.AreEqual(1, players.Count);
        }

        [Test]
        public void Test_AdvancedAi()
        {
            AiPlayerMinMax teacher = new AiPlayerMinMax("Teacher", 0f, Rules.Default);


            AiPlayerMinMax playerA = new AiPlayerMinMax("A", 1f, teacher);
            AiPlayerMinMax playerB = new AiPlayerMinMax("B", 1f, teacher);

            Game game = new Game(Rules.Default, new[] { playerA, playerB });

            List<Player> players = game.Loop();

            Assert.AreEqual(1, players.Count);
            Assert.AreSame(playerB, players[0]);



            playerA = new AiPlayerMinMax("A", -1f, teacher);
            playerB = new AiPlayerMinMax("B", 1f, teacher);

            game = new Game(Rules.Default, new[] { playerA, playerB });

            players = game.Loop();

            Assert.AreEqual(1, players.Count);
            Assert.AreSame(playerB, players[0]);



            playerA = new AiPlayerMinMax("A", 1f, teacher);
            playerB = new AiPlayerMinMax("B", -1f, teacher);

            game = new Game(Rules.Default, new[] { playerA, playerB });

            players = game.Loop();

            Assert.AreEqual(1, players.Count);
            Assert.AreSame(playerA, players[0]);



            playerA = new AiPlayerMinMax("A", -1f, teacher);
            playerB = new AiPlayerMinMax("B", -1f, teacher);

            game = new Game(Rules.Default, new[] { playerA, playerB });

            players = game.Loop();

            Assert.AreEqual(1, players.Count);
            Assert.AreSame(playerA, players[0]);
        }

        [Test]
        public void Test_Game_FullRun()
        {
            Rules rules = Rules
                .Build(new[] { 1, 2 })
                .LastMoveWins()
                .Players(2)
                .AddRules(new Move(new[] { 1, 0 }), new Move(new[] { 0, 1 })).Create();

            Game game = new Game(rules, new[] { new AiPlayerFirst("A"), new AiPlayerFirst("B") });

            Assert.AreEqual(GameState.ChoosePlayer, game.State);
            Assert.AreEqual("B", game.CurrentPlayer.Name);
            Assert.IsNull(game.LastMove);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.RequestPlayerResponse, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.IsNull(game.LastMove);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.ApplyMove, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 1, 0 })));
            Assert.AreEqual(1, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.CheckConditions, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 1, 0 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.ChoosePlayer, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 1, 0 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.RequestPlayerResponse, game.State);
            Assert.AreEqual("B", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 1, 0 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.ApplyMove, game.State);
            Assert.AreEqual("B", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(2, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.CheckConditions, game.State);
            Assert.AreEqual("B", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.ChoosePlayer, game.State);
            Assert.AreEqual("B", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.RequestPlayerResponse, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.ApplyMove, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(1, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.CheckConditions, game.State);
            Assert.AreEqual("A", game.CurrentPlayer.Name);
            Assert.True(game.LastMove.Equals(new Move(new[] { 0, 1 })));
            Assert.AreEqual(0, game.CurrentPlayground.Rows[0]);
            Assert.AreEqual(0, game.CurrentPlayground.Rows[1]);
            Assert.True(game.Step());

            Assert.AreEqual(GameState.GameOver, game.State);
            List<Player> winningPlayers = game.GetWinningPlayers();
            Assert.AreEqual(1, winningPlayers.Count);
            Assert.AreEqual("A", winningPlayers[0].Name);
            Assert.False(game.Step());
        }

        [Test]
        [Explicit]
        public void Test_AdvancedAi_GamePlan_Resources()
        {
            GamePlan gamePlan = new GamePlan(Rules.Build(new[] { 2, 2 }).AddSingleRowRules(1, 2).Players(2).LastMoveWins().Create());
            gamePlan.Generate();

            gamePlan = new GamePlan(Rules.Build(new[] { 1, 2, 3, 4, 5 }).AddSingleRowRules(1, 3).Players(2).LastMoveWins().Create());
            gamePlan.Generate();

            gamePlan = new GamePlan(Rules.Build(new[] { 2, 2, 8, 5, 10, 4 }).AddSingleRowRules(1, 5).Players(4).LastMoveWins().Create());
            gamePlan.Generate();
        }
    }
}