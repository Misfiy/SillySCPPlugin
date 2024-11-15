﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
using Scp914;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using Features = Exiled.API.Features;
using Random = UnityEngine.Random;

namespace SillySCP.Handlers
{
    public class Player : IRegisterable
    {
        public static Player Instance { get; private set; }
        
        public void Init()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Died += OnPlayerDead;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += OnScp914UpgradeInv;
            Exiled.Events.Handlers.Player.Escaping += OnEscaping;
            Exiled.Events.Handlers.Scp106.Attacking += OnScp106Attacking;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDead;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= OnScp914UpgradeInv;
            Exiled.Events.Handlers.Player.Escaping -= OnEscaping;
            Exiled.Events.Handlers.Scp106.Attacking -= OnScp106Attacking;
        }

        private void OnScp106Attacking(AttackingEventArgs ev)
        {
            if (!ev.Target.GetEffect(EffectType.Traumatized).IsEnabled)
            {
                ev.Target.EnableEffect(EffectType.PocketCorroding);
            }
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player.Role.Type == RoleTypeId.FacilityGuard && ev.Player.IsCuffed)
            {
                ev.IsAllowed = true;
                ev.NewRole = RoleTypeId.ChaosConscript;
            }
        }

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (!Round.IsEnded && Round.IsStarted && ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(RespawnSystem.RespawnTimer(ev.Player));
            }
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (!ev.Player.IsAlive && Round.IsStarted)
            {
                Timing.RunCoroutine(RespawnSystem.RespawnTimer(ev.Player));
            }

            if (ev.Player.Role == RoleTypeId.ClassD)
            {
                int random = Random.Range(1, 1_000_000);
                if (random == 1)
                {
                    ev.Player.Scale = new(ev.Player.Scale.x * 2, ev.Player.Scale.y, ev.Player.Scale.z);
                }
            }
        }

        private void OnPlayerDead(DiedEventArgs ev)
        {
            Timing.RunCoroutine(RespawnSystem.RespawnTimer(ev.Player));
        }
        
        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason == SpawnReason.Escaped)
            {
                PriorityInventoryModule.Main(ev.Player, ev.Items);
            }
        }
        
        private void OnScp914UpgradeInv(UpgradingInventoryItemEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Item.Type == ItemType.Coin)
            {
                var randomNum = Random.Range(1, 3);
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.Flashlight);
                        break;
                    }
                    case 2:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.Radio);
                    }
                        break;
                    case 3:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.KeycardJanitor);
                        break;
                    }
                }
            }
        }

        public IEnumerator<float> StartNukeDamage(Features.Player player)
        {
            yield return Timing.WaitForSeconds(180);
            while(player.CurrentRoom.Type == RoomType.HczNuke)
            {
                player.Hurt(1f);
                yield return Timing.WaitForSeconds(1);
            }

            yield return 0;
        }
    }
}