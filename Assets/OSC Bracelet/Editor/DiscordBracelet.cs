#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.IO;
using VRC.SDK3.Avatars.ScriptableObjects;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;

namespace Shadoki
{
    public class DiscordBracelet : EditorWindow
    {
        private GameObject braceletOnAvatar;
        private VRCExpressionParameters vrcExpressionParameters;
        private AnimatorController avatarFXLayerController;
        private AnimatorController discordBraceletController;
        private AnimationClip notificationCallAnim;
        private AnimationClip notificationEndAnim;
        private AnimationClip notificationStartAnim;
        private AnimationClip notificationWaitingAnim;


        [MenuItem("Shadoki/Discord Bracelet")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<DiscordBracelet>();
        }

        private void HandleClick()
        {
            if (
            braceletOnAvatar == null
            || notificationCallAnim == null
            || notificationEndAnim == null
            || notificationStartAnim == null
            || notificationWaitingAnim == null
            )
            {
                return;
            }
            string braceletPath = GetObjectPath(braceletOnAvatar);
            Debug.Log(braceletPath);
            if (!braceletPath.EndsWith("DiscordBracelet/"))
            {
                Debug.LogError("[DiscordBracelet] Expected game object to have name 'DiscordBracelet'. Did you grab the right object?");
                return;
            }
            AssetDatabase.StartAssetEditing();
            List<AnimationClip> animationClips = new List<AnimationClip>(){
                notificationCallAnim, notificationEndAnim, notificationStartAnim, notificationWaitingAnim
            };
            animationClips.ForEach(clip =>
            {
                ReplacePath(clip, braceletPath);
            });
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
            if (avatarFXLayerController != null && discordBraceletController != null)
            {
                Debug.Log("[DiscordBracelet] Merging animator controllers...");
                try
                {
                    AnimatorCloner.MergeControllers(avatarFXLayerController, discordBraceletController, null, true);
                }
                catch (System.Exception error)
                {
                    Debug.LogError(error);
                }
                Debug.Log("[DiscordBracelet] Merging complete");
            }

            if (vrcExpressionParameters != null)
            {
                Debug.Log("[DiscordBracelet] Adding expression parameters...");
                AddExpressionParameterValues(vrcExpressionParameters);
                Debug.Log("[DiscordBracelet] Expression parameters complete");
            }

            Debug.Log("[DiscordBracelet] Complete!");
        }

        private void AddExpressionParameterValues(VRCExpressionParameters vrcExpressionParameters)
        {
            var osc_discord_band = "osc_discord_band";
            var osc_discord_call = "osc_discord_call";
            var oscDiscordBandParamFound = false;
            var oscDiscordCallParamFound = false;
            foreach (var param in vrcExpressionParameters.parameters)
            {
                if (param.name == osc_discord_band)
                {
                    oscDiscordBandParamFound = true;
                }
                if (param.name == osc_discord_call)
                {
                    oscDiscordCallParamFound = true;
                }
            }
            if (!oscDiscordBandParamFound)
            {
                Debug.Log("[DiscordBracelet] Creating osc_discord_band parameter");
                var parameters = CloneExpressionParameters(vrcExpressionParameters.parameters, 1);
                var index = parameters.Length - 1;
                parameters[index] = new ExpressionParameter();
                parameters[index].name = osc_discord_band;
                parameters[index].valueType = ExpressionParameters.ValueType.Bool;
                parameters[index].saved = false;
                vrcExpressionParameters.parameters = parameters;
            }
            if (!oscDiscordCallParamFound)
            {
                Debug.Log("[DiscordBracelet] Creating osc_discord_call parameter");
                var parameters = CloneExpressionParameters(vrcExpressionParameters.parameters, 1);
                var index = parameters.Length - 1;
                parameters[index] = new ExpressionParameter();
                parameters[index].name = osc_discord_call;
                parameters[index].valueType = ExpressionParameters.ValueType.Bool;
                parameters[index].saved = false;
                vrcExpressionParameters.parameters = parameters;
            }

        }

        private ExpressionParameter[] CloneExpressionParameters(ExpressionParameter[] expressionParameters, int addedLength)
        {
            var result = new ExpressionParameter[expressionParameters.Length + addedLength];
            for (int i = 0; i < expressionParameters.Length; i++)
            {
                result[i] = expressionParameters[i];
            }
            return result;
        }

        static string GetObjectPath(GameObject currentGameObject)
        {
            if (currentGameObject == null)
                return "";

            string path = currentGameObject.name;

            while (currentGameObject.transform.parent != null)
            {
                currentGameObject = currentGameObject.transform.parent.gameObject;
                // Bails early if the parent above is null
                if (currentGameObject.transform.parent == null)
                {
                    return path + "/";
                }
                path = $"{currentGameObject.name}/{path}";
            }

            return path + "/";
        }

        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("Discord Bracelet on your Avatar", EditorStyles.label);
            braceletOnAvatar = (GameObject)EditorGUILayout.ObjectField(braceletOnAvatar, typeof(GameObject), true);
            GUILayout.Space(5);
            GUILayout.Label("Your FX Layer Animator Controller", EditorStyles.label);
            avatarFXLayerController = (AnimatorController)EditorGUILayout.ObjectField(avatarFXLayerController, typeof(AnimatorController), true);
            GUILayout.Space(5);
            GUILayout.Label("Your Avatar Expressions Parameters", EditorStyles.label);
            vrcExpressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcExpressionParameters, typeof(VRCExpressionParameters), true);
            GUILayout.Space(5);
            GUILayout.Label("Discord Bracelet Animator", EditorStyles.label);
            discordBraceletController = (AnimatorController)EditorGUILayout.ObjectField(discordBraceletController, typeof(AnimatorController), false);
            GUILayout.Space(5);
            GUILayout.Label("Notification Call Animation", EditorStyles.label);
            notificationCallAnim = (AnimationClip)EditorGUILayout.ObjectField(notificationCallAnim, typeof(AnimationClip), false);
            GUILayout.Space(5);
            GUILayout.Label("Notification Start Animation", EditorStyles.label);
            notificationStartAnim = (AnimationClip)EditorGUILayout.ObjectField(notificationStartAnim, typeof(AnimationClip), false);
            GUILayout.Space(5);
            GUILayout.Label("Notification Waiting Animation", EditorStyles.label);
            notificationWaitingAnim = (AnimationClip)EditorGUILayout.ObjectField(notificationWaitingAnim, typeof(AnimationClip), false);
            GUILayout.Space(5);
            GUILayout.Label("Notification End Animation", EditorStyles.label);
            notificationEndAnim = (AnimationClip)EditorGUILayout.ObjectField(notificationEndAnim, typeof(AnimationClip), false);
            GUILayout.Space(10);
            GUILayout.Label("This will not modify your FX layer, it will create a new one instead", EditorStyles.label);
            GUILayout.Label("in the OSC Bracelet/Animations directory.", EditorStyles.label);
            GUILayout.Space(10);
            GUILayout.Label("This will modify your existing expression parameters.", EditorStyles.label);
            GUILayout.Space(10);
            if (GUILayout.Button("Apply"))
            {
                HandleClick();
            }
        }

        string CreateNewPathLine(string replacer, string path)
        {
            return $"    path: {path}{replacer}";
        }

        void ReplacePath(AnimationClip animationClip, string newPath)
        {
            Undo.RecordObject(animationClip, "Animation Path Replacement");
            string assetPath = AssetDatabase.GetAssetPath(animationClip);
            string[] allLines = File.ReadAllLines(assetPath);
            int replacementCount = 0;
            string replacer1 = "Discord Band/Logo Bone";
            string replacer2 = "Discord Band";
            string replacer3 = "Logo Bone";
            string replacer4 = "Logo Mesh";
            StringBuilder newFileContents = new StringBuilder();

            for (int i = 0; i < allLines.Length; ++i)
            {
                string lineToAppend = "";
                if (allLines[i].StartsWith("    path: "))
                {
                    string newLine = "";
                    if (allLines[i].EndsWith(replacer1))
                    {
                        newLine = CreateNewPathLine(replacer1, newPath);
                    }
                    else if (allLines[i].EndsWith(replacer2))
                    {
                        newLine = CreateNewPathLine(replacer2, newPath);
                    }
                    else if (allLines[i].EndsWith(replacer3))
                    {
                        newLine = CreateNewPathLine(replacer3, newPath);
                    }
                    else if (allLines[i].EndsWith(replacer4))
                    {
                        newLine = CreateNewPathLine(replacer4, newPath);
                    }
                    if (newLine != "")
                    {
                        if (allLines[i] != newLine)
                        {
                            lineToAppend = newLine;
                            replacementCount++;
                        }
                    }
                }
                string nextLine = lineToAppend != "" ? lineToAppend : allLines[i];
                newFileContents.AppendLine(nextLine.TrimEnd(Environment.NewLine.ToCharArray()));
            }

            if (replacementCount > 0)
            {
                File.WriteAllText(assetPath, newFileContents.ToString());
            }
            Debug.Log("[DiscordBracelet] Replaced " + replacementCount + " paths in " + assetPath);
        }
    }
}
#endif
