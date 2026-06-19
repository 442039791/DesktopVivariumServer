//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.Rendering;

//namespace SoftKitty.PCW.Demo
//{
//    public class CameraControl : MonoBehaviour
//    {
//        public static float ApertureAdd = 0F;
//        public static float FovAdd = 0F;
//        public static bool Active = true;
//        public LayerMask GroundLayer;
//        public Transform RotY;
//        public Transform RotX;
//        public Transform OffsetY;
//        public Camera Cam;
//        public Transform FollwingTarget;
//        private float AngleY;
//        private float AngleX =30F;
//        private float Scroll = 60F;
//        private float Clipping = 6F;
//        private bool Clipped = false;
//        private RaycastHit GroundHit;
//        private bool MouseButtonDown =false;
//        public Vector3 StartPos;
//        public float MoveSpeed;
//        private float ScrollWheelSpeed;
//        void Start()
//        {
//            transform.localPosition = StartPos;
////#if UNITY_EDITOR || MY_CUSTOM_MACRO
//            ScrollWheelSpeed = 0.1f;
//            AngleY = 0F;
//            AngleX = 0f;
//            //#else
//            //            ScrollWheelSpeed =2f;
//            //#endif
//            //Debug.Log(GraphicsSettings.transparencySortMode);
//        }

//        void Update()
//        {
//            //Active = !BlockGenerator.instance.isTeleporting();
//            if (!BuildControl.BuildMode /*&& !EventSystem.current.IsPointerOverGameObject()*/)
//            {
//                int i = 0;
//                if (CustomInput.GetKey(KeyCode.J))
//                    i += 1;
//                if (CustomInput.GetKey(KeyCode.K))
//                    i -= 1;
//                //Debug.Log(Scroll);
//#if BUILDMODE
//                float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // 获取鼠标滚轮输入
//                Scroll = Mathf.Clamp(Scroll - scrollInput * ScrollWheelSpeed * 50f, 1f, 100f);
//#else
//                Scroll = Mathf.Clamp(Scroll - i * ScrollWheelSpeed , 1f, 100f);
//#endif

//                if (CustomInput.GetKey(KeyCode.I))
//                {
//                    transform.localPosition = StartPos;
//                    AngleY = 0F;
//                    AngleX = 0f;
//                    Scroll = 60f;
//                }
//            }
                
//            FovAdd = Mathf.Lerp(FovAdd,0F,Time.deltaTime*1F);
//            ApertureAdd = Mathf.Lerp(ApertureAdd, 0F, Time.deltaTime * 1F);
//            Cam.fieldOfView = 60F + FovAdd;
            

//            //if (!Active) return;
//            if (Input.GetMouseButtonDown(1)) MouseButtonDown = true;
//            if (Input.GetMouseButtonUp(1)) MouseButtonDown = false;
//#if BUILDMODE
//            if (MouseButtonDown)
//            {
//                Cursor.lockState =  CursorLockMode.Locked;
//                Cursor.visible = false;
//                AngleY += Input.GetAxis("Mouse X")*Time.deltaTime*100F;
//                AngleX = Mathf.Clamp(AngleX- Input.GetAxis("Mouse Y") * Time.deltaTime * 50F, -30F,80F);
//                AngleY = AngleY % 360F;
//                if (AngleY > 180F) AngleY -= 360F;
//                if (AngleY < -180F) AngleY += 360F;
//            }
//            else
//            {
//                Cursor.lockState = CursorLockMode.None;
//                Cursor.visible = true;
//            }
//#endif
//        }
//        void LateUpdate()
//        {
//            // 更新摄像头的缩放位置
//            Cam.transform.localPosition = new Vector3(
//                Cam.transform.localPosition.x,
//                Cam.transform.localPosition.y,
//                Mathf.Lerp(Cam.transform.localPosition.z, -Scroll, Time.deltaTime * 5F)
//            );
//            //if (FollwingTarget == null || !Active) return;
//            //transform.position = FollwingTarget.position;
//#if BUILDMODE
//            RotX.localEulerAngles = new Vector3(AngleX, AngleY, 0F);
//#endif
//            if (BuildControl.BuildMode )
//                return;
//            Vector3 movement = Vector3.zero;
//            if(CustomInput.GetKey(KeyCode.W))
//            {
//                movement.y += Time.deltaTime * MoveSpeed;
//            }
//            if (CustomInput.GetKey(KeyCode.A))
//            {
//                movement.x-= Time.deltaTime * MoveSpeed;
//            }
//            if (CustomInput.GetKey(KeyCode.S))
//            {
//                movement.y -= Time.deltaTime * MoveSpeed;
//            }
//            if (CustomInput.GetKey(KeyCode.D))
//            {
//                movement.x += Time.deltaTime * MoveSpeed;
//            }
//            Cam.transform.localPosition += movement;
//            //Debug.Log(AngleY+"+" +AngleX);
//            //RotY.localEulerAngles = new Vector3(0F, AngleY, 0F);

//            //Cam.transform.localPosition = new Vector3(0F,0F, Mathf.Lerp(Cam.transform.localPosition.z, Mathf.Max(-Clipping, -Scroll),Time.deltaTime* (Clipped ? 100F:2F)));
//        }

//        //private void FixedUpdate()
//        //{
//        //    //if (!Active) return;

//        //    //if (Physics.Linecast(RotX.transform.position, Cam.transform.position - Cam.transform.forward * 2F, out GroundHit, GroundLayer, QueryTriggerInteraction.Ignore))
//        //    //{
//        //    //    Clipping = Mathf.Min(Scroll, GroundHit.distance - 0.5F);
//        //    //    Clipped = true;
//        //    //}
//        //    //else
//        //    //{
//        //    //    Clipping = Scroll;
//        //    //    Clipped = false;
//        //    //}

//        //}
//    }
//}
