using UnityEngine;

namespace Sources.Runtime.Gameplay
{
    // ── Febucci effect tags you can use inside any Content string:
    //
    //   Animations  : <wiggle>  <shake>  <bounce>  <wave>  <swing>
    //   Colors      : <rainbow>  <color=#FF6B6B>red</color>  <gradient="#00C9FF","#92FE9D">text</gradient>
    //   TMP styling : <b>bold</b>  <i>italic</i>  <u>underline</u>  <size=130%>big</size>
    //
    //   Example Content entry:
    //   "Hello, <wiggle><color=#FFD700>adventurer</color></wiggle>! Are you <shake>ready</shake>?"
    
    [CreateAssetMenu(menuName = "Data/Dialogue", fileName = "DialogueData")]
    public sealed class DialogueData : ScriptableObject
    {
        [field: SerializeField, TextArea(3,8)] public string[] Contents { get; private set; }
    }
}