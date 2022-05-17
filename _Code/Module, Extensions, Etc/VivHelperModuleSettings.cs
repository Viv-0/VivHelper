using Celeste.Mod;
using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Xna.Framework.Input;

namespace VivHelper {
    [SettingName("modoptions_VivHelperModule")]
    public class VivHelperModuleSettings : EverestModuleSettings {
        public bool ResetBadValues { get; set; } = false;
        public void CreateResetBadValuesEntry(TextMenu menu, bool inGame) {
            TextMenu.Item item;
            if (!inGame)
                menu.Add(item = new TextMenu.Button("modoptions_VivHelper_ResetValues".DialogCleanOrNull()).Pressed(delegate { ResetValues(); }));
        }

        [SettingName("modoptions_VivHelperModule_FollowerDist")]
        [SettingSubText("modoptions_VivHelperModule_FollowerDist_sub")]
        [SettingRange(1, 128)]
        public int FPDistance { get; set; } = 30;

        [SettingName("modoptions_VivHelperModule_FollowerDist2")]
        [SettingSubText("modoptions_VivHelperModule_FollowerDist2_sub")]
        [SettingRange(0, 30)]
        public int FFDistance { get; set; } = 5;

        public bool MakeClose { get; set; } = false;

        public bool ShowOneUseOnCustomRefills { get; set; } = false;

        public enum ColorRefillType {
            Normal = 0,
            Decreased = 1,
            Minimal = 2,
            None = 3

        }
        public ColorRefillType DecreaseParticles { get; set; } = ColorRefillType.Normal;

        [SettingName("modoptions_VivHelper_RCSL")]
        [SettingSubText("modoptions_VivHelper_RCSL_sub")]
        public bool RCSLines { get; set; } = false;
        [SettingName("modoptions_VivHelper_RCSBW")]
        [SettingSubText("modoptions_VivHelper_RCSBW_sub")]
        public bool ColorblindRCS { get; set; } = false;
        public bool DisableStaminaFlash { get; set; } = false;
        public bool SetFlashColorToHair { get; set; } = false;

        [DefaultButtonBinding(Buttons.Back, Keys.Tab)]
        public ButtonBinding DisplayPlaybacksInLookout { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.Z)]
        public ButtonBinding ReceiveLookoutHint { get; set; }

        private void ResetValues() {
            VivHelperModule.Settings.FPDistance = 30;
            VivHelperModule.Settings.FFDistance = 5;
            VivHelperModule.Settings.MakeClose = false;
            VivHelperModule.Settings.DecreaseParticles = ColorRefillType.Normal;
            VivHelperModule.Settings.DisableStaminaFlash = false;
            VivHelperModule.Settings.SetFlashColorToHair = false;
        }

    }
}
