// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using LibreLancer.Ini;
using LibreLancer.Render;
using WattleScript.Interpreter;

namespace LibreLancer
{
    [WattleScriptUserData]
    public class GameSettings : IRendererSettings
    {
        [Entry("master_volume")]
        public float MasterVolume = 1.0f;
        [Entry("sfx_volume")]
        public float SfxVolume = 1.0f;
        [Entry("music_volume")]
        public float MusicVolume = 1.0f;
        [Entry("vsync")]
        public bool VSync = true;
        [Entry("anisotropy")]
        public int Anisotropy = 0;
        [Entry("msaa")]
        public int MSAA = 0;

        int IRendererSettings.SelectedAnisotropy => Anisotropy;
        TextureFiltering IRendererSettings.SelectedFiltering =>
            Anisotropy == 0 ? TextureFiltering.Trilinear : TextureFiltering.Anisotropic;
        int IRendererSettings.SelectedMSAA => MSAA;
        
        public int[] AnisotropyLevels() => RenderContext.GetAnisotropyLevels();
        public int MaxMSAA() => RenderContext.MaxSamples;

        [WattleScriptHidden]
        public void Write(TextWriter writer)
        {
            static string Fmt(float f) => f.ToString("F3", CultureInfo.InvariantCulture);
            writer.WriteLine("[Settings]");
            writer.WriteLine($"master_volume = {Fmt(MasterVolume)}");
            writer.WriteLine($"sfx_volume = {Fmt(SfxVolume)}");
            writer.WriteLine($"music_volume = {Fmt(MusicVolume)}");
            writer.WriteLine($"vsync = {(VSync ? "true" : "false")}");
            writer.WriteLine($"anisotropy = {Anisotropy}");
            writer.WriteLine($"msaa = {MSAA}");
        }
        
        [WattleScriptHidden] 
        public RenderContext RenderContext;
        
        [WattleScriptHidden]
        public GameSettings MakeCopy()
        {
            var gs = new GameSettings();
            gs.MasterVolume = MasterVolume;
            gs.SfxVolume = SfxVolume;
            gs.MusicVolume = MusicVolume;
            gs.VSync = VSync;
            gs.Anisotropy = Anisotropy;
            gs.MSAA = MSAA;
            gs.RenderContext = RenderContext;
            return gs;
        }
    }
}