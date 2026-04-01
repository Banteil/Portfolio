using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConversationContent
{
    string[] content =
    {
        "엘프 여성;0;밖에는 몬스터들이 있어요.",
        "엘프 여성;0;몸 조심 하세요.",
        "엘프 여성;1;조심하세요.",        
    };

    public string[] Content { get { return content; } }
}
