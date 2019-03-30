using System;
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
            C_Ttttt.GetInstance.S_GetAssets(C_UIBase.Mono,delegate(Dictionary<string, int> assetsPathDic) {
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
    public class C_Crad : MonoBehaviour {
        public  RectTransform rectTransform;
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
        public void S_InitCard(RectTransform rectTransforms, int ii,float basiceSize) {
            C_UGUI.S_Get(rectTransforms, ii).o_Press = delegate (C_UGUI uGUI) {
                currentSelectCard = rectTransforms;
                fingerPressPosi = Input.mousePosition;
                pressPosi = currentSelectCard.position;
                if (selectList.Contains(cardDic[ii]) == false) {
                    if (selectList.Count >= C_Parameter.selectCardMaxNumber.Value) {
                        int index = 0;// selectList.Count - 1;
                        C_Crad crad0 = selectList[index];
                        crad0.S_CancelSelect();
                    }
                    selectList.Add(cardDic[ii]);
                    cardDic[ii].S_Select(basiceSize * C_Parameter.selectRadiusZoomRatio.Value);
                }
                C_UIBase.Mono.StartCoroutine(I_DragUpdate());
            };

            C_UGUI.S_Get(rectTransforms).o_Lift = delegate (C_UGUI uGUI) {
                currentSelectCard = null;
            };
            cardDic[ii] = this;
        }
        static RectTransform currentSelectCard;
        static Vector3 pressPosi, fingerPressPosi;
        static List<C_Crad> selectList = new List<C_Crad>();
        static Dictionary<int, C_Crad> cardDic = new Dictionary<int, C_Crad>();
        IEnumerator I_DragUpdate() {
            if (currentSelectCard == null) {
                yield break;
            }
            Vector3 offect = Input.mousePosition - fingerPressPosi;
            currentSelectCard.position = pressPosi + offect;
            yield return new WaitForSeconds(0);
            C_UIBase.Mono.StartCoroutine(I_DragUpdate());
        }


        public static void S_CloseData() {
            cardDic.Clear();
            currentSelectCard = null;
            for (int i = 0; i < selectList.Count; i++) {
                selectList[i].S_CancelSelect();
            }
            selectList.Clear();
        }


        public IEnumerator I_Load(string assetsPath,int assetsType, float basiceSize) {
            this.assetsPath = assetsPath;
            this.assetsType = assetsType;
            this.basiceSize = basiceSize;
            string assetsPathPic = assetsPath + ".pic";
            if (assetsType < 10) {
                if (File.Exists(assetsPathPic)) {
                    WWW www = new WWW("file:///" + assetsPathPic);
                    yield return www;
                    if (www.error != null) {
                        Debug.Log(("加载失败__" + assetsPathPic).S_SetColor("ff0000"));
                    } else {
                        rawImage.texture = www.texture;
                        sizeDelta = new Vector2(www.texture.width, www.texture.height) * basiceSize / C_Ttttt.thumbnailLenght;
                        rectTransform.sizeDelta = sizeDelta;
                        SetCapsuleCollider(1);
                    }
                } else {
                }
            } else {
                isPlay = false;
                Debug.LogFormat("视频___{0}", assetsPath);
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = "file:///" + assetsPath;
                videoPlayer.playOnAwake = false;
                videoPlayer.waitForFirstFrame = true;
                videoPlayer.sendFrameReadyEvents = true;
                videoPlayer.frameReady += S_VideoFrameEvent;
                videoPlayer.Play();
            }
            
        }
        float basiceSize;
        public string assetsPath;
        int assetsType;
        bool isPlay;
        VideoPlayer videoPlayer;
      //  RenderTexture renderTexture;
        void S_VideoFrameEvent(VideoPlayer source, long frameIdx) {
            rawImage.texture = source.texture;
            if (isPlay == false) {
                sizeDelta = new Vector2(source.texture.width, source.texture.height) * basiceSize / source.texture.width;
                rectTransform.sizeDelta = sizeDelta;
                SetCapsuleCollider(1);
                isPlay = true;
                if (C_Parameter.videoIsAllPlay.Value == 0) {
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
        public Action<string, int> d_SelectCardEvent;
        public void S_Select(float radius) {
            rawImage.enabled = false;
            GameObject.DestroyImmediate(rigidbody);
            SetCapsuleCollider(radius, true);
            selectCard = C_SelectCard.o_ObjectPool.S_GetObj(C_SelectCard.backOnlyAssetsPath);
            selectCard.S_Open(this, assetsPath, assetsType, radius);
            if (d_SelectCardEvent != null) {
                d_SelectCardEvent(assetsPath, assetsType);
            }
        }
        public void S_CancelSelect() {
            selectList.Remove(this);
            rawImage.enabled = true;
            selectCard.S_Close();
            SetCapsuleCollider(1);
            S_AddRigidbody();
            SetCapsuleCollider(1);
        }
        void SetCapsuleCollider(float zoomRatio,bool isRadius=false) {
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

        void FixedUpdate() {
          //  rigidbody.velocity = new Vector3(0, 0, 0);
        }
        //void OnTriggerEnter(Collider other) { //当进入触发器
        //    Debug.LogFormat("{0}__进入{1}", other.name, name);
        //}
        //void OnTriggerExit(Collider other) { //当退出触发器
        //}
        //void OnTriggerStay(Collider other) { // 当逗留触发器
        //}
    }
    public class C_SelectCard:MonoBehaviour {
        public static C_ObjectPool<C_SelectCard> o_ObjectPool = new C_ObjectPool<C_SelectCard>();
        public static C_Int4 backOnlyAssetsPath = new C_Int4(-12, 14, 1, 0);
        public static void S_InitObjectPool() {
            o_ObjectPool.d_ObjCreateEvent = delegate (C_Int4 onlyAssetsPath) {
                GameObject backBox = C_128_AssetsCore.GetInstance.S_GetModel(onlyAssetsPath, 10);
                C_SelectCard selectCard= backBox.AddComponent<C_SelectCard>() ;
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
        public RawImage videoImage;
        public RectTransform rectTransform, boxTransform, videoRectTransform;
        public VideoPlayer videoPlayer;
        Button closeButton;
        void S_Init() {
            rectTransform = GetComponent<RectTransform>();
            boxTransform = transform.Find("box").GetComponent<RectTransform>();
            videoImage = gameObject.transform.Find("video01").GetComponent<RawImage>();
            videoRectTransform = videoImage.GetComponent<RectTransform>();
            videoPlayer = videoImage.GetComponent<VideoPlayer>();
            closeButton = transform.Find("closeButton").GetComponent<Button>();
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
                    S_SetSize(videoRectTransform, source.texture, radius);
                }
            };
            closeButton.onClick.AddListener(delegate () {
                card.S_CancelSelect();
                //S_Close();
            });
        }
        float radius;
        C_Crad card;
        public void S_Open(C_Crad card,string assetsPath, int assetsType,float radius) {
            this.card = card;
            this.radius = radius;
            transform.SetParent(card.rectTransform);
            transform.localPosition = Vector3.zero;
            // transform.localScale = Vector3.one * radius / 200;
            rectTransform.sizeDelta = Vector2.one * radius * 2;
            boxTransform.sizeDelta= Vector2.one * (radius * 2f+60);
            videoImage.texture = null;
            if (assetsType < 10) {
                C_Ttttt.S_WWW(C_UIBase.Mono, "file:///" + assetsPath, delegate (WWW www) {
                    if (www == null) {
                        Debug.LogErrorFormat("读取图片错误__{0}__", assetsPath);
                        videoImage.texture = null;
                    } else {
                        Texture2D tt = www.texture;
                      //  tt.format = TextureFormat.ARGB32;
                        videoImage.texture = tt;
                        S_SetSize(videoRectTransform, www.texture, radius);
                    }
                });
            } else {
                videoPlayer.url = "file:///" + assetsPath;
                videoPlayer.Play();
            }
        }
        void S_SetSize(RectTransform rectTransform0, Texture texture, float radius) {
            Vector2 size;
            float thumbnailLenght =radius * 1.6f;
            if (texture.width >= texture.height) {
                size = new Vector2(thumbnailLenght, texture.height * thumbnailLenght / texture.width);
            } else {
                size = new Vector2(texture.width * thumbnailLenght / texture.height, thumbnailLenght);
            }
            //Vector2 size = new Vector2(texture.width, texture.height) * 512 / C_Ttttt.thumbnailLenght;
            rectTransform0.sizeDelta = size;
        }
        public void S_Close() {
            o_ObjectPool.S_SetToDeathObj(backOnlyAssetsPath,this);
        }
    }
}