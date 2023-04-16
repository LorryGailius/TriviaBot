using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TriviaBot.External;

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
        public async Task StartTrivia(CommandContext context)
        {
            QuestionManager questionManager = new QuestionManager();

            var interactivity = context.Client.GetInteractivity();

            Dictionary<DiscordUser, int> score = new Dictionary<DiscordUser, int>();

            int responseCode = await questionManager.GetQuestions(5);

            if (responseCode != 0)
            {
                await context.Channel.SendMessageAsync(":red_square: **ERROR FETCHING QUESTIONS** :red_square:");
                return;
            }

            DiscordEmoji accept = DiscordEmoji.FromName(context.Client, ":white_check_mark:", false);
            TimeSpan timer = TimeSpan.FromSeconds(20);
            int timeLeft = 10;

            var joinMessage = new DiscordEmbedBuilder()
            {
                Title = $"{DiscordEmoji.FromName(context.Client, ":orange_circle:", false)} **DISCORD TRIVIA** {DiscordEmoji.FromName(context.Client, ":orange_circle:", false)}",
                Description = $"**To join the game please react with {accept}**\n**Good luck and have fun!**\n**Time remaining to join: {timeLeft} seconds!**",
            };

            var sentInvite = await context.Channel.SendMessageAsync(embed: joinMessage);
            timeLeft--;

            await sentInvite.CreateReactionAsync(accept);

            for (int i = 0; i <= timeLeft; timeLeft--)
            {
                await Task.Delay(1000);
                var newMessage = new DiscordEmbedBuilder()
                {
                    Title = $"{DiscordEmoji.FromName(context.Client, ":orange_circle:", false)} **DISCORD TRIVIA** {DiscordEmoji.FromName(context.Client, ":orange_circle:", false)}",
                    Description = $"**To join the game please react with {accept}**\n**Good luck and have fun!**\n**Time remaining to join: {timeLeft} seconds!**",
                };
                await sentInvite.ModifyAsync(new DiscordMessageBuilder().AddEmbed(newMessage));
            }

            int count = 0;
            var joinReaction = await sentInvite.GetReactionsAsync(accept);

            foreach (var user in joinReaction)
            {
                if (!user.IsBot)
                {
                    count++;
                    score.Add(user, 0);
                }
            }

            foreach (Question question in questionManager.questions)
            {
                List<string> answers = new List<string>();
                Random rng = new Random();
                int time = 10;

                // Randomize answers
                answers.Add(question.correct_answer);
                answers.AddRange(question.incorrect_answers);
                answers = answers.OrderBy(x => rng.Next()).ToList();

                DiscordEmoji[] emoji = {DiscordEmoji.FromName(context.Client, ":one:", false),
                                                DiscordEmoji.FromName(context.Client, ":two:", false),
                                                DiscordEmoji.FromName(context.Client, ":three:", false),
                                                DiscordEmoji.FromName(context.Client, ":four:", false),};

                var questionMessage = new DiscordEmbedBuilder()
                {
                    Title = $"{question.question}",
                    Description = $"Time remaining: {time} seconds",
                };

                var discordMessage = await context.Channel.SendMessageAsync(embed: questionMessage);
                time--;

                foreach (DiscordEmoji em in emoji)
                {
                    await discordMessage.CreateReactionAsync(em);
                }

                for (int i = 0; i <= time; time--)
                {
                    await Task.Delay(1000);
                    var newmessage = new DiscordEmbedBuilder()
                    {
                        Title = $"{question.question}",
                        Description = $"{emoji[0]} | {answers[0]}\n\n" + $"{emoji[1]} | {answers[1]}\n\n" + $"{emoji[2]} | {answers[2]}\n\n" + $"{emoji[3]} | {answers[3]}\n\n" + $"Time remaining: {time} seconds",
                    };
                    await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(newmessage));
                }

                //Find index with correct answer
                int correctIdx = answers.IndexOf(question.correct_answer);

                var correctUsers = await discordMessage.GetReactionsAsync(emoji[correctIdx]);

                foreach (var user in correctUsers)
                {
                    if (!user.IsBot)
                    {
                        score[user] += 1;
                    }
                }

                await Task.Delay(1000);

                await context.Channel.SendMessageAsync($"Correct answer was **{question.correct_answer}**");

                await Task.Delay(3000);
            }

            //Find user with highest score
            var winner = score.Aggregate((l, r) => l.Value > r.Value ? l : r);

            var finalMessage = new DiscordEmbedBuilder()
            {
                Title = $"{DiscordEmoji.FromName(context.Client, ":orange_circle:", false)} **DISCORD TRIVIA** {DiscordEmoji.FromName(context.Client, ":orange_circle:", false)}",
                Description = $"**The game has ended!\nWinner is: {winner.Key.Mention} with a score of {winner.Value}**",
            };

            await context.Channel.SendMessageAsync(embed: finalMessage);

        }
    }
}
