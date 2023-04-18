using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TriviaBot.External;
using static System.Formats.Asn1.AsnWriter;

namespace TriviaBot.Commands
{
    public class Essential : BaseCommandModule
    {
        [Command("ping")]
        public async Task Test(CommandContext context)
        {
            await context.Channel.SendMessageAsync("pong");
        }

        [Command("help")]
        public async Task ShowHelp(CommandContext context)
        {
            var message = new DiscordEmbedBuilder()
            {
                Title = ":information_source: **DISCORD TRIVIA** :information_source:",
                Description = "To play: the game type ***!trivia*** into the chat!\n ",
            };

            await context.Channel.SendMessageAsync(embed: message);
        }

        [Command("trivia")]
        public async Task StartTrivia(CommandContext context, int numberOfQuestions = 10)
        {
            QuestionManager questionManager = new QuestionManager();
            List<CategoryOption> categories = questionManager.GetCategories();
            Dictionary<DiscordUser, int> players = new Dictionary<DiscordUser, int>();
            var interactivity = context.Client.GetInteractivity();

            // Assign title and description
            // `timeLeft` is a placeholder for the time remaining to join the game
            // `accept` is a placeholder for the emoji that the user will react with to join the game
            string title = $"{DiscordEmoji.FromName(context.Client, ":orange_circle:", false)} **DISCORD TRIVIA** {DiscordEmoji.FromName(context.Client, ":orange_circle:", false)}";
            string description = $"**To join the game please react with `joinEmoji`**\n**Good luck and have fun!**\n**Time remaining to join: `queueTimer` seconds!**";

            DiscordEmoji accept = DiscordEmoji.FromName(context.Client, ":white_check_mark:", false);
            players = await GetPlayers(context, 15, accept, title, description);
            
            if (players.Count == 0)
            {
                await context.Channel.SendMessageAsync(":red_square: **NO ONE JOINED THE GAME** :red_square:");
                return;
            }

            int responseCode = await questionManager.GetQuestions(numberOfQuestions);

            if (responseCode != 0)
            {
                await context.Channel.SendMessageAsync(":red_square: **ERROR FETCHING QUESTIONS** :red_square:");
                return;
            }

            DiscordEmoji[] emoji = {DiscordEmoji.FromName(context.Client, ":one:", false),
                                                DiscordEmoji.FromName(context.Client, ":two:", false),
                                                DiscordEmoji.FromName(context.Client, ":three:", false),
                                                DiscordEmoji.FromName(context.Client, ":four:", false),};

            int questionTime = 10;

            foreach (Question question in questionManager.questions)
            {
                var correctPlayers = await DisplayQuestion(context, questionTime, question, emoji);

                foreach(var player in correctPlayers)
                {
                    if(!player.IsBot)
                    {
                        players[player]++;
                    }
                }

                await Task.Delay(1000);
            }

            await PrintLeaderboard(context, players, title);
            return;
        }
        
        /// <summary>
        /// Displays message into discord for joining the game  
        /// </summary>
        /// <param name="context">Context gotten from Discord Command</param>
        /// <param name="queueTimer">How many seconds to wait for players to join</param>
        /// <param name="joinEmoji">What emoji represents a joined player</param>
        /// <returns>A dictionary of players and their scores</returns>
        private async Task<Dictionary<DiscordUser, int>> GetPlayers(CommandContext context, int queueTimer, DiscordEmoji joinEmoji, string title, string description)
        {
            Dictionary<DiscordUser, int> players = new Dictionary<DiscordUser, int>();

            string newDescription = description.Replace("`queueTimer`", queueTimer.ToString());
            newDescription = newDescription.Replace("`joinEmoji`", joinEmoji);

            var joinMessage = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = newDescription,
            };

            var sentInvite = await context.Channel.SendMessageAsync(embed: joinMessage);
            queueTimer--;
            await sentInvite.CreateReactionAsync(joinEmoji);

            for (int i = 0; i <= queueTimer; queueTimer--)
            {
                newDescription = description.Replace("`queueTimer`", queueTimer.ToString());
                newDescription = newDescription.Replace("`joinEmoji`", joinEmoji);
                await Task.Delay(1000);
                var newMessage = new DiscordEmbedBuilder()
                {
                    Title = title,
                    Description = newDescription,
                };
                await sentInvite.ModifyAsync(new DiscordMessageBuilder().AddEmbed(newMessage));
            }

            var joinReaction = await sentInvite.GetReactionsAsync(joinEmoji);

            foreach (var user in joinReaction)
            {
                if (!user.IsBot)
                {
                    players.Add(user, 0);
                }
            }

            return players;
        }

        /// <summary>
        /// Displays a question and waits for answers
        /// </summary>
        /// <param name="context">Context gotten from Discord Command</param>
        /// <param name="questionTimer">How many seconds to wait for the question</param>
        /// <param name="question">The question object</param>
        /// <param name="emoji">Array of emojis to represent answering</param>
        /// <returns>A list of players who answered correctly</returns>
        private async Task<IReadOnlyList<DiscordUser>> DisplayQuestion(CommandContext context, int questionTimer, Question question, DiscordEmoji[] emoji)
        {
            List<string> answers = new List<string>();
            answers.Add(question.correct_answer);
            answers.AddRange(question.incorrect_answers);
         
            if(emoji.Length != answers.Count)
            {
                throw new Exception("Emoji and answers do not match");
            }

            // Shuffle answers
            Random rng = new Random();
            answers = answers.OrderBy(x => rng.Next()).ToList();

            string title = "`question`";
            string description = $"{emoji[0]} | {answers[0]}\n\n" + $"{emoji[1]} | {answers[1]}\n\n" + $"{emoji[2]} | {answers[2]}\n\n" + $"{emoji[3]} | {answers[3]}\n\n" + $"Time remaining: `questionTimer` seconds";

            var questionMessage = new DiscordEmbedBuilder()
            {
                Title = title.Replace("`question`", question.question),
                Description = description.Replace("`questionTimer`", questionTimer.ToString()),
            };

            var sentQuestion = await context.Channel.SendMessageAsync(questionMessage);
            questionTimer--;

            foreach(var em in emoji)
            {
                await sentQuestion.CreateReactionAsync(em);
            }

            for (int i = 0; i <= questionTimer; questionTimer--)
            {
                await Task.Delay(1000);
                var newmessage = new DiscordEmbedBuilder()
                {
                    Title = title.Replace("`question`", question.question),
                    Description = description.Replace("`questionTimer`", questionTimer.ToString()),
                };
                await sentQuestion.ModifyAsync(new DiscordMessageBuilder().AddEmbed(newmessage));
            }

            // Find index with correct answer
            int correctIdx = answers.IndexOf(question.correct_answer);

            // TODO: Check if user reacted to multiple answers
            // Current method only checks if user reacted to correct answer
            // CollectReactionsAsync() does not work for some reason
            // Getting users seperately for each answer works but is not efficient
            // Get users who reacted with correct answer
            var correctUsers = await sentQuestion.GetReactionsAsync(emoji[correctIdx]);

            await Task.Delay(1000);

            await context.Channel.SendMessageAsync($"Correct answer was **{question.correct_answer}**");

            return correctUsers;
        }

        /// <summary>
        /// Prints the leaderboard at the end of the game
        /// </summary>
        /// <param name="context">Context gotten from Discord Command</param>
        /// <param name="players">A dictionary of players and their scores</param>
        private async Task PrintLeaderboard(CommandContext context, Dictionary<DiscordUser, int> players, string title)
        {
            // Create leaderboard string
            var sortedScore = players.OrderByDescending(x => x.Value);
            string leaderboard = string.Empty;
            int i = 1;
            foreach (var player in sortedScore)
            {
                if (i > 3)
                {
                    break;
                }
                leaderboard += $"{i}. {player.Key.Mention} - {player.Value} points\n";
                i++;
            }

            // Print leaderboard
            var finalMessage = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = $"**The game has ended!\nWinner is: {sortedScore.First().Key.Mention}!**\n" +
                $"\n**Leaderboard:**\n" +
                $"{leaderboard}",
            };
            await context.Channel.SendMessageAsync(embed: finalMessage);
        }

    }
}
