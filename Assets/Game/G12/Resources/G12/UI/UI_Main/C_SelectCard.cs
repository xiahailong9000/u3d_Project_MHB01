using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC_Game;
using CC_Util;
using UI00;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

namespace G12 {
    /// <summary>
    /// 第二层卡片
    /// </summary>
    public class C_SelectCard : MonoBehaviour {

        public static C_ObjectPool<C_SelectCard> o_ObjectPool = new C_ObjectPool<C_SelectCard>();
        public static C_Int4 backOnlyAssetsPath = new C_Int4(-12, 14, 1, 0);
        public static void S_InitObjectPool() {
            o_ObjectPool.d_ObjCreateEvent = delegate (C_Int4 onlyAssetsPath) {
                GameObject backBox = C_128_AssetsCore.GetInstance.S_GetModel(onlyAssetsPath, 10);
                C_SelectCard selectCard = backBox.AddComponent<C_SelectCard>();
                selectCard.S_Init();
                return selectCard;
            };
            o_ObjectPool.d_ObjRebornEvent = delegate (C_SelectCard nn) {
                nn.gameObject.SetActive(true);
            };
            o_ObjectPool.d_ObjDeathEvent = delegate (C_SelectCard nn) {
                nn.transform.SetParent(null);
                nn.gameObject.SetActive(false);
            };
        }



        public RectTransform rectRoot, boxTransform;
        public C_Card0[] card0s = new C_Card0[15];
        Button closeButton;
        void S_Init() {
            rectRoot = GetComponent<RectTransform>();
            boxTransform = transform.Find("box").GetComponent<RectTransform>();
            RectTransform rectTransform =gameObject.transform.Find("video01").GetComponent<RectTransform>();
            rectTransform.gameObject.SetActive(false);
            for (int i=0;i< card0s.Length; i++) {
                card0s[i] = new C_Card0(RectTransform.Instantiate(rectTransform));
                card0s[i].rectTransform.SetParent(rectTransform.parent);
                card0s[i].rectTransform.name = "dd_" + i;
            }
            closeButton = transform.Find("closeButton").GetComponent<Button>();

            closeButton.onClick.AddListener(delegate () {
                father.S_CancelSelect();
                //S_Close();
            });
        }
        Dictionary<string, int> assetsDic;
        C_Crad father;
        C_Card0[] showCards;
        public void S_Open(C_Crad father, Dictionary<string, int> assetsDic, float radius) {
            this.father = father;
            this.assetsDic = assetsDic;
            rectRoot.SetParent(father.rectTransform);
            rectRoot.localPosition = Vector3.zero;
            rectRoot.sizeDelta = Vector2.one * radius * 2;
            boxTransform.sizeDelta = Vector2.one * (radius * 2f + 60);

            showCards = new C_Card0[assetsDic.Count];
            float pow = Mathf.Pow(assetsDic.Count, 0.5f);
            int row = (int)pow;
            int column = (int)((assetsDic.Count + 0.5f * row) / row);
            Debug.LogFormat("横竖计算___{0}___{1}____{2}____{3}", assetsDic.Count, pow, row, column);
            float basicsWidth = (radius * 2)/row;
            float basicsHeight = (radius * 2)/column;
            var dic = assetsDic.GetEnumerator();
            for (int i=0;i< card0s.Length; i++) {
                bool bb= dic.MoveNext();
                C_Card0 card= card0s[i];
                if (bb) {
                    float posiX = (-row/2f+0.5f + (i % row)) * basicsWidth;
                    float posiY = (-column/2f+0.5f + (i / row)) * basicsHeight;
                    Vector3 posi = new Vector3(posiX, posiY,0);
            

                    Vector3 direction = posi.normalized;
                   // Debug.LogFormat("___________{0}_____{1}___{2}___{3}___{4}", i, row / 2f + 0.5f, (i % row), posi, direction);
                    float ratio = Mathf.Clamp(radius / (posi.magnitude*1.5f), 0.1f,1);
                    float ratio2 = Mathf.Clamp(radius / (posi.magnitude * 1.2f), 0.1f, 1);
                    // ratio = 1;
                    card.rectTransform.gameObject.SetActive(true);
                    card.rectTransform.sizeDelta = new Vector2(basicsWidth, basicsHeight)* ratio*0.8f;


                    card.S_Open(this, dic.Current.Key, dic.Current.Value,direction, basicsWidth, basicsHeight, radius);
                   
                    card.rectTransform.position = posi* ratio2+ rectRoot.position;
                    showCards[i] = card;

                } else {
                    card.rectTransform.gameObject.SetActive(false);
                }
            }
            father.d_PressEvent = S_GroupPressEvent;
            father.d_LiftEvent = S_GroupLiftEvent;
            rectRoot.localScale = Vector3.one*0.2f;
            rectRoot.DOScale(Vector3.one, 0.3f);
        }
        void S_SetSize(RectTransform rectTransform0, Texture texture, float radius) {
            Vector2 size;
            float thumbnailLenght = radius * 1.6f;
            if (texture.width >= texture.height) {
                size = new Vector2(thumbnailLenght, texture.height * thumbnailLenght / texture.width);
            } else {
                size = new Vector2(texture.width * thumbnailLenght / texture.height, thumbnailLenght);
            }
            //Vector2 size = new Vector2(texture.width, texture.height) * 512 / C_Ttttt.thumbnailLenght;
            rectTransform0.sizeDelta = size;
        }
        public void S_Close() {
            for (int i = 0; i < showCards.Length; i++) {
                showCards[i].S_CancelSelect();
            }
            rectRoot.localScale = Vector3.one;
            rectRoot.DOScale(Vector3.one * 0.2f, 0.3f).OnComplete(delegate () {
                o_ObjectPool.S_SetToDeathObj(backOnlyAssetsPath, this);
            });
        }
        public void S_PressEvent() {
            father.S_PressEvent();
        }
        public void S_LiftEvent() {
            father.S_LiftEvent();
        }
        public void S_GroupPressEvent() {
            for (int i = 0; i < showCards.Length; i++) {
                showCards[i].S_PressEvent();
            }
        }
        public void S_GroupLiftEvent() {
            for (int i = 0; i < showCards.Length; i++) {
                showCards[i].S_LiftEvent();
            }
        }
        [Serializable]
        public class C_Card0 {
            public RectTransform rectTransform;
            public RawImage videoImage;
            public VideoPlayer videoPlayer;
            public C_Card0(RectTransform rectTransform) {
                this.rectTransform = rectTransform;
                videoImage = rectTransform.GetComponent<RawImage>();
                videoPlayer = videoImage.GetComponent<VideoPlayer>();

                videoPlayer.playOnAwake = false;
                videoPlayer.source = VideoSource.Url;
                //  videoPlayer.url = "file:///" + assetsPath;
                videoPlayer.waitForFirstFrame = true;
                videoPlayer.sendFrameReadyEvents = true;
                bool isStartframe = true;
                videoPlayer.frameReady += delegate (VideoPlayer source, long frameIdx) {
                    videoImage.texture = source.texture;
                    if (isStartframe) {
                        isStartframe = false;
                        S_SetSize( source.texture, radius);
                    }
                };
                C_UGUI.S_Get(rectTransform).d_Press = delegate (C_UGUI uGUI) {
                    //Debug.Log("按下_____1");
                    fingerPressPosi = Input.mousePosition;
                    father.S_PressEvent();
                };
                C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                    //Debug.Log("抬起_____1");
                    father.S_LiftEvent();
                    if(Vector3.Distance(fingerPressPosi, Input.mousePosition) < 6) {
                        S_SelectCard();
                    }
                };
            }
            static Vector3 fingerPressPosi;
            void S_SetSize(Texture texture, float radius) {
                Vector2 size;
                float thumbnailLenght = radius * 1.6f;
                thumbnailLenght = basicsWidth+ basicsHeight;
                if (texture.width >= texture.height) {
                    size = new Vector2(thumbnailLenght, texture.height * thumbnailLenght / texture.width);
                } else {
                    size = new Vector2(texture.width * thumbnailLenght / texture.height, thumbnailLenght);
                }
                //return;
                //Vector2 size = new Vector2(texture.width, texture.height) * 512 / C_Ttttt.thumbnailLenght;
                rectTransform.sizeDelta = size/3;
            }
            C_SelectCard father;
            float radius;
            string assetsPath;
            int assetsType;
            Vector3 direction;
            float basicsWidth;
            float basicsHeight;
            public void S_Open(C_SelectCard father, string assetsPath, int assetsType, Vector3 direction, float basicsWidth, float basicsHeight, float radius) {
                this.father = father;
                this.radius = radius;
                this.direction = direction;
                this.assetsPath = assetsPath;
                this.assetsType = assetsType;
                this.basicsWidth = basicsWidth;
                this.basicsHeight = basicsHeight;
                //Vector3 vv = father.rectRoot.position;
                //rectTransform.position = new Vector3(vv.x, vv.y, -radius * 3);
                radius = 70;
                videoImage.texture = null;
                if (assetsType < 10) {
                    C_AssetsLoad.S_WWW(C_UIBase.Mono, "file:///" + assetsPath+".pic", delegate (WWW www) {
                        if (www == null) {
                            Debug.LogErrorFormat("读取图片错误__{0}__", assetsPath);
                            videoImage.texture = null;
                        } else {
                            Texture2D tt = www.texture;
                            //  tt.format = TextureFormat.ARGB32;
                            videoImage.texture = tt;
                            S_SetSize(www.texture, radius);
                        }
                    });
                } else {
                    videoPlayer.url = "file:///" + assetsPath;
                    videoPlayer.Play();
                }
            }
            public C_PreviewCard previewCard;
            void S_SelectCard() {
                //Debug.Log("fffffffffffffffffffffff");
                if (previewCard == null) {
                    previewCard = C_PreviewCard.o_ObjectPool.S_GetObj(C_PreviewCard.previewOnlyAssetsPath);
                } else {
                    Debug.LogFormat("已经被选中____不能重复选中");
                }
                previewCard.S_Open(this, assetsPath, assetsType, direction, radius);
                G12Manage.GetInstance.uiMain.S_ShowAssetsPath(assetsPath, assetsType);
            }
            public void S_CancelSelect() {
                if (previewCard != null) {
                    previewCard.S_Close();
                    previewCard = null;
                }
            }
            public void S_PressEvent() {
                if (previewCard != null) {
                    previewCard.S_PressEvent();
                } else {
                   // Debug.LogFormat("没有被选中____不能打开");
                }
            }
            public void S_LiftEvent() {
                if (previewCard != null) {
                    previewCard.S_LiftEvent();
                } else {
                  //  Debug.LogFormat("没有被选中____不能打开");
                }
            }
        }
    }
}
