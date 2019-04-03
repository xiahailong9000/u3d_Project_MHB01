using System;
using System.Collections;
using System.Collections.Generic;
using CC_Game;
using CC_Util;
using UI00;
using UI02;
using UnityEngine;
using UnityEngine.UI;
namespace G12 {
    public class UI_Setup : C_UIBase {
        public UI_Setup(Transform father = null) : base(father) { }
        public override string S_GetPrefabPath() {
            return "Game/G12/Resources/G12/UI/UI_Setup/UI_Setup.prefab";
        }
        public override C_LanguageBase S_GetLanguageObj() {
            return null;
        }
        Button clearButton, AgreeButton;
        Slider screenCardNumberSlider, selectCardMaxNumberSlider, selectRadiusZoomRatioSlider;
        Text screenCardNumberSliderExplain, selectCardMaxNumberSliderExplain, selectRadiusZoomRatioSliderExplain;
        C_Toggle isShowAssetsPathToggle, videoIsAllPlayToggle;
        protected override void S_Init() {
            base.S_Init();
           
            screenCardNumberSlider = GetControl<Slider>("view/screenCardNumberSlider");
            selectCardMaxNumberSlider = GetControl<Slider>("view/selectCardMaxNumberSlider");
            selectRadiusZoomRatioSlider = GetControl<Slider>("view/selectRadiusZoomRatioSlider");
            clearButton = GetControl<Button>("view/clearButton");
            AgreeButton = GetControl<Button>("view/AgreeButton");
            screenCardNumberSliderExplain = GetControl<Text>("explain", screenCardNumberSlider);
            selectCardMaxNumberSliderExplain = GetControl<Text>("explain", selectCardMaxNumberSlider);
            selectRadiusZoomRatioSliderExplain = GetControl<Text>("explain", selectRadiusZoomRatioSlider);
            isShowAssetsPathToggle = GetControl<C_Toggle>("view/isShowAssetsPathToggle");
            videoIsAllPlayToggle = GetControl<C_Toggle>("view/videoIsAllPlayToggle");
            screenCardNumberSlider.onValueChanged.AddListener(delegate (float ff) {
                int value = ((int)ff);
                screenCardNumberSliderExplain.text = "屏幕照片数量 " + value;
              //  C_Parameter.screenCardNumber = value;
            });
            selectCardMaxNumberSlider.onValueChanged.AddListener(delegate (float ff) {
                int value = ((int)ff);
                selectCardMaxNumberSliderExplain.text = "能选中的照片最大数量  " + value;
                UI_Main.C_Parameter.selectCardMaxNumber.Value = value;
            });
            selectRadiusZoomRatioSlider.onValueChanged.AddListener(delegate (float ff) {
                int value = ((int)ff);
                selectRadiusZoomRatioSliderExplain.text = "选中泡泡半径放大的倍数  " + value;
                UI_Main.C_Parameter.selectRadiusZoomRatio.Value = value;
            });
            clearButton.onClick.AddListener(delegate () {
                S_Close();
            });
            AgreeButton.onClick.AddListener(delegate () {
                S_Close();
                G12Manage.GetInstance.uiMain.S_StartRun();
            });
            isShowAssetsPathToggle.d_SelectEvent += delegate (C_Toggle nn) {
                UI_Main.C_Parameter.isShowAssetsPath.Value = nn.isSelected ? 1 : 0;
            };
            videoIsAllPlayToggle.d_SelectEvent += delegate (C_Toggle nn) {
                UI_Main.C_Parameter.videoIsAllPlay.Value = nn.isSelected ? 1 : 0;
                UI_SystemHints.GetInstance.o_HintsView.S_ShowMessage2(videoIsAllPlayToggle.transform, "切换视频播放模式", "需要重新加载场景", delegate (bool isConfirm) {
                    if (isConfirm) {
                        S_Close();
                        G12Manage.GetInstance.uiMain.S_StartRun();
                    }
                });
            };

        }
        public override void S_Open(Transform parentButton, bool isUIAnimction = false) {
            base.S_Open(parentButton, true);
            S_RefreshShow();
        }
        void S_RefreshShow() {
           // screenCardNumberSlider.value= C_Parameter.screenCardNumber;
            selectCardMaxNumberSlider.value = UI_Main.C_Parameter.selectCardMaxNumber.Value;
            selectRadiusZoomRatioSlider.value = UI_Main.C_Parameter.selectRadiusZoomRatio.Value;
            isShowAssetsPathToggle.isSelected = UI_Main.C_Parameter.isShowAssetsPath.Value == 1 ? true : false;
            videoIsAllPlayToggle.isSelected = UI_Main.C_Parameter.videoIsAllPlay.Value == 1 ? true : false;
        }
    }
}