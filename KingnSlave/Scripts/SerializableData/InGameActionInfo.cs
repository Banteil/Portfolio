using System;

[Serializable]
public class InGameActionInfo
{
    public string room_no;
    public string uid;
    public string action_cd;
    public string card_cd;
    public int card_no;
    public int round_no;

    public InGameActionInfo(string room_no_, string uid_, string action_cd_, string card_cd_, int card_no_, int round_no_)
    {
        room_no = room_no_;
        uid = uid_;
        action_cd = action_cd_;
        card_cd = card_cd_;
        card_no = card_no_;
        round_no = round_no_;
    }
}