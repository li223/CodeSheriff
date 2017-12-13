﻿using System;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using CodeSheriff.Helper;

namespace CodeSheriff
{
    public class Commands
    {
        [Command("ignoreme"), Description("This command tells the bot to ingore your code")]
        public async Task IgnoreMeAsync(CommandContext ctx)
        {
            var serviceClass = ctx.Services.GetRequiredService<ServiceClass>();
            var helper = ctx.Services.GetRequiredService<JsonHelper>();
            var ignored = serviceClass.Data.IgnoredUsers.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.GuildId == ctx.Guild.Id);
            if (ignored == null)
            {
                serviceClass.Data.IgnoredUsers.Add(new IgnoredUser()
                {
                    UserId = ctx.Member.Id,
                    GuildId = ctx.Guild.Id
                });
                helper.SaveData(serviceClass.Data);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("unignoreme"), Description("This command tells the bot to keep judging your code")]
        public async Task UnIgnoreMeAsync(CommandContext ctx)
        {
            var serviceClass = ctx.Services.GetRequiredService<ServiceClass>();
            var helper = ctx.Services.GetRequiredService<JsonHelper>();
            var ignored = serviceClass.Data.IgnoredUsers.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.GuildId == ctx.Guild.Id);
            if (ignored != null)
            {
                serviceClass.Data.IgnoredUsers.Remove(ignored);
                helper.SaveData(serviceClass.Data);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("add"), RequireOwnerOrMod, Description("Adds a keyword to the file.")]  //TODO allow pipe symbol for separation
        public async Task AddAsync(CommandContext ctx, string _keyword, params string[] reasons)
        {
            var serviceClass = ctx.Services.GetRequiredService<ServiceClass>();
            var helper = ctx.Services.GetRequiredService<JsonHelper>();
            var word = serviceClass.Data.FlaggedWords?.FirstOrDefault(x => x.Word == _keyword && x.GuildId == ctx.Guild.Id);

            if (word == null)
            {
                serviceClass.Data.FlaggedWords.Add(new FlaggedWord()
                {
                    GuildId = ctx.Guild.Id,
                    Word = _keyword,
                    Reasons = reasons.ToArray()
                });
                helper.SaveData(serviceClass.Data);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("remove"), RequireOwnerOrMod, Description("Removes a keyword from the database")]
        public async Task RemoveAsync(CommandContext ctx, string keyword)
        {
            var serviceClass = ctx.Services.GetRequiredService<ServiceClass>();
            var helper = ctx.Services.GetRequiredService<JsonHelper>();
            var word = serviceClass.Data.FlaggedWords.FirstOrDefault(x => x.Word == keyword && x.GuildId == ctx.Guild.Id);
            if (word != null)
            {
                serviceClass.Data.FlaggedWords.Remove(word);
                helper.SaveData(serviceClass.Data);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("shutdown"), Aliases("exit"), RequireOwner, Description("Shuts down the bot"), Hidden]
        public async Task ShutdownAsync(CommandContext ctx)
        {
            Console.WriteLine("Shutting down...");
            //Make the bot appear offline
            var serviceClass = ctx.Services.GetRequiredService<ServiceClass>();
            var helper = ctx.Services.GetRequiredService<JsonHelper>();
            helper.SaveData(serviceClass.Data);

            await ctx.Client.UpdateStatusAsync(userStatus: UserStatus.Invisible);
            await ctx.Client.DisconnectAsync();
        }
    }

    public class RequireOwnerOrMod : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.Run(() =>
            {
                var IsMod = ctx.Member.Roles.Any(x => x.Permissions.HasPermission(Permissions.ManageGuild));
                var IsBotOwner = ctx.Member.Id == ctx.Client.CurrentApplication.Owner.Id;
                if (IsMod || IsBotOwner) return true;
                else return false;
            });
        }
    }
}
