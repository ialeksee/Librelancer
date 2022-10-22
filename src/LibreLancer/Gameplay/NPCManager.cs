using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LibreLancer.AI;
using LibreLancer.Data.Solar;

namespace LibreLancer
{
    public class NPCManager
    {
        private NPCWattleScripting scripting;
        public ServerWorld World;
        public NPCManager(ServerWorld world)
        {
            this.World = world;
            scripting = new NPCWattleScripting(this);
        }

        public Task<string> RunScript(string src)
        {
            TaskCompletionSource<string> source = new TaskCompletionSource<string>();
            World.EnqueueAction(() =>
            {
                source.SetResult(scripting.Run(src));
            });
            return source.Task;
        }
        
        public void Despawn(GameObject obj)
        {
            World.RemoveNPC(obj);
        }

        private Dictionary<string, GameObject> missionNPCs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

        public void NpcDoAction(string nickname, Action<GameObject> act)
        {
            World.EnqueueAction(() =>
            {
                act(missionNPCs[nickname]);
            });
        }
        public GameObject DoSpawn(string nickname, Loadout loadout, GameData.Pilot pilot, Vector3 position, Quaternion orient, MissionRuntime msn = null)
        {
            NetShipLoadout netLoadout = new NetShipLoadout();
            netLoadout.Items = new List<NetShipCargo>();
            var ship = World.Server.GameData.GetShip(loadout.Archetype);
            netLoadout.ShipCRC = ship.CRC;
            var obj = new GameObject(ship, World.Server.Resources, false, true);
            obj.Name = $"Bob NPC - {loadout.Nickname}";
            obj.Nickname = nickname;
            obj.SetLocalTransform(Matrix4x4.CreateFromQuaternion(orient) * Matrix4x4.CreateTranslation(position));
            obj.Components.Add(new SHealthComponent(obj)
            {
                CurrentHealth = ship.Hitpoints,
                MaxHealth = ship.Hitpoints
            });
            foreach (var equipped in loadout.Equip)
            {
                var e = World.Server.GameData.GetEquipment(equipped.Nickname);
                if (e == null) continue;
                EquipmentObjectManager.InstantiateEquipment(obj, World.Server.Resources, null, EquipmentType.Server,
                    equipped.Hardpoint, e);
                netLoadout.Items.Add(new NetShipCargo(0, e.CRC, equipped.Hardpoint ?? "internal", 255, 1));
            }
            var npcComponent = new SNPCComponent(obj, this) {Loadout = netLoadout, MissionRuntime = msn};
            npcComponent.SetPilot(pilot);
            obj.Components.Add(npcComponent);            
            obj.Components.Add(new AutopilotComponent(obj));
            obj.Components.Add(new ShipSteeringComponent(obj));
            obj.Components.Add(new ShipPhysicsComponent(obj) { Ship = ship });
            obj.Components.Add(new WeaponControlComponent(obj));
            World.OnNPCSpawn(obj);
            if (nickname != null) missionNPCs[nickname] = obj;
            return obj;
        }
    }
}