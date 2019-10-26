using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CC_Util;
using UI00;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace G12 {
    /// <summary>
    /// 第一层卡片
    /// </summary>
    public class C_Card : MonoBehaviour {
        public RectTransform rectTransform;
        public RawImage rawImage;
        CapsuleCollider capsuleCollider;
        new Rigidbody rigidbody;
        Vector2 sizeDelta;
        public void Init(RectTransform rectTransform) {
            this.rectTransform = rectTransform;
            rawImage = rectTransform.GetComponent<RawImage>();
            rigidbody = rectTransform.GetComponent<Rigidbody>();
            capsuleCollider = rectTransform.GetComponent<CapsuleCollider>();
            sizeDelta = Vector2.one * 20;
            SetCapsuleCollider(1);
        }
        int index;
        public void S_InitCard(RectTransform rectTransform, int ii) {
            index = ii;
            C_UGUI.S_Get(rectTransform, ii).d_Press = delegate (C_UGUI uGUI) {
                S_PressEvent(uGUI.o_PointerEventData);
            };
            C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                S_LiftEvent(uGUI.o_PointerEventData);
            };
            C_UGUI.S_Get(rectTransform).d_DragEvent = delegate (C_UGUI uGUI) {
                S_DragEvent(uGUI.o_PointerEventData);
            };
            cardDic[ii] = this;
        }


        //RectTransform currentSelectCard;
        Vector3 pressPosi, fingerPressPosi;
        static List<C_Card> selectList = new List<C_Card>();
        static Dictionary<int, C_Card> cardDic = new Dictionary<int, C_Card>();
        public Action<PointerEventData> d_PressEvent, d_LiftEvent, d_DragEvent;
        public void S_PressEvent(PointerEventData pointerEventData) {
            transform.SetSiblingIndex(10000);
            Debug.Log("C_Card按下_____" + assetsPathPic);
            //currentSelectCard = rectTransform;
            fingerPressPosi = pointerEventData.position.S_ToVector3();
            pressPosi = rectTransform.position;
            if (d_PressEvent != null) {
                d_PressEvent(pointerEventData);
            }
            //C_UIBase.Mono.StartCoroutine(I_DragUpdate());
        }
        public void S_LiftEvent(PointerEventData pointerEventData) {
            Debug.Log("C_Card抬起_____" + assetsPathPic);
            //currentSelectCard = null;
            if (d_LiftEvent != null) {
                d_LiftEvent(pointerEventData);
            }
            if (Vector3.Distance(pointerEventData.position.S_ToVector3(), fingerPressPosi) < 6) {
                S_SelectCard(index, C_Parameter.basiceCardSize);
            }
        }
        public void S_DragEvent(PointerEventData pointerEventData) {
            Vector3 offect = pointerEventData.position.S_ToVector3() - fingerPressPosi;
            rectTransform.position = pressPosi + offect;
        }
        static void S_SelectCard(int ii, float basiceSize) {
            if (selectList.Contains(cardDic[ii]) == false) {
                if (selectList.Count >= UI_Main.C_Parameter.selectCardMaxNumber.Value) {
                    int index = 0;// selectList.Count - 1;
                    C_Card crad0 = selectList[index];
                    crad0.S_CancelSelect();
                }
                selectList.Add(cardDic[ii]);
                cardDic[ii].S_Select(basiceSize * (float)UI_Main.C_Parameter.selectRadiusZoomRatio.Value);
            }
        }


        public static void S_CloseData() {
            cardDic.Clear();
            //currentSelectCard = null;
            for (int i = 0; i < selectList.Count; i++) {
                selectList[i].S_CancelSelect();
            }
            selectList.Clear();
        }

        string assetsPathPic;
        public IEnumerator I_Load(Dictionary<string, int> assetsDic, float basiceSize) {
            this.assetsDic = assetsDic;
            this.basiceSize = basiceSize;
            var assets=  assetsDic.ElementAt(0);
            assetsPathPic = assets.Key + ".pic";
            if (assets.Value < 10) {
                if (File.Exists(assetsPathPic)) {
                    WWW www = new WWW("file:///" + assetsPathPic);
                    yield return www;
                    if (www.error != null) {
                        Debug.LogError(("加载失败__" + assetsPathPic).S_SetColor("ff0000"));
                    } else {
                        rawImage.texture = www.texture;
                        sizeDelta = new Vector2(www.texture.width, www.texture.height) * basiceSize / C_AssetsLoad.thumbnailLenght;
                        rectTransform.sizeDelta = sizeDelta;
                        SetCapsuleCollider(1);
                    }
                } else {
                }
            } else {
                isPlay = false;
                Debug.LogFormat("视频___{0}", assets.Key);
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = "file:///" + assets.Key;
                videoPlayer.playOnAwake = false;
                videoPlayer.waitForFirstFrame = true;
                videoPlayer.sendFrameReadyEvents = true;
                videoPlayer.frameReady += S_VideoFrameEvent;
                videoPlayer.Play();
            }

        }
        float basiceSize;

        public Dictionary<string, int> assetsDic;
        bool isPlay;
        VideoPlayer videoPlayer;
        void S_VideoFrameEvent(VideoPlayer source, long frameIdx) {
            rawImage.texture = source.texture;
            if (isPlay == false) {
                sizeDelta = new Vector2(source.texture.width, source.texture.height) * basiceSize / source.texture.width;
                rectTransform.sizeDelta = sizeDelta;
                SetCapsuleCollider(1);
                isPlay = true;
                if (UI_Main.C_Parameter.videoIsAllPlay.Value == 0) {
                    //Texture2D tt = source.texture as Texture2D;
                    //rawImage.texture = C_Ttttt.ScaleTexture(tt, source.texture.width / 5, source.texture.height / 5);

                    Texture2D videoFrameTexture = new Texture2D(2, 2);
                    RenderTexture renderTexture = source.texture as RenderTexture;
                    if (videoFrameTexture.width != renderTexture.width || videoFrameTexture.height != renderTexture.height) {
                        videoFrameTexture.Resize(renderTexture.width, renderTexture.height);
                    }
                    RenderTexture.active = renderTexture;
                    videoFrameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    videoFrameTexture.Apply();
                    RenderTexture.active = null;
                    videoPlayer.frameReady -= S_VideoFrameEvent;
                    videoPlayer.sendFrameReadyEvents = false;
                    rawImage.texture = videoFrameTexture;
                    // C_Ttttt.S_Thumbnail(assetsPath, videoFrameTexture);
                    videoPlayer.Stop();
                }
            }
        }
        public void S_AddRigidbody() {
            rigidbody = rectTransform.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                rigidbody = rectTransform.gameObject.AddComponent<Rigidbody>();
            }
            rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
            rigidbody.freezeRotation = true;
            rigidbody.useGravity = false;
        }
        C_SelectCard selectCard;
        public void S_Select(float radius) {
            transform.SetSiblingIndex(10000);
            rawImage.enabled = false;
            GameObject.DestroyImmediate(rigidbody);
            SetCapsuleCollider(radius, true);
            selectCard = C_SelectCard.o_ObjectPool.S_GetObj(C_SelectCard.backOnlyAssetsPath);
            selectCard.S_Open(this, assetsDic, radius);
        }
        public void S_CancelSelect() {
            selectList.Remove(this);
            rawImage.enabled = true;
            selectCard.S_Close();
            SetCapsuleCollider(1);
            S_AddRigidbody();
            SetCapsuleCollider(1);
        }
        void SetCapsuleCollider(float zoomRatio, bool isRadius = false) {
            if (isRadius) {
                zoomRatio += 0.3f;
                capsuleCollider.radius = zoomRatio;
                capsuleCollider.height = zoomRatio;
            } else {
                zoomRatio += 0.3f;
                if (sizeDelta.x > sizeDelta.y) {
                    capsuleCollider.direction = 0;
                    capsuleCollider.radius = sizeDelta.y * zoomRatio / 2;
                    capsuleCollider.height = sizeDelta.x * zoomRatio;
                } else {
                    capsuleCollider.direction = 1;
                    capsuleCollider.radius = sizeDelta.x * zoomRatio / 2;
                    capsuleCollider.height = sizeDelta.y * zoomRatio;
                }
            }

        }

        public class C_Parameter {
            public static float basiceCardSize;
        }
    }
}
