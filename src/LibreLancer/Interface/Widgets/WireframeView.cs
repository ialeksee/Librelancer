// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using LibreLancer.Utf.Cmp;
using WattleScript.Interpreter;

namespace LibreLancer.Interface
{
    [UiLoadable]
    [WattleScriptUserData]
    public class WireframeView : Widget3D
    {
        public WireframeView()
        {
            OrbitPan = Vector2.Zero;
            CanRotate = false;
        }

        public InterfaceColor WireframeColor { get; set; }


        private TargetShipWireframe target;
        public void SetWireframe(TargetShipWireframe target)
        {
            this.target = target;
        }
        
        public override void Render(UiContext context, RectangleF parentRectangle)
        {
            base.Render(context, parentRectangle);
            var rect = GetMyRectangle(context, parentRectangle);
            if (rect.Width <= 0 || rect.Height <= 0) return;
            Background?.Draw(context, rect);
            if (target != null) {
                Draw3DViewport(context, rect);
            }
            Border?.Draw(context, rect);
        }

        void DrawWires(UiContext context)
        {
            int i = 0;
            foreach (var part in target.Model.AllParts)
            {
                if (part.Wireframe != null)
                {
                    DrawVMeshWire(context, part.Wireframe, part.LocalTransform * target.Matrix);
                }
            }
        }
        void DrawVMeshWire(UiContext context, VMeshWire wires, Matrix4x4 mat)
        {
            var color = (WireframeColor ?? InterfaceColor.White).GetColor(context.GlobalTime);
            context.Lines.Color = color;
            for (int i = 0; i < wires.Lines.Length / 2; i++)
            {
                context.Lines.DrawLine(
                    Vector3.Transform(wires.Lines[i * 2],mat),
                    Vector3.Transform(wires.Lines[i * 2 + 1],mat)
                );
            }
        }
        
        protected override void Draw3DContent(UiContext context, RectangleF rect)
        {
            var cam = GetCamera(-target.Model.GetRadius() * 2.05f, context, rect);
            context.Lines.StartFrame(cam, context.RenderContext);
            DrawWires(context);
            context.Lines.Render();
        }
    }
}