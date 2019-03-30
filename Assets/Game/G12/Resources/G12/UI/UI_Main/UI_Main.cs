﻿using System;
using System.Collections;
using System.Collections.Generic;
using CC_Game;
using CC_Util;
using UI00;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Video;

namespace G12 {
    public class UI_Main : C_UIBase {
        public UI_Main(Transform father = null) : base(father) { }
        public override string S_GetPrefabPath() {
            return "Game/G12/Resources/G12/UI/UI_Main/UI_Main.prefab";
        }
        public override C_LanguageBase S_GetLanguageObj() {
            return null;
        }
        UI_Setup uiSetup {
            get {
                if(_uiSetup==null|| _uiSetup.rectTransform == null) {
                    _uiSetup = new UI_Setup();
                }
                return _uiSetup;
            }
        }
        UI_Setup _uiSetup;
        public C_Parameter parameter = new C_Parameter();
        Button setupButton;
        RectTransform selectBox, cardPrefab, boundary, load, showPathBox;
        Text progress, showPathText;
        protected override void S_Init() {
            base.S_Init();
            setupButton = GetControl<Button>("setupButton");
            selectBox = GetControl<RectTransform>("selectBox");
            cardPrefab = GetControl<RectTransform>("cardPrefab", selectBox);
            boundary = GetControl<RectTransform>("boundary");
            load = GetControl<RectTransform>("load");
            progress = GetControl<Text>("progress", load);
            showPathBox = GetControl<RectTransform>("showPathBox");
            showPathText = GetControl<Text>("text", showPathBox);
            rectTransform.gameObject.SetActive(true);
          
            SetBoundary(boundary);
            setupButton.onClick.AddListener(delegate () {
                uiSetup.S_Open(setupButton.transform);
            });
            C_SelectCard.S_InitObjectPool();
            S_dddd();
            C_Parameter.isShowAssetsPath.d_ChangleEvent += delegate (long dd) {
                if (C_Parameter.isShowAssetsPath.Value == 1) {
                    showPathBox.gameObject.SetActive(true);
                } else {
                    showPathBox.gameObject.SetActive(false);
                }
            };
            C_Parameter.isShowAssetsPath.Value += 0;
        }
        void SetBoundary(RectTransform boundary) {
            boundary.GetChild(0).position = new Vector3(Screen.width / 2, Screen.height+100, 0);
            boundary.GetChild(1).position = new Vector3(Screen.width+100, Screen.height/2, 0);
            boundary.GetChild(2).position = new Vector3(Screen.width / 2, -100, 0);
            boundary.GetChild(3).position = new Vector3(-100, Screen.height/2, 0);
        }
        public void S_AssetsLoadProgress(float progress0) {
            if (progress0 < 1) {
                if (load.gameObject.activeSelf == false) {
                    load.gameObject.SetActive(true);
                }
                progress.text =string.Format("场景正在加载中   {0}%", (progress0 * 100).ToString("00.0"));
            } else {
                if (load.gameObject.activeSelf == true) {
                    load.gameObject.SetActive(false);
                }
            }
        }
        public void S_dddd() {
            C_AssetsLoad.GetInstance.S_GetAssets(C_UIBase.Mono,delegate(Dictionary<string, int> assetsPathDic) {
                Debug.Log("资源数量____" + assetsPathDic.Count);
                C_Parameter.screenCardNumber = assetsPathDic.Count;
                S_ResetGame(assetsPathDic);
            });
           
        }
        float basiceSize;
        /// <summary>
        /// 重新开始生成卡片数据
        /// </summary>
        void S_ResetGame(Dictionary<string, int> assetsPathDic) {
            int screenMaxNumber = C_Parameter.screenCardNumber +(int)(C_Parameter.selectCardMaxNumber.Value * C_Parameter.selectRadiusZoomRatio.Value* C_Parameter.selectRadiusZoomRatio.Value*Mathf.PI);
            basiceSize = ((Screen.width * Screen.height) / screenMaxNumber )*0.55f;
            basiceSize = Mathf.Pow(basiceSize, 0.5f);
            cardPrefab.sizeDelta = new Vector2(1.2f, 0.75f) * basiceSize;
            if (cardFather != null) {
                GameObject.Destroy(cardFather.gameObject);
            }
            cardFather = new GameObject("cardFather02").transform;
            cardFather.SetParent(cardPrefab.parent);
            C_Crad.S_CloseData();
            C_UIBase.Mono.StartCoroutine(S_InitSelectBox(assetsPathDic, cardPrefab));
        }
        Transform cardFather;
        IEnumerator S_InitSelectBox(Dictionary<string, int> assetsPathDic, RectTransform cardPrefab) {
            cardPrefab.gameObject.SetActive(false);
            float interval = 300 / basiceSize;
            RectTransform father = (RectTransform)cardPrefab.parent;
            int numberX = (int)(father.rect.width/ (cardPrefab.rect.width+ interval));
            int i = 0;
            var dic= assetsPathDic.GetEnumerator();
            while (dic.MoveNext()) {
                RectTransform rect = RectTransform.Instantiate(cardPrefab, cardFather);
                rect.gameObject.SetActive(true);
                rect.localScale = Vector3.one;
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
   
                rect.GetComponent<RectTransform>().position =
                new Vector3((cardPrefab.sizeDelta.x + interval) * (0.5f+i % numberX),Screen.height -(cardPrefab.sizeDelta.y + interval) * (0.5f+i / numberX), 0);
                rect.sizeDelta = Vector2.one * basiceSize; ;
         
                C_Crad crad = rect.gameObject.AddComponent<C_Crad>();
                crad.Init(rect);
                crad.S_InitCard(rect, i, basiceSize);
                crad.d_SelectCardEvent = delegate (string path,int type) {
                    showPathText.text = path;
                };
                S_AssetsLoadProgress(i /( assetsPathDic.Count+0.01f));
                yield return C_UIBase.Mono.StartCoroutine(crad.I_Load(dic.Current.Key, dic.Current.Value, basiceSize));
                i++;
            }
            yield return new WaitForSeconds(0);
            S_AssetsLoadProgress(2);
        }
    }
    public class C_Parameter {
        /// <summary>
        /// 能选中的照片最大数量
        /// </summary>
        public static LongData selectCardMaxNumber = new LongData("selectCardMaxNumber", 4);
        /// <summary>
        /// 屏幕照片最大数量
        /// </summary>
        public static int screenCardNumber = 800;
        /// <summary>
        /// 选中照片的放大比例
        /// </summary>
        public static floatData selectRadiusZoomRatio = new floatData("selectRadiusZoomRatio", 2);
        /// <summary>
        /// 是否现在资源路径
        /// </summary>
        public static LongData isShowAssetsPath = new LongData("isShowAssetsPath", 0);
        /// <summary>
        /// 视频是否全部播放
        /// </summary>
        public static LongData videoIsAllPlay = new LongData("videoIsAllPlay",0);
    }


}