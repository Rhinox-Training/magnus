using System;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


namespace Rhinox.Magnus
{
    [SmartFallbackDrawn]
    public class PlayerStart : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Color playerStartColor = new Color(0, 0.8f, 0, 1);
        
        [FoldoutGroup("Proportions")][SerializeField] private float _playerHeight = 1.8f;
        [FoldoutGroup("Proportions")][SerializeField] private float _headSize = 0.3f;
        [FoldoutGroup("Proportions")][SerializeField] private float _legToHeightRatio = 0.45f;
        [FoldoutGroup("Proportions")][SerializeField] private float _torsoWidth = 0.44f;
        [FoldoutGroup("Proportions")][SerializeField] private float _armToLegLengthRatio = 0.92f;
        [FoldoutGroup("Proportions")][SerializeField] private float _legToLegHeadRadiusRatio = 0.35f;
        [FoldoutGroup("Proportions")][SerializeField] private float _legCenterOffset = 1.2f;
        
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
        }

        [MenuItem(OPERATION_NAME, true)]
        private static bool ValidateCreate()
        {
            return Selection.transforms.Length <= 1;
        }

        private void OnDrawGizmos()
        {
            var matCache = Gizmos.matrix;
            Color oldColor = Gizmos.color;
            Gizmos.matrix = transform.localToWorldMatrix;
            {
                Gizmos.color = playerStartColor;

                Gizmos.DrawWireSphere(new Vector3(0, _playerHeight - (_headSize / 2.0f), 0), (_headSize / 2.0f));

                float legLength = _playerHeight * _legToHeightRatio;
                float torsoLength = _playerHeight - (_headSize + legLength);
                
                Gizmos.DrawWireCube(new Vector3(0, legLength + torsoLength / 2.0f, 0),
                    new Vector3(_torsoWidth, torsoLength, _torsoWidth / 2.7f));

                // Draw limbs
                float legRadius = _headSize * _legToLegHeadRadiusRatio;
                float legOffset = legRadius * _legCenterOffset;
                GizmosExt.DrawCapsule(new Vector3(-legOffset, legLength, 0),
                    new Vector3(-legOffset, 0, 0), playerStartColor, legRadius);

                GizmosExt.DrawCapsule(new Vector3(legOffset, legLength, 0),
                    new Vector3(legOffset, 0, 0), playerStartColor, legRadius);

                float armLength = legLength * _armToLegLengthRatio;
                float armRadius = legRadius;

                float armOffset = armRadius + (_torsoWidth / 2.0f);

                GizmosExt.DrawCapsule(new Vector3(-armOffset, _playerHeight - _headSize, 0),
                    new Vector3(-armOffset, (_playerHeight - _headSize - armLength), 0),
                    playerStartColor, armRadius);

                GizmosExt.DrawCapsule(new Vector3(armOffset, _playerHeight - _headSize, 0),
                    new Vector3(armOffset, (_playerHeight - _headSize - armLength), 0),
                    playerStartColor, armRadius);

                // Draw bottom gizmo
                GizmosExt.DrawCircle(Vector3.zero, transform.up, playerStartColor, 0.5f);
                GizmosExt.DrawArrow(Vector3.zero, Vector3.forward, playerStartColor);

                Gizmos.color = oldColor;
            }
            Gizmos.matrix = matCache;
        }
#endif
    }
}