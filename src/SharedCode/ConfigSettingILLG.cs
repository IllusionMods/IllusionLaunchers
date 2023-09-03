using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace InitSetting
{
    // config.xml
    public class Preferences
    {
        public Title Title { get; set; }
        public Graphic Graphic { get; set; }
        public Camera Camera { get; set; }
        public Text Text { get; set; }
        public GameSystem GameSystem { get; set; }
        public HScene HScene { get; set; }
        public Sound Sound { get; set; }
        public Voice Voice { get; set; }
        public Debug Debug { get; set; }
    }

    public class Title
    {
        public bool EnabledChara { get; set; } = false;
        public bool IsUserChara { get; set; } = true;
        public string CharaFileName { get; set; } = "";
        public bool EnabledCCCustomBGM { get; set; } = false;
        public int CCBGMID { get; set; } = 2;
    }

    public class Camera
    {
        public bool InvertMoveX { get; set; } = false;
        public bool InvertMoveY { get; set; } = false;
        public bool Look { get; set; } = true;
        public double SensitivityX { get; set; } = 0.5;
        public double SensitivityY { get; set; } = 0.5;
    }

    public class Graphic
    {
        public string ScrSize { get; set; } = "1600 x 900 (16 : 9)";
        public int ScrWidth { get; set; } = 1600;
        public int ScrHeight { get; set; } = 900;
        public bool FullScreen { get; set; } = false;
        public int TargetDisplay { get; set; } = 0;
        public bool Bloom { get; set; } = true;
        public bool DepthOfField { get; set; } = true;
        public bool Vignette { get; set; } = true;
        public bool SSAO { get; set; } = true;
        public bool Fog { get; set; } = true;
        public int Quality { get; set; } = 0;
        public bool Map { get; set; } = true;
        public bool Shield { get; set; } = true;
        public bool AmbientLight { get; set; } = false;
        public string BackColor { get; set; } = "16,16,16,255";
    }

    public class Text
    {
        public int FontSpeed { get; set; } = 40;
        public double WindowAlpha { get; set; } = 0.8;
    }

    public class GameSystem
    {
        public bool ForegroundEyes { get; set; } = false;
        public bool ForegroundEyebrow { get; set; } = false;
        public bool HohoAka { get; set; } = true;
    }

    public class HScene
    {
        public bool Visible { get; set; } = true;
        public bool Son { get; set; } = true;
        public bool Cloth { get; set; } = true;
        public bool Accessory { get; set; } = true;
        public bool Shoes { get; set; } = true;
        public bool SecondVisible { get; set; } = true;
        public bool SecondSon { get; set; } = true;
        public bool SecondCloth { get; set; } = true;
        public bool SecondAccessory { get; set; } = true;
        public bool SecondShoes { get; set; } = true;
        public int SiruDraw { get; set; } = 0;
        public int UrineDraw { get; set; } = 0;
        public bool SweatDraw { get; set; } = true;
        public bool Urine { get; set; } = false;
        public int SioDraw { get; set; } = 0;
        public bool Sio { get; set; } = false;
        public bool FeelingGauge { get; set; } = true;
        public bool ActionGuide { get; set; } = true;
        public bool InitCamera { get; set; } = true;
        public bool EyeDir0 { get; set; } = false;
        public bool NeckDir0 { get; set; } = false;
        public bool EyeDir1 { get; set; } = false;
        public bool NeckDir1 { get; set; } = false;
        public bool HomeCallConciergeEventSkip { get; set; } = false;
        public bool SimpleBody { get; set; } = false;
        public string SilhouetteColor { get; set; } = "0,0,1,0,5";
        public bool WeakStop { get; set; } = false;
        public bool EscapeStop { get; set; } = false;
        public string MobColor_M { get; set; } = "0,0,1,1";
        public string MobColor_F { get; set; } = "0,0,1,1";
    }

    public class Sound
    {
        public string Master { get; set; } = "Volume[100] : Switch[True]";
        public string BGM { get; set; } = "Volume[40] : Switch[True]";
        public string ENV { get; set; } = "Volume[80] : Switch[True]";
        public string SystemSE { get; set; } = "Volume[50] : Switch[True]";
        public string GameSE { get; set; } = "Volume[70] : Switch[True]";
    }

    public class Voice
    {
        public string Master { get; set; } = "Volume[100] : Switch[True]";
        public string c00 { get; set; } = "Volume[100] : Switch[True]";
        public string c01 { get; set; } = "Volume[100] : Switch[True]";
        public string c02 { get; set; } = "Volume[100] : Switch[True]";
        public string c03 { get; set; } = "Volume[100] : Switch[True]";
        public string c04 { get; set; } = "Volume[100] : Switch[True]";
        public string c05 { get; set; } = "Volume[100] : Switch[True]";
        //public string c-1 { get; set; } = "Volume[100] : Switch[True]";
    }

    public class Debug
    {
        public bool FPS { get; set; } = false;
    }

    // setup.xml
    public class Setting
    {
        public int Language { get; set; }
    }
}
