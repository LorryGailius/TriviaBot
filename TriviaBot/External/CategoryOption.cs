using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.External
{
    public struct CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Emoji { get; set; }

        public CategoryOption(string name, int id, string emoji)
        {
            Id = id;
            Name = name;
            Emoji = emoji;
        }

    }
}
