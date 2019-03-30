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
        C_Card0[] card0s = new C_Card0[15];
        Button closeButton;
        void S_Init() {
            rectRoot = GetComponent<RectTransform>();
            boxTransform = transform.Find("box").GetComponent<RectTransform>();
            RectTransform rectTransform =gameObject.transform.Find("video01").GetComponent<RectTransform>();
            rectTransform.gameObject.SetActive(false);
            for (int i=0;i< card0s.Length; i++) {
                card0s[i] = new C_Card0(RectTransform.Instantiate(rectTransform));
                card0s[i].rectTransform.SetParent(rectTransform.parent);
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
            var dic = assetsDic.GetEnumerator();
            for (int i=0;i< card0s.Length; i++) {
                bool bb= dic.MoveNext();
                C_Card0 card= card0s[i];
                if (bb) {
                    card.rectTransform.gameObject.SetActive(true);
                    card.S_Open(this, dic.Current.Key, dic.Current.Value, 55);
                    showCards[i] = card;

                } else {
                    card.rectTransform.gameObject.SetActive(false);
                }
            }
            father.d_PosiOffectEvent = delegate (Vector3 offect) {
                for(int i=0;i< showCards.Length; i++) {
                    showCards[i].S_Offect(offect);
                }
            };
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
            o_ObjectPool.S_SetToDeathObj(backOnlyAssetsPath, this);
        }
        public void S_PressEvent() {
            father.S_PressEvent(father.rectTransform);
        }
        public void S_DragEvent(Vector3 offect) {
            father.S_DragEvent(offect);
        }
        public void S_LiftEvent() {
            father.S_LiftEvent();
        }
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
                        S_SetSize(rectTransform, source.texture, radius);
                    }
                };
                C_UGUI.S_Get(rectTransform).d_Press = delegate (C_UGUI uGUI) {
                    Debug.Log("按下_____");
                    fingerPressPosi = Input.mousePosition;
                    father.S_PressEvent();
                };
                C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                    father.S_DragEvent(uGUI.o_PointerEventData.delta);
                };
                C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                    Debug.Log("抬起_____");
                    father.S_LiftEvent();
                    if(Vector3.Distance(fingerPressPosi, Input.mousePosition) < 6) {
                        S_SelectCard();
                    }
                };
            }
            static Vector3 fingerPressPosi;
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
            C_SelectCard father;
            float radius;
            string assetsPath;
            int assetsType;
            public void S_Open(C_SelectCard father, string assetsPath, int assetsType, float radius) {
                this.father = father;
                this.radius = radius;
                this.assetsPath = assetsPath;
                this.assetsType = assetsType;
                Vector3 vv = father.rectRoot.position;
                rectTransform.position =new Vector3(vv.x,vv.y,-radius*3) ;
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
                            S_SetSize(rectTransform, www.texture, radius);
                        }
                    });
                } else {
                    videoPlayer.url = "file:///" + assetsPath;
                    videoPlayer.Play();
                }
            }
            C_PreviewCard previewCard;
            void S_SelectCard() {
               // if (previewCard == null) {
                    previewCard = C_PreviewCard.o_ObjectPool.S_GetObj(C_PreviewCard.previewOnlyAssetsPath);
                    previewCard.S_Open(this, assetsPath, assetsType, 80);
              //  } else {

              //  }
            }
            public void S_Offect(Vector3 offect) {
                previewCard.rectTransform.position += offect;
            }
        }
    }
}
