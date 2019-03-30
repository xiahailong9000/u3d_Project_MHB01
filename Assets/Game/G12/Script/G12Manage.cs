using System.Collections;
using System.Collections.Generic;
using CC_Util;
using UI02;
using UnityEngine;
using UnityEngine.UI;

namespace G12 {
    public class G12Manage : MonoBehaviour {
        public RenderTexture[] renderTexture;
        public UI_Main uiMain;
        public static G12Manage GetInstance;
        private void Awake() {
            GetInstance = this;
        }
        void Start() {
            Game.G12.CC_Resources.ResourcesPath.S_SetPathToDic();
            UI_SystemHints.prefabPath = "Game/G12/Resources/G12/UI/UI_SystemHints/UI_SystemHints.prefab";
            uiMain = new UI_Main();
        }
    }
}