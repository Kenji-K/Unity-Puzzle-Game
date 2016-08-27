using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Parse;

namespace Assets.Scripts.Util {
    internal class GlobalData {
        public const int TOTAL_GAME_MODES = 3;

        private static GlobalData instance;
        public static GlobalData Instance {
            get {
                if (instance == null) {
                    instance = new GlobalData();
                }
                return instance;
            }
        }

        public string GameMode { get; set; }
        public ParseObject GameStats { get; set; }
        public Dictionary<string, Color32> SolidColors { get; set; }
        public Dictionary<string, Color32> TransparentColors { get; set; }
        public Dictionary<string, Color32> HighlightColors { get; set; }

        public Dictionary<string, Leaderboard.DTOUserScore> FriendsInfo { get; set; }

        private List<Leaderboard.DTOUserScore> globalTopScores;
        public List<Leaderboard.DTOUserScore> GlobalTopScores {
            get { return globalTopScores; }
            set {
                globalTopScores = value;
                //Debug.Log("Global top scores set!");
                if (OnGlobalTopScoresSet != null) {
                    OnGlobalTopScoresSet();
                }
            }
        }

        public delegate void GlobalTopScoresSet();
        public event GlobalTopScoresSet OnGlobalTopScoresSet;

        private List<Leaderboard.DTOUserScore> friendsScores;
        public List<Leaderboard.DTOUserScore> FriendsScores {
            get { return friendsScores; }
            set { 
                friendsScores = value;
                //Debug.Log("Friends scores set!");
                if (OnFriendsScoresSet != null) {
                    OnFriendsScoresSet();
                }
            }
        }

        public delegate void FriendsScoresSet();
        public event FriendsScoresSet OnFriendsScoresSet;

        public string version = "1.0.0.0";
        public string playerName = "Anonymous";
        public string playerFacebookID = "";
        public Dictionary<string, long> highScores = new Dictionary<string,long>();

        public float masterVolume;
        public float musicVolume;
        public float effectsVolume;

        public int periodicModeSeed;
        public string rawSeed;

        public GlobalData() {
            highScores.Add("ClassicMode", 0);
            highScores.Add("GrowthMode", 0);
            //highScores.Add("MiniMode", 0);
            highScores.Add("PeriodicMode", 0);

            SolidColors = new Dictionary<string, Color32>();
            SolidColors.Add("ClassicMode", new Color32(14, 103, 164, 255)); //Blue
            SolidColors.Add("NormalMode", new Color32(14, 103, 164, 255)); //Blue, mismo modo que classic solo que añadiendo este para compatibilizar
            SolidColors.Add("GrowthMode", new Color32(234, 138, 26, 255)); //Orange
            SolidColors.Add("MiniMode", new Color32(108, 192, 1, 255)); //Green
            SolidColors.Add("PeriodicMode", new Color32(236, 0, 0, 255)); //Red
            SolidColors.Add("TutorialMode", new Color32(154, 1, 191, 255)); //Pink-ish Purple (?)

            HighlightColors = new Dictionary<string, Color32>();
            HighlightColors.Add("ClassicMode", new Color32(44, 133, 194, 255)); //Blue
            HighlightColors.Add("NormalMode", new Color32(44, 133, 194, 255)); //Blue, mismo modo que classic solo que añadiendo este para compatibilizar
            HighlightColors.Add("GrowthMode", new Color32(255, 168, 56, 255)); //Orange
            HighlightColors.Add("MiniMode", new Color32(138, 222, 31, 255)); //Green
            HighlightColors.Add("PeriodicMode", new Color32(255, 30, 30, 255)); //Red
            HighlightColors.Add("TutorialMode", new Color32(184, 31, 221, 255)); //Pink-ish Purple (?)

            TransparentColors = new Dictionary<string, Color32>();
            TransparentColors.Add("ClassicMode", new Color32(14, 103, 164, 0)); //Blue
            TransparentColors.Add("NormalMode", new Color32(14, 103, 164, 0)); //Blue, mismo modo que classic solo que añadiendo este para compatibilizar
            TransparentColors.Add("GrowthMode", new Color32(234, 138, 26, 0)); //Orange
            TransparentColors.Add("MiniMode", new Color32(108, 192, 1, 0)); //Green
            TransparentColors.Add("PeriodicMode", new Color32(236, 0, 0, 0)); //Red
            TransparentColors.Add("TutorialMode", new Color32(154, 1, 191, 255)); //Pink-ish Purple (?)
        }


    }
}
