using System;
using Terraria;
using TShockAPI;

namespace Nanami
{
    internal class NanamiListener : IDisposable
    {
        public NanamiListener()
        {
            GetDataHandlers.PlayerDamage += OnPlayerDamage;
            GetDataHandlers.KillMe += OnKillMe;
        }
        public void Dispose()
        {
            GetDataHandlers.PlayerDamage -= OnPlayerDamage;
            GetDataHandlers.KillMe -= OnKillMe;
        }

        static void OnPlayerDamage(object sender, GetDataHandlers.PlayerDamageEventArgs args)
        {
            var calculatedDmg = (int)Main.CalculateDamagePlayersTake(args.Damage, Main.player[args.ID].statDefense);
            PlayerPvpData.GetPlayerData(args.Player).Damage(calculatedDmg); // 记录 伤害量
            PlayerPvpData.GetPlayerData(args.ID).Hurt(calculatedDmg);       // 记录 承受伤害量
        }
        static void OnKillMe(object sender, GetDataHandlers.KillMeEventArgs args)
        {
            if (!args.Pvp)return;
            Console.WriteLine($"{args.PlayerId}-{args.Player.Name}");
            Console.WriteLine($"{args.PlayerDeathReason._sourcePlayerIndex}-{args.Player.Name}");
            args.Player.RespawnTimer = Nanami.Config.RespawnPvPSeconds;
            PlayerPvpData.GetPlayerData(args.Player).Die(args.Damage);// 处理死亡事件

            var killer = args.PlayerDeathReason._sourcePlayerIndex;
            var killerProj = args.PlayerDeathReason._sourceProjectileType;
            var killerItem = args.PlayerDeathReason._sourceItemType;

            var deathText = "被{0}的{1}杀死了!";

            if (killerProj != 0)
                deathText = string.Format(deathText, TShock.Players[killer].Name, Lang.GetProjectileName(killerProj));
            else if (killerItem != 0)
                deathText = string.Format(deathText, TShock.Players[killer].Name, Lang.GetItemNameValue(killerItem));
            else
                deathText = $"被{TShock.Players[killer].Name}杀死了！";

            PlayerPvpData.GetPlayerData(killer).Kill(ref deathText);// 处理杀死事件

            args.PlayerDeathReason._sourceCustomReason = args.Player.Name + deathText;

            Main.player[args.PlayerId].KillMe(args.PlayerDeathReason, args.Damage, args.Direction, true);
            NetMessage.SendPlayerDeath(args.PlayerId, args.PlayerDeathReason, args.Damage, args.Direction, true, -1, args.Player.Index);

            args.Handled = true;
        }
    }
}
