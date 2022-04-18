using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Immunity
{
    public partial class Immunity : Form
    {
        public class Variables
        {
            public BackgroundWorker builder = new BackgroundWorker();
            public Manager.Threads manager = new Manager.Threads();
            public Dictionary<string,List<string>> dump = new Dictionary<string, List<string>>();
            public Popup.Wait waiter = new Popup.Wait();
            public Popup.Error select_all = new Popup.Error("Be careful some configuration settings will be in conflict");
        }

        Variables variables = new Variables();

        public Immunity()
        {
            InitializeComponent();
            InitializeUi();
            InitializeWorker();
        }

        private void InitializeWorker()
        {
            variables.builder.DoWork += new DoWorkEventHandler(build);
        }

        private void InitializeUi()
        {
            Region = Region.FromHrgn(Manager.Ui.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            label_version.Text = Manager.Ui.version;
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private Engine.Item templates(string value)
        {
            Dictionary<string, Engine.Item> templates = new Dictionary<string, Engine.Item>()
            {
                { "FOV scaling: native horizontal", new Engine.Item(Engine.Types.get(Engine.Types.local_player, true), "AspectRatioAxisConstraint", "AspectRatio_MaintainYFOV") },
                { "FOV scaling: native vertical", new Engine.Item(Engine.Types.get(Engine.Types.local_player, true), "AspectRatioAxisConstraint", "AspectRatio_MaintainXFOV") },

                { "Screen mod: borderless", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.FullScreenMode", 1) },
                { "Screen mod: fullscreen", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.FullScreenMode", 0) },

                { "Anti-aliasing (AA): disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 0) },
                { "Anti-aliasing (AA): very low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 1) },
                { "Anti-aliasing (AA): low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 2) },
                { "Anti-aliasing (AA): medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 3) },
                { "Anti-aliasing (AA): high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 4) },
                { "Anti-aliasing (AA): very high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 5) },
                { "Anti-aliasing (AA): maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.PostProcessAAQuality", 6) },

                { "Verify peer: disable", new Engine.Item(Engine.Types.get(Engine.Types.engine_network_settings, true), "n.VerifyPeer", false) },
                { "Verify peer: enable", new Engine.Item(Engine.Types.get(Engine.Types.engine_network_settings, true), "n.VerifyPeer", true) },

                { "Anti-aliasing (FXAA): disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DefaultFeature.AntiAliasing", 0) },
                { "Anti-aliasing (FXAA): enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DefaultFeature.AntiAliasing", 1) },

                { "Image sharpening: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 0.0) },
                { "Image sharpening: very low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 1.0) },
                { "Image sharpening: low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 3.0) },
                { "Image sharpening: medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 5.0) },
                { "Image sharpening: high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 8.0) },
                { "Image sharpening: maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Sharpen", 10.0) },

                { "Anisotropic filtering (AF): disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 0) },
                { "Anisotropic filtering (AF): very low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 2) },
                { "Anisotropic filtering (AF): low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 4) },
                { "Anisotropic filtering (AF): medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 8) },
                { "Anisotropic filtering (AF): high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 12) },
                { "Anisotropic filtering (AF): maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MaxAnisotropy", 16) },

                { "Vertical sync (Vsync): disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.VSync", 0) },
                { "Vertical sync (Vsync): enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.VSync", 1) },

                { "Smooth frame rate: disable", new Engine.Item(Engine.Types.get(Engine.Types.engine_engine, true), "bSmoothFrameRate", 0) },
                { "Smooth frame rate: enable", new Engine.Item(Engine.Types.get(Engine.Types.engine_engine, true), "bSmoothFrameRate", 1) },

                { "Frame rate cap: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "t.MaxFPS", 0) },
                { "Frame rate cap: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "t.MaxFPS", 1) },

                { "Adaptive exposure: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.EyeAdaptationQuality", 0) },
                { "Adaptive exposure: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.EyeAdaptationQuality", 1) },

                { "Chromatic aberration: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.SceneColorFringeQuality", 0) },
                { "Chromatic aberration: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.SceneColorFringeQuality", 1) },

                { "Depth of field: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DepthOfFieldQuality", 0) },
                { "Depth of field: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DepthOfFieldQuality", 1) },

                { "Film grain: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.GrainQuantization", 0) },
                { "Film grain: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.GrainQuantization", 1) },

                { "Lens flare: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.LensFlareQuality", 0) },
                { "Lens flare: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.LensFlareQuality", 1) },

                { "Tonemapping: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 0) },
                { "Tonemapping: very low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 1) },
                { "Tonemapping: low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 2) },
                { "Tonemapping: medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 3) },
                { "Tonemapping: high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 4) },
                { "Tonemapping: maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Tonemapper.Quality", 5) },

                { "Motion blur: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DefaultFeature.MotionBlur", 0) },
                { "Motion blur: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.DefaultFeature.MotionBlur", 1) },

                { "Motion blur quality: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MotionBlurQuality", 0) },
                { "Motion blur quality: low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MotionBlurQuality", 1) },
                { "Motion blur quality: medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MotionBlurQuality", 2) },
                { "Motion blur quality: high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MotionBlurQuality", 3) },
                { "Motion blur quality: maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.MotionBlurQuality", 4) },

                { "Allow occlusion queries: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.AllowOcclusionQueries", 0) },
                { "Allow occlusion queries: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.AllowOcclusionQueries", 1) },

                { "View distance scale: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 0) },
                { "View distance scale: very low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 1) },
                { "View distance scale: low", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 3) },
                { "View distance scale: medium", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 5) },
                { "View distance scale: high", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 7) },
                { "View distance scale: maximum", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.ViewDistanceScale", 10) },

                { "Fog: disable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Fog", 0) },
                { "Fog: enable", new Engine.Item(Engine.Types.get(Engine.Types.system_settings, false), "r.Fog", 1) },

                { "Shadow quality: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.ShadowQuality", false) },
                { "Shadow quality: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.ShadowQuality", true) },

                { "Ambient occlusion: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.DefaultFeature.AmbientOcclusion", false) },
                { "Ambient occlusion: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.DefaultFeature.AmbientOcclusion", true) },

                { "Auto exposure: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.DefaultFeature.AutoExposure", false) },
                { "Auto exposure: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.DefaultFeature.AutoExposure", true) },

                { "Max anisotropy: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.MaxAnisotropy", 0) },
                { "Max anisotropy: low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.MaxAnisotropy", 2) },
                { "Max anisotropy: medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.MaxAnisotropy", 4) },
                { "Max anisotropy: high", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.MaxAnisotropy", 6) },
                { "Max anisotropy: maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.MaxAnisotropy", 8) },

                { "Early Z pass: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.EarlyZPass", 0) },
                { "Early Z pass: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.EarlyZPass", 1) },
                { "Early Z pass movable: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.EarlyZPassMovable", false) },
                { "Early Z pass movable: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.EarlyZPassMovable", true) },
                
                { "Screen space ambient occlusion smart blur (SSAO): disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSAOSmartBlur", 0) },
                { "Screen space ambient occlusion smart blur (SSAO): enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSAOSmartBlur", 1) },

                { "Hierarchical Z-Buffer Occlusion (HZB): disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.HZBOcclusion", 0) },
                { "Hierarchical Z-Buffer Occlusion (HZB): enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.HZBOcclusion", 1) },

                { "Ambient occlusion levels: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.AmbientOcclusionLevels", 0) },
                { "Ambient occlusion levels: low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.AmbientOcclusionLevels", 1) },
                { "Ambient occlusion levels: medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.AmbientOcclusionLevels", 2) },
                { "Ambient occlusion levels: high", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.AmbientOcclusionLevels", 3) },
                { "Ambient occlusion levels: maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.AmbientOcclusionLevels", 4) },

                { "Bloom quality: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 0) },
                { "Bloom quality: very low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 1) },
                { "Bloom quality: low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 2) },
                { "Bloom quality: medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 3) },
                { "Bloom quality: high", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 4) },
                { "Bloom quality: maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.BloomQuality", 5) },

                { "Screen space reflection quality (SSR): disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSR.Quality", 0) },
                { "Screen space reflection quality (SSR): enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSR.Quality", 1) },

                { "Subsurface scattering shading scale (SSS): disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 0) },
                { "Subsurface scattering shading scale (SSS): very low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 1) },
                { "Subsurface scattering shading scale (SSS): low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 3) },
                { "Subsurface scattering shading scale (SSS): medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 5) },
                { "Subsurface scattering shading scale (SSS): high", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 7) },
                { "Subsurface scattering shading scale (SSS): maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.SSS.Scale", 10) },

                { "True sky quality: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 0.0) },
                { "True sky quality: very low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 0.1) },
                { "True sky quality: low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 0.3) },
                { "True sky quality: medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 0.5) },
                { "True sky quality: high", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 0.7) },
                { "True sky quality: maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.TrueSkyQuality", 1) },

                { "Up sample quality: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.UpsampleQuality", 0) },
                { "Up sample quality: enable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.UpsampleQuality", 1) },

                { "Shadow max resolution: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.Shadow.MaxResolution", 0) },
                { "Shadow max resolution: low", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.Shadow.MaxResolution", 128) },
                { "Shadow max resolution: medium", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.Shadow.MaxResolution", 512) },
                { "Shadow max resolution: maximum", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.Shadow.MaxResolution", 2048) },

                { "Reflection environment: disable", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.ReflectionEnvironment", 0) },
                { "Reflection environment: blend with scene", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.ReflectionEnvironment", 1) },
                { "Reflection environment: overwrite scene", new Engine.Item(Engine.Types.get(Engine.Types.renderer_settings, true), "r.ReflectionEnvironment", 2) },
            };
            //r.DetailMode = 0
            //r.SSS.SampleS
            //r.LensFlareQuality = 0
            //r.oneframethreadlag = 1
            //r.simpledynamiclighting = 1
            //r.LightShaftQuality = 0
            //r.RefractionQuality = 0
            //r.ExposureOffset = 0.3

            return (templates[value]);
        }
        
        private void button_build_Click(object sender, EventArgs e)
        {
            variables.manager.set_checkedlistbox(checkboxes_modules, false);
            variables.manager.button(button_build, false);
            variables.builder.RunWorkerAsync();

            while (variables.builder.IsBusy == true)
            {
                Application.DoEvents();
            }
            variables.manager.set_checkedlistbox(checkboxes_modules, true);
            variables.manager.button(button_build, true);
        }

        private void dump()
        {
            string date = DateTime.Now.ToString("hmmssddMMyyyy");
            List<string> data = stacker(variables.dump);

            File.WriteAllLines($"Engine_{date}.ini", data);
        }

        private void header()
        {
            List<string> content = new List<string>()
            {
                $"; Engine file generated by Immunity v{Manager.Ui.version}",
                "; Immunity author: Neo",
                "; All information in Elevatia's server",
            };

            variables.dump.Add("; Immunity", content);
        }

        private List<string> stacker(Dictionary<string, List<string>> data)
        {
            List<string> stacked = new List<string>();
            bool header = true;

            foreach (KeyValuePair<string, List<string>> item in data)
            {
                if (header == true)
                {
                    stacked.Add($"{item.Key}");
                    header = false;
                }
                else
                {
                    stacked.Add($"\n{item.Key}");
                }
                
                foreach (string element in item.Value)
                {
                    stacked.Add(element);
                }
            }

            return (stacked);
        }

        private void reset()
        {
            if (variables.dump.Count > 0)
                variables.dump.Clear();
            header();
        }

        private void build(object sender, EventArgs e)
        {
            CheckedListBox box = variables.manager.get_checkedlistbox(checkboxes_modules);
            Engine.Item item = null;
            List<string> list = null;
            string type = null;

            reset();
            foreach (string data in box.CheckedItems)
            {
                item = templates(data);
                type = $"[{item.type}]";
                if (variables.dump.ContainsKey(type) == true)
                {
                    list = variables.dump[type];
                    list.Add(build_option(item.option, item.value));
                    variables.dump[type] = list;
                }
                else
                {
                    list = new List<string>();
                    list.Add(build_option(item.option, item.value));
                    variables.dump.Add(type, list);
                }
            }
            dump();
        }

        private string build_option(string option, object value)
        {
            return ($"{option}={value}");
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            variables.select_all.ShowDialog();
            for (int i = 0; i < checkboxes_modules.Items.Count; i++)
            {
                checkboxes_modules.SetItemChecked(i, true);
            }
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkboxes_modules.Items.Count; i++)
            {
                checkboxes_modules.SetItemChecked(i, false);
            }
        }

        private void bunifuCards2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
