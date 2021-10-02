// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using System.Collections.Generic;
using LibreLancer.GameData.Items;
using LibreLancer.Utf.Cmp;

namespace LibreLancer
{
    /// <summary>
    /// Return a GameObject only if you add one to the parent
    /// </summary>
    public delegate GameObject MountEquipmentHandler(GameObject parent, ResourceManager res, EquipmentType type, string hardpoint, Equipment equip);

    public enum EquipmentType
    {
        Server,
        RemoteObject,
        LocalPlayer,
        Cutscene
    }
    public class EquipmentObjectManager
    {
        static Dictionary<Type, MountEquipmentHandler> handlers = new Dictionary<Type, MountEquipmentHandler>();
        public static void RegisterType<T>(MountEquipmentHandler handler)
        {
            handlers.Add(typeof(T), handler);
        }
        public static void InstantiateEquipment(GameObject parent, ResourceManager res, EquipmentType type, string hardpoint, Equipment equip)
        {
            var etype = equip.GetType();
            if (!handlers.TryGetValue(etype, out var handle))
            {
                FLLog.Error("Equipment", $"Cannot instantiate {etype}");
                return;
            }
            var obj = handle(parent, res, type, hardpoint, equip);
            //Do setup of child attachment, hardpoint, lod inheriting, static position etc.
            if (obj != null)
            {
                obj.Parent = parent;
                parent.Children.Add(obj);
                if (equip.LODRanges != null && obj.RenderComponent != null)
                    obj.RenderComponent.LODRanges = equip.LODRanges;
                if(equip.HPChild != null)
                {
                    Hardpoint hpChild = obj.GetHardpoint(equip.HPChild);
                    if (hpChild != null)
                    {
                        Matrix4x4.Invert(hpChild.Transform, out var invTr);
                        obj.SetLocalTransform(invTr);
                    }
                }
                var hp = parent.GetHardpoint(hardpoint);
                obj.Attachment = hp;
                if(obj.RenderComponent is ModelRenderer && parent.RenderComponent != null)
                {
                    if (parent.RenderComponent.LODRanges != null)
                    {
                        obj.RenderComponent.InheritCull = true;
                    }
                    else if (parent.RenderComponent is ModelRenderer)
                    {
                        var mr = (ModelRenderer)parent.RenderComponent;
                        //if (mr.Model.Mesh != null && mr.Model.Switch2 != null)
                         //  obj. RenderComponent.InheritCull = true;
                        //if(mr.CmpParts != null)
                        //{
                            /*Part parentPart = null;
                            if (hp.parent != null)
                                parentPart = mr.CmpParts.Find((o) => o.ObjectName == hp.parent.ChildName);
                            else
                                parentPart = mr.CmpParts.Find((o) => o.ObjectName == "Root");
                            if (parentPart.Model.Switch2 != null)
                                obj.RenderComponent.InheritCull = true;*/
                        //}
                    }
                }
 
            }
        }
    }
}