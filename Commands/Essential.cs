using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

            int responseCode = await questionManager.GetQuestions();

            if(responseCode != 0)
            {
                await context.Channel.SendMessageAsync(":red_square: **ERROR FETCHING QUESTIONS** :red_square:");
                return;
            }

            foreach(Question question in questionManager.questions)
            {
                List<string> answers = new List<string>();
                Random rng = new Random();

                // Randomize answers
                answers.Add(question.correct_answer);
                answers.AddRange(question.incorrect_answers);
                answers = answers.OrderBy(x => rng.Next()).ToList();

                DiscordEmoji[] emoji = {DiscordEmoji.FromName(context.Client, ":one:", false),
                                                DiscordEmoji.FromName(context.Client, ":two:", false),
                                                DiscordEmoji.FromName(context.Client, ":three:", false),
                                                DiscordEmoji.FromName(context.Client, ":four:", false),};

                var message = new DiscordEmbedBuilder()
                {
                    Title = $"{question.question}",
                    Description = $"{emoji[0]} | {answers[0]}\n\n" + $"{emoji[1]} | {answers[1]}\n\n" + $"{emoji[2]} | {answers[2]}\n\n" + $"{emoji[3]} | {answers[3]}\n\n" + $"",
                };

                await context.Channel.SendMessageAsync(embed: message);

                await Task.Delay(10000);
            }
        }


    }
}
