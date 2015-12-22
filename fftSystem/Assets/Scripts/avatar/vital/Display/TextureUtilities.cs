using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// This class is a Helper function to provide texture utilities. 
/// </summary>
public class TextureUtilities
{
	
	/// <summary>
	/// Combines two textures based on the the region of interest template.
	/// </summary>
	/// <returns>
	/// The combined texture.
	/// </returns>
	/// <param name='org'>
	/// The original texture.
	/// </param>
	/// <param name='overlay'>
	/// The overlay texture.
	/// </param>
	/// <param name='bodytemplate'>
	/// The indices of the Bodytemplate.
	/// </param>
	public static Texture2D CombineTexturesFromTemplate (Texture2D org, Texture2D overlay, int [] bodytemplate)
	{				
		Color[] overlayPixels = overlay.GetPixels ();
		Color[] orgPixels = org.GetPixels ();
			
		for (int i=0; i< bodytemplate.Length; ++i) {
			int btIdx = bodytemplate [i];
			float alphaOverlay = overlayPixels [btIdx].a;
			Color overlayPixel = overlayPixels [btIdx];
			Color basePixel = orgPixels [btIdx];
					
					
					
			float ac = alphaOverlay + (1.0f - alphaOverlay) * basePixel.a;
					
			float r = (1.0f / ac) * (alphaOverlay * overlayPixel.r + (1.0f - alphaOverlay) * basePixel.a * basePixel.r);
			float g = (1.0f / ac) * (alphaOverlay * overlayPixel.g + (1.0f - alphaOverlay) * basePixel.a * basePixel.g);
			float b = (1.0f / ac) * (alphaOverlay * overlayPixel.b + (1.0f - alphaOverlay) * basePixel.a * basePixel.b);
					
					
					
			Color newPix = new Color (r, g, b, ac);
	
			
			orgPixels [btIdx] = newPix;
						
		}
			
		org.SetPixels (orgPixels);
		org.Apply ();
				
				
		return org;
	}
			
			
			
			
	/// <summary>
	/// Combines the textures from template.
	/// </summary>
	/// <returns>
	/// The textures from template.
	/// </returns>
	/// <param name='org'>
	/// Org.
	/// </param>
	/// <param name='overlay'>
	/// Overlay.
	/// </param>
	/// <param name='bodytemplate'>
	/// Bodytemplate.
	/// </param>
	/// <param name='regions'>
	/// Regions.
	/// </param>
	/// <param name='combineAlpha'>
	/// Combine alpha.
	/// </param>
	public static Texture2D CombineTexturesFromTemplate (Texture2D org, Texture2D overlay, Texture2D bodytemplate, Color32 [] regions, bool combineAlpha)
	{
				
		Color[] overlayPixels = overlay.GetPixels ();
		Color[] orgPixels = org.GetPixels ();
		Color[] bodyPixels = bodytemplate.GetPixels ();
				
				
		for (int i = 0; i < orgPixels.Length; ++i) {

			foreach (Color32 c in regions) {
						
				//seems like unity3d has an import bug. pixels are somehow changed
				if ((bodyPixels [i].r) >= (c.r - 3) && (bodyPixels [i].r) <= (c.r + 3)) {
							
					float alphaOverlay = overlayPixels [i].a;
					Color overlayPixel = overlayPixels [i];
					Color basePixel = orgPixels [i];
							
							
							
					float ac = alphaOverlay + (1.0f - alphaOverlay) * basePixel.a;
							
					float r = (1.0f / ac) * (alphaOverlay * overlayPixel.r + (1.0f - alphaOverlay) * basePixel.a * basePixel.r);
							
							
							
					float g = (1.0f / ac) * (alphaOverlay * overlayPixel.g + (1.0f - alphaOverlay) * basePixel.a * basePixel.g);
					float b = (1.0f / ac) * (alphaOverlay * overlayPixel.b + (1.0f - alphaOverlay) * basePixel.a * basePixel.b);
							
							
							
					Color newPix = new Color (r, g, b, ac);
					orgPixels [i] = newPix;
				}
			}
					
		}
		org.SetPixels (orgPixels);
		org.Apply ();
				
				
		return org;
	}
			
			
	/// <summary>
	/// Combines two textures with alpha blending.
	/// </summary>
	/// <returns>
	/// Pixelarray as color32
	/// </returns>
	/// <param name='a'>
	/// Texture a (base)
	/// </param>
	/// <param name='b'>
	/// Texture b (overlay).
	/// </param>
	public static Color32[] CombineTextures32 (Texture2D a, Texture2D b)
	{
		Color32[] aPixels = a.GetPixels32 ();
		Color32[] bPixels = b.GetPixels32 ();
		for (int i = 0; i < aPixels.Length; ++i) {
			float alphaOverlay = bPixels [i].a / 255.0f;
			Color32 newPix = new Color32 (
				
						(byte)(alphaOverlay * ((byte)bPixels [i].r) + (1 - alphaOverlay) * aPixels [i].r),
	                    (byte)(alphaOverlay * ((byte)bPixels [i].g) + (1 - alphaOverlay) * aPixels [i].g),
					    (byte)(alphaOverlay * ((byte)bPixels [i].b) + (1 - alphaOverlay) * aPixels [i].b),
				        (byte)Mathf.Min (255, bPixels [i].a + aPixels [i].a)
				     );
				
			aPixels [i] = newPix;
					
		}
	
		return aPixels;
	}
		
	/// <summary>
	/// Combines two textures with alpha blending.
	/// </summary>
	/// <returns>
	/// Pixelarray of the texture
	/// </returns>
	/// <param name='baseTexture'>
	/// Base texture.
	/// </param>
	/// <param name='overlayTexture'>
	/// Overlay texture.
	/// </param>
	public static Color[] CombineTextures (Texture2D baseTexture, Texture2D overlayTexture)
	{				
		Color[] basePixels = baseTexture.GetPixels ();
		Color[] overlayPixels = overlayTexture.GetPixels ();
				
		for (int i = 0; i < basePixels.Length; ++i) {	
			float alphaOverlay = overlayPixels [i].a;
			Color overlayPixel = overlayPixels [i];
			Color basePixel = basePixels [i];
		
			float ac = alphaOverlay + (1.0f - alphaOverlay) * basePixels [i].a;				
			float r = (1.0f / ac) * (alphaOverlay * overlayPixel.r + (1.0f - alphaOverlay) * basePixel.a * basePixel.r);
			float g = (1.0f / ac) * (alphaOverlay * overlayPixel.g + (1.0f - alphaOverlay) * basePixel.a * basePixel.g);
			float b = (1.0f / ac) * (alphaOverlay * overlayPixel.b + (1.0f - alphaOverlay) * basePixel.a * basePixel.b);

			Color newPix = new Color (r, g, b, ac);
			basePixels [i] = newPix;
		}
			
		return basePixels;
	}
	
	/// <summary>
	/// Debugs the texture colors.
	/// </summary>
	/// <param name='t'>
	/// the texture
	/// </param>
	public static void DebugTextureColors (Texture2D t)
	{
		List<int> l = new List<int> ();
		Color32[] colors = t.GetPixels32 ();
		foreach (Color32 c in colors) {
			if (!l.Contains (c.r)) {
				l.Add (c.r);
			}
		}
		for (int i = 0; i < l.Count; ++i) {
			Debug.Log (l [i]);
		}
	}
			
	/// <summary>
	/// Encodes the texture to file.
	/// </summary>
	/// <param name='tex'>
	/// Texture.
	/// </param>
	/// <param name='filename'>
	/// The filename.
	/// </param>
	public static void EncodeTextureToFile (Texture2D tex, string filename)
	{
						
		byte[] newTexBytes = tex.EncodeToPNG ();
		string saveName = filename;
		if (!filename.EndsWith (".png")) {
			saveName += ".png";
		}
		FileStream fs = new FileStream (saveName, FileMode.Create);
		fs.Write (newTexBytes, 0, newTexBytes.Length);
		fs.Flush ();
		fs.Close ();
			
	}
	
	/// <summary>
	/// Combines multiple textures upon there appearance in the regioncolors. First element will be added first to orgPixels.
	/// </summary>
	/// <returns>
	/// The Color array of the combined textures.
	/// </returns>
	/// <param name='orgPixels'>
	/// The original pixels. Since this is a ref, the return and orgPixels are the same
	/// </param>
	/// <param name='regioncolors'>
	/// The List with the indices of the region of interest and the corresponding texture pixelcolors
	/// </param>
	public static Color[] CombineMultipleTextures (ref Color [] orgPixels, ref IList<KeyValuePair<int[], Color[]>> regioncolors)
	{
		for(int j = 0; j < regioncolors.Count;++j)
		{
			Color[] overlayPixels = regioncolors[j].Value;
			int [] regions = regioncolors[j].Key;
			for (int i = 0; i < regions.Length; ++i) {
					
				float alphaOverlay = overlayPixels [regions[i]].a;
				Color overlayPixel = overlayPixels [regions[i]];
				Color basePixel = orgPixels [regions[i]];
						
				float ac = alphaOverlay + (1.0f - alphaOverlay) * basePixel.a;
						
				float r = (1.0f / ac) * (alphaOverlay * overlayPixel.r + (1.0f - alphaOverlay) * basePixel.a * basePixel.r);						
				float g = (1.0f / ac) * (alphaOverlay * overlayPixel.g + (1.0f - alphaOverlay) * basePixel.a * basePixel.g);
				float b = (1.0f / ac) * (alphaOverlay * overlayPixel.b + (1.0f - alphaOverlay) * basePixel.a * basePixel.b);
				
				Color newPix = new Color (r, g, b, ac);
				orgPixels [regions[i]] = newPix;					
			}
		}
		return orgPixels;
	}	
	
	/// <summary>
	/// An enumaration for the RGB Color channels.
	/// </summary>
	public enum ColorChannel{
		red,
		green,
		blue
	}
	
	/// <summary>
	/// Adds an offset to a specific color channel.
	/// </summary>
	/// <returns>
	/// An array of colors with the channel offset.
	/// </returns>
	/// <param name='orgPixels'>
	/// the original pixels. Since this is a ref, the return and orgPixels are the same
	/// </param>
	/// <param name='regionofinterest'>
	/// The array with the indices of the region of interest.
	/// </param>
	/// <param name='factor'>
	/// the offset Factor.
	/// </param>
	/// <param name='colorChannel'>
	/// The Color channel. Combine them with the | operator
	/// </param>
	public static Color[] AddChannelOffset(ref Color [] orgPixels, int[] regionofinterest, float factor, ColorChannel colorChannel )
	{
				
		for(int j = 0; j < regionofinterest.Length;++j)
		{
			if(colorChannel== ColorChannel.red)
			{
				float oldred = orgPixels[regionofinterest[j]].r;
				float red = Mathf.Min(1.0f, oldred*(1.0f+factor));
				orgPixels[regionofinterest[j]].r = red;
			}
			if(colorChannel== ColorChannel.green)
			{
				float oldgreen = orgPixels[regionofinterest[j]].g;
				float green = Mathf.Min(1.0f, oldgreen*(1.0f+factor));
				orgPixels[regionofinterest[j]].g= green;
			}
			if(colorChannel== ColorChannel.blue)
			{
				float oldblue = orgPixels[regionofinterest[j]].b;
				float blue = Mathf.Min(1.0f, oldblue*(1.0f+factor));
				orgPixels[regionofinterest[j]].b= blue;
			}						
		}
		
		return orgPixels;
	}
		
			
}
		

