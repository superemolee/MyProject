
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaPointCacheAnimClip
{
	public string			name;
	public float			start;
	public float			end;
	public MegaRepeatMode	loop;

	public MegaPointCacheAnimClip(string _name, float _start, float _end, MegaRepeatMode _loop)
	{
		name = _name;
		start = _start;
		end = _end;
		loop = _loop;
	}
}

[AddComponentMenu("Modifiers/Point Cache Animator")]
[ExecuteInEditMode]
public class MegaPointCacheAnimator : MonoBehaviour
{
	public MegaPointCache	mod;

	public List<MegaPointCacheAnimClip>	clips = new List<MegaPointCacheAnimClip>();

	public int current = 0;
	public float t = -1.0f;	// Current clip time
	public float at = 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=1802");
	}

	public bool IsPlaying()
	{
		if ( t >= 0.0f )
			return true;

		return false;
	}

	public void SetTime(float time)
	{
		t = time;
	}

	public float GetTime()
	{
		return at;
	}

	public void PlayClip(int i)
	{
		if ( i < clips.Count )
		{
			current = i;
			t = 0.0f;
		}
	}

	public void PlayClip(string name)
	{
		for ( int i = 0; i < clips.Count; i++ )
		{
			if ( clips[i].name == name )
			{
				current = i;
				t = 0.0f;
				return;
			}
		}
	}

	public void Stop()
	{
		t = -1.0f;
	}

	public int AddClip(string name, float start, float end, MegaRepeatMode loop)
	{
		MegaPointCacheAnimClip clip = new MegaPointCacheAnimClip(name, start, end, loop);
		clips.Add(clip);
		return clips.Count - 1;
	}

	public string[] GetClipNames()
	{
		string[] names = new string[clips.Count];

		for ( int i = 0; i < clips.Count; i++ )
		{
			names[i] = clips[i].name;
		}

		return names;
	}

	void Update()
	{
		if ( mod == null )
			mod = (MegaPointCache)GetComponent<MegaPointCache>();

		if ( mod && clips.Count > 0 && current < clips.Count )
		{
			if ( t >= 0.0f )
			{
				t += Time.deltaTime;
				float dt = clips[current].end - clips[current].start;

				switch ( clips[current].loop )
				{
					case MegaRepeatMode.Loop: at = Mathf.Repeat(t, dt); break;
					case MegaRepeatMode.PingPong: at = Mathf.PingPong(t, dt); break;
					case MegaRepeatMode.Clamp: at = Mathf.Clamp(t, 0.0f, dt); break;
				}

				at += clips[current].start;
				mod.SetAnim(at);
			}
		}
	}
}