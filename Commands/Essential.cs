using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle(":information_source: **DISCORD TRIVIA** :information_source:")
                .WithDescription("To play: the game type ***!trivia*** into the chat!\n ")
                );

            await context.Channel.SendMessageAsync(message);
        }


    }
}
