﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using Castle.DynamicProxy.Contributors;
using LibreLancer.Utf.Ale;
using LibreLancer.Utf.Mat;

namespace LibreLancer.Fx
{
	public class FxBasicAppearance : FxAppearance
	{
		public bool QuadTexture;
		public bool MotionBlur;
		public AlchemyColorAnimation Color;
		public AlchemyFloatAnimation Alpha;
		public AlchemyFloatAnimation HToVAspect;
		public AlchemyFloatAnimation Rotate;
		public AlchemyFloatAnimation Size;
		public BlendMode BlendInfo = BlendMode.Normal;
		public string Texture;
		public bool UseCommonAnimation = false;
		public AlchemyFloatAnimation Animation;
		public AlchemyCurveAnimation CommonAnimation;
		public bool FlipHorizontal = false;
		public bool FlipVertical = false;

		public FxBasicAppearance (AlchemyNode ale) : base(ale)
		{
			AleParameter temp;
			if (ale.TryGetParameter ("BasicApp_QuadTexture", out temp)) {
				QuadTexture = (bool)temp.Value;
			}
			if (ale.TryGetParameter("BasicApp_TriTexture", out temp))
			{
				if ((bool)temp.Value) {
					FLLog.Warning ("ALE", "BasicApp_TriTexture not implemented");
				}
			}
			if (ale.TryGetParameter ("BasicApp_MotionBlur", out temp)) {
				MotionBlur = (bool)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_Color", out temp)) {
				Color = (AlchemyColorAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_Alpha", out temp)) {
				Alpha = (AlchemyFloatAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_HtoVAspect", out temp)) {
				HToVAspect = (AlchemyFloatAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_Rotate", out temp)) {
				Rotate = (AlchemyFloatAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_TexName", out temp)) {
				Texture = (string)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_UseCommonTexFrame", out temp)) {
				UseCommonAnimation = (bool)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_TexFrame", out temp)) {
				Animation = (AlchemyFloatAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_CommonTexFrame", out temp)) {
				CommonAnimation = (AlchemyCurveAnimation)temp.Value;
			}
			if (ale.TryGetParameter ("BasicApp_FlipTexU", out temp)) {
				FlipHorizontal = (bool)temp.Value;
			}
			if (ale.TryGetParameter("BasicApp_FlipTexV", out temp)) {
				FlipVertical = (bool)temp.Value;
			}
			if (ale.TryGetParameter("BasicApp_Size", out temp)) {
				Size = (AlchemyFloatAnimation)temp.Value;
			}
			if (ale.TryGetParameter("BasicApp_BlendInfo", out temp)) {
				BlendInfo = BlendMap.Map((Tuple<uint, uint>)temp.Value);
			}
		}

        public override void Draw(ref Particle particle, int pidx, float lasttime, float globaltime, NodeReference reference, ResourceManager res, ParticleEffectInstance instance, ref Matrix4x4 transform, float sparam)
        {
            var time = particle.TimeAlive / particle.LifeSpan;
            var node_tr = GetAttachment(reference, transform);

            Vector3 deltap;
            Quaternion deltaq;
            if(DoTransform(reference,sparam,lasttime,globaltime,out deltap, out deltaq)) {
                particle.Position += deltap;
                particle.Orientation *= deltaq;
            }
			var p = Vector3.Transform(Vector3.Transform(particle.Position, particle.Orientation), node_tr);
            TextureHandler.Update(Texture, res);
			var c = Color.GetValue(sparam, time);
			var a = Alpha.GetValue(sparam, time);
			instance.Pool.DrawBasic(
                particle.Instance,
                this,
                TextureHandler,
				p,
				new Vector2(Size.GetValue(sparam, time)) * 2,
				new Color4(c, a),
				GetFrame(globaltime, sparam, ref particle),
                Rotate == null ? 0f : MathHelper.DegreesToRadians(Rotate.GetValue(sparam, time)),
                reference.Index
			);
		}

        public ParticleTexture TextureHandler = new ParticleTexture();

        protected float GetFrame(float globaltime, float sparam, ref Particle particle)
        {
            float frame = 0;
            if (UseCommonAnimation)
            {
                frame = CommonAnimation.GetValue(sparam, globaltime);
            }
            else
            {
                frame = Animation.GetValue(sparam, particle.TimeAlive / particle.LifeSpan);
            }
            return  MathHelper.Clamp(frame, 0, 1);
        }
    }
}

