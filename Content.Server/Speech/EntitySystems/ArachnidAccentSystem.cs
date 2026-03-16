using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class ArachnidAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArachnidAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, ArachnidAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;
        var msg_res = new System.Text.StringBuilder();

        foreach (var c in message)
        {
            msg_res.Append(c);

            if ("ццччщщЦЦЧЧЩЩ".Contains(c)) 
            {
                if (_random.Prob(0.3f)) 
                {
                    msg_res.Append(c);
                }
            }
        }

        args.Message = AddEndingTch(msg_res.ToString());
    }

    public string AddEndingTch(string msg_res)
    {
        var msg = msg_res;
        var ending = "";
        char last_letter = '.';
        int lastLetterIdx = -1;
        for (int j = msg.Length - 1; j >= 0; j--)
        {
            if (char.IsLetter(msg[j]))
            {
                lastLetterIdx = j;
                break;
            }
        }
        if (lastLetterIdx == -1)
        return msg_res;
        var textPart = msg.Substring(0, lastLetterIdx + 1);
        var punctuationPart = msg.Substring(lastLetterIdx + 1);       

        int i = 1;
        while(i < msg.Length)
            {
                if(char.IsLetter(msg[^i]))
                {
                    last_letter = msg[^i];
                    break;
                }
                i++;
            }
        if(msg_res.Length > 5)
        {
            if(_random.Prob(0.1f))
            {
                int choice = 0;
                if(!string.IsNullOrEmpty(msg))
                {
                    choice = _random.Next(2);
                }

                switch(choice)
                {
                    case 0:
                        ending ="-тц";
                        break;
                    case 1:
                        ending = "-кх";
                        break;
                }
            }
        }

        if (!string.IsNullOrEmpty(msg) && char.IsUpper(last_letter))
        {
            ending = ending.ToUpper();
        }

        return textPart + ending + punctuationPart;
    }
}
