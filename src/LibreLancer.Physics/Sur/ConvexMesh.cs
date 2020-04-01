﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package
using System;
using System.Numerics;
namespace LibreLancer.Physics.Sur
{
    public class ConvexMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public uint ParentCrc;
    }
}
