// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using LibreLancer.Utf.Anm;
using LibreLancer.Utf.Cmp;
using LibreLancer.Utf.Dfm;

namespace LibreLancer
{
    public class DfmSkeletonManager
    {
        public float FloorHeight;
        public float RootHeight;
        
        public DfmFile Head;
        public DfmFile Body;
        public DfmFile LeftHand;
        public DfmFile RightHand;
        
        public DfmSkinning HeadSkinning;
        public DfmSkinning BodySkinning;
        public DfmSkinning LeftHandSkinning;
        public DfmSkinning RightHandSkinning;
        
        //Connect Head
        private HardpointDefinition HeadBodyHp;
        private BoneInstance HeadBodyBone;
        private HardpointDefinition BodyHeadHp;
        private BoneInstance BodyHeadBone;
        //Connect Left Hand
        private HardpointDefinition LeftBodyHp;
        private BoneInstance LeftBodyBone;
        private HardpointDefinition BodyLeftHp;
        private BoneInstance BodyLeftBone;
        //Connect Right Hand
        private HardpointDefinition RightBodyHp;
        private BoneInstance RightBodyBone;
        private HardpointDefinition BodyRightHp;
        private BoneInstance BodyRightBone;
        //Scripting
        List<ScriptInstance> RunningScripts = new List<ScriptInstance>();
        struct ResolvedJoint
        {
            public BoneInstance Bone;
            public JointMap JointMap;
            public ResolvedJoint(BoneInstance bone, JointMap jm)
            {
                Bone = bone;
                JointMap = jm;
            }
        }

        //Used for object maps
        public bool ApplyRootMotion => _rootMotionInstance != null;
        public Vector3 RootTranslation => _rootMotionInstance.RootTranslation;

        public Vector3 RootTranslationOrigin
        {
            get => _rootMotionInstance.RootTranslationOrigin;
            set => _rootMotionInstance.RootTranslationOrigin = value;
        }

        public Quaternion RootRotation => _rootMotionInstance.RootRotation;

        public Quaternion RootRotationOrigin
        {
            get => _rootMotionInstance.RootRotationOrigin;
            set => _rootMotionInstance.RootRotationOrigin = value;
        }

        private ScriptInstance _rootMotionInstance;
        class ScriptInstance
        {
            public double T;
            public float Duration;
            public List<ObjectMap> ObjectMaps = new List<ObjectMap>();
            public List<ResolvedJoint> Joints = new List<ResolvedJoint>();
            public DfmSkeletonManager Parent;
            public Vector3 RootTranslation;
            public Vector3 RootTranslationOrigin;
            public Quaternion RootRotation = Quaternion.Identity;
            public Quaternion RootRotationOrigin = Quaternion.Identity;
            public bool RunScript(TimeSpan delta)
            {
                T += delta.TotalSeconds;
                var ft = (float) T;
                bool running = false;
                foreach (var j in Joints)
                {
                    var ch = j.JointMap.Channel;
                    var cht = ft;
                    if (Duration > 0) cht = ft % ch.Duration;
                    if (ch.HasOrientation)
                        j.Bone.Rotation = ch.QuaternionAtTime(cht);
                    if (ch.HasPosition)
                        j.Bone.Translation = ch.PositionAtTime(cht);
                    if (ft < ch.Duration)
                        running = true;
                }
                
                foreach (var o in ObjectMaps)
                {
                    if (!o.ParentName.Equals("Root", StringComparison.OrdinalIgnoreCase)) continue;
                    Vector3 translate = Vector3.Zero;
                    Quaternion rotate = Quaternion.Identity;
                    var cht = ft;
                    if (Duration > 0)
                    {
                        cht = ft % o.Channel.Duration;
                        if (ft > o.Channel.Duration && (Math.Abs(o.Channel.Duration - cht) < 0.001))
                            cht = 0;
                    }
                    if (o.Channel.HasPosition) translate = o.Channel.PositionAtTime(cht);
                    if (o.Channel.HasOrientation) rotate = o.Channel.QuaternionAtTime(cht);
                    if (Duration > 0)
                    {
                        var timesPassed = (int)Math.Floor(ft / o.Channel.Duration);
                        if (timesPassed > 0)
                        {
                            var trOne = Vector3.Zero;
                            Quaternion qOne;
                            if (o.Channel.HasPosition) trOne = o.Channel.PositionAtTime(o.Channel.Duration);
                            if (o.Channel.HasOrientation) qOne = o.Channel.QuaternionAtTime(o.Channel.Duration);
                            for (int i = 0; i < timesPassed; i++) {
                                translate += trOne;
                            }
                        }
                    }
                    RootTranslation = translate;
                    RootRotation = rotate;
                    Parent._rootMotionInstance = this;
                }
                if (Duration > 0)
                    return T < Duration;
                else
                    return running;
            }
        }

        public DfmSkeletonManager(DfmFile body, DfmFile head = null, DfmFile leftHand = null, DfmFile rightHand = null)
        {
            Body = body;
            BodySkinning = new DfmSkinning(body);
            if (head != null)
            {
                Head = head;
                HeadSkinning = new DfmSkinning(head);
            }
            if (leftHand != null)
            {
                LeftHand = leftHand;
                LeftHandSkinning = new DfmSkinning(leftHand);
            }
            if (rightHand != null)
            {
                RightHand = rightHand;
                RightHandSkinning = new DfmSkinning(rightHand);
            }
            ConnectBones();
        }

        void ConnectBones()
        {
            if (Head != null)
            {
                HeadSkinning.GetHardpoint("hp_head", out HeadBodyHp, out HeadBodyBone);
                BodySkinning.GetHardpoint("hp_head", out BodyHeadHp, out BodyHeadBone);
            }
            if (LeftHand != null)
            {
                LeftHandSkinning.GetHardpoint("hp_left b", out LeftBodyHp, out LeftBodyBone);
                BodySkinning.GetHardpoint("hp_left b", out BodyLeftHp, out BodyLeftBone);
            }
            if (RightHand != null)
            {
                RightHandSkinning.GetHardpoint("hp_right b", out RightBodyHp, out RightBodyBone);
                BodySkinning.GetHardpoint("hp_right b", out BodyRightHp, out BodyRightBone);
            }
        }
        //A - attaching object, B - attaching to
        Matrix4 GetAttachmentTransform(HardpointDefinition hpA, BoneInstance boneA, HardpointDefinition hpB, BoneInstance boneB)
        {
            var child = hpA.Transform * boneA.LocalTransform();
            child.Invert();
            var parent = hpB.Transform * boneB.LocalTransform();
            return child * parent;
        }
        
        public void UpdateScripts(TimeSpan delta)
        {
            _rootMotionInstance = null;
            List<ScriptInstance> toRemove = new List<ScriptInstance>();
            foreach (var sc in RunningScripts)
            {
                if(!sc.RunScript(delta)) toRemove.Add(sc);
            }
            foreach(var sc in toRemove) RunningScripts.Remove(sc);
        }
        
        public void UploadBoneData(UniformBuffer bonesBuffer)
        {
            int off = 0;
            BodySkinning.SetBoneData(bonesBuffer, ref off);
            if (Head != null)
            {
                HeadSkinning.SetBoneData(bonesBuffer, ref off);
            }
            if (LeftHand != null)
            {
                LeftHandSkinning.SetBoneData(bonesBuffer, ref off);   
            }
            if (RightHand != null)
            {
                RightHandSkinning.SetBoneData(bonesBuffer, ref off);
            }
        }

        public void GetTransforms(Matrix4 source, out Matrix4 head, out Matrix4 leftHand, out Matrix4 rightHand)
        {
            if (Head != null)
                head = GetAttachmentTransform(HeadBodyHp, HeadBodyBone, BodyHeadHp, BodyHeadBone) * source;
            else
                head = source;
            if (LeftHand != null)
                leftHand = GetAttachmentTransform(LeftBodyHp, LeftBodyBone, BodyLeftHp, BodyLeftBone) * source;
            else
                leftHand = source;
            if (RightHand != null)
                rightHand = GetAttachmentTransform(RightBodyHp, RightBodyBone, BodyRightHp, BodyRightBone) * source;
            else
                rightHand = source;
        }
        
        public void StartScript(Script anmScript, float duration)
        {
            if(anmScript.HasRootHeight) RootHeight = anmScript.RootHeight;
            var inst = new ScriptInstance();
            inst.Duration = duration;
            inst.ObjectMaps = anmScript.ObjectMaps;
            inst.Parent = this;
            foreach (var jm in anmScript.JointMaps)
            {
                if (BodySkinning.Bones.TryGetValue(jm.ChildName, out BoneInstance bb))
                    inst.Joints.Add(new ResolvedJoint(bb, jm));
                else if (Head != null && HeadSkinning.Bones.TryGetValue(jm.ChildName, out BoneInstance bh))
                    inst.Joints.Add(new ResolvedJoint(bh, jm));
                else if (LeftHand != null && LeftHandSkinning.Bones.TryGetValue(jm.ChildName, out BoneInstance bl))
                    inst.Joints.Add(new ResolvedJoint(bl, jm));
                else if (RightHand != null && RightHandSkinning.Bones.TryGetValue(jm.ChildName, out BoneInstance br))
                    inst.Joints.Add(new ResolvedJoint(br, jm));
            }
            RunningScripts.Add(inst);
        }
        
    }
}