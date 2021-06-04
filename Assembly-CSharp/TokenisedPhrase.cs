using System;
using Facepunch;

[Serializable]
public class TokenisedPhrase : Translate.Phrase
{
	public override string translated
	{
		get
		{
			string text = base.translated;
			if (!text.Contains("["))
			{
				return text;
			}
			text = text.Replace("[inventory.toggle]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.toggle").ToUpper()));
			text = text.Replace("[inventory.togglecrafting]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.togglecrafting").ToUpper()));
			text = text.Replace("[+map]", string.Format("[{0}]", Input.GetButtonWithBind("+map").ToUpper()));
			text = text.Replace("[inventory.examineheld]", string.Format("[{0}]", Input.GetButtonWithBind("inventory.examineheld").ToUpper()));
			text = text.Replace("[slot2]", string.Format("[{0}]", Input.GetButtonWithBind("+slot2").ToUpper()));
			text = text.Replace("[attack]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+attack")).ToUpper()));
			text = text.Replace("[attack2]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+attack2")).ToUpper()));
			text = text.Replace("[+use]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+use")).ToUpper()));
			text = text.Replace("[+altlook]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+altlook")).ToUpper()));
			text = text.Replace("[+reload]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+reload")).ToUpper()));
			text = text.Replace("[+voice]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+voice")).ToUpper()));
			text = text.Replace("[+lockBreakHealthPercent]", $"{0.15f:0%}");
			return text.Replace("[+gestures]", string.Format("[{0}]", TranslateMouseButton(Input.GetButtonWithBind("+gestures")).ToUpper()));
		}
	}

	public TokenisedPhrase(string t = "", string eng = "")
		: base(t, eng)
	{
	}

	public static string TranslateMouseButton(string mouseButton)
	{
		switch (mouseButton)
		{
		case "mouse0":
			return "Left Mouse";
		case "mouse1":
			return "Right Mouse";
		case "mouse2":
			return "Center Mouse";
		default:
			return mouseButton;
		}
	}
}
