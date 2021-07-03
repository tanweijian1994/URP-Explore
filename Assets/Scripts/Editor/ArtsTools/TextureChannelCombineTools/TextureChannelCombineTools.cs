using UnityEditor;
using UnityEngine;

public class TextureChannelCombineTools : EditorWindow
{
    [MenuItem("Arts Tools/Texture/ChannelCombineTools[window]")]
    public static void ShowExample()
    {
        TextureChannelCombineTools wnd = GetWindow<TextureChannelCombineTools>();
        wnd.titleContent = new GUIContent("TextureChannelCombineTools");
    }
}