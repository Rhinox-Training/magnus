using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class PlayerStart : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/Player Start", priority = 232)]
        public static void Foobar()
        {
            Debug.Log("Foobar");
        }
        
        private const string OPERATION_NAME = "GameObject/Player Start";
        
        [MenuItem(OPERATION_NAME, false, 26)]
        private static void CreatePlayerStart(MenuCommand menuCommand)
        {
            var playerStart = new GameObject("PlayerStart").AddComponent<PlayerStart>();
            if (menuCommand.context is GameObject gameObj)
            {
                playerStart.transform.SetParent(gameObj.transform, false);
                playerStart.transform.Reset();
            }
            else
            {
            }
            
            
        }
        
        [MenuItem(OPERATION_NAME, true)]
        private static bool ValidateCreate()
        {
            return Selection.transforms.Length <= 1;
        }
        
        private void OnDrawGizmos()
        {
            Color oldColor = Gizmos.color;
            Color playerStartColor = new Color(0, 0.8f, 0, 1);
            const float playerHeight = 1.8f;
            
            Gizmos.color = playerStartColor;
            // Draw player body
            const float headSize = 0.3f;
            Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(0, playerHeight - (headSize / 2.0f), 0)), (headSize / 2.0f));

            const float legLength = playerHeight * 0.45f;
            const float torsoLength = playerHeight - (headSize + legLength);
            const float torsoWidth = 0.44f;
            Gizmos.DrawWireCube(transform.TransformPoint(new Vector3(0, legLength + torsoLength / 2.0f, 0)), 
                transform.TransformDirection(new Vector3(torsoWidth, torsoLength,torsoWidth / 2.7f)));
            
            // Draw limbs
            const float legRadius = headSize * 0.35f;
            const float legOffset = legRadius * 1.2f;
            GizmosExt.DrawCapsule(transform.TransformPoint(new Vector3(-legOffset, legLength, 0)), 
                transform.TransformPoint(new Vector3(-legOffset, 0, 0)), playerStartColor, legRadius);
            
            GizmosExt.DrawCapsule(transform.TransformPoint(new Vector3(legOffset, legLength, 0)), 
                transform.TransformPoint(new Vector3(legOffset, 0, 0)), playerStartColor, legRadius);

            const float armLength = legLength * 0.92f;
            const float armRadius = legRadius;
            
            const float armOffset = armRadius + (torsoWidth / 2.0f);
            
            GizmosExt.DrawCapsule(transform.TransformPoint(new Vector3(-armOffset,  playerHeight - headSize, 0)), 
                transform.TransformPoint(new Vector3(-armOffset, (playerHeight - headSize - armLength), 0)), playerStartColor, armRadius);
            
            GizmosExt.DrawCapsule(transform.TransformPoint(new Vector3(armOffset, playerHeight - headSize, 0)), 
                transform.TransformPoint(new Vector3(armOffset, (playerHeight - headSize - armLength), 0)), playerStartColor, armRadius);
            
            // Draw bottom gizmo
            GizmosExt.DrawCircle(transform.position, transform.up, playerStartColor, 0.5f);
            GizmosExt.DrawArrow(transform.position, transform.forward, playerStartColor);

            Gizmos.color = oldColor;
        }
#endif
    }
}