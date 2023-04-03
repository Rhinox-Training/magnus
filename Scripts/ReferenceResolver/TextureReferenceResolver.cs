using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.Magnus
{
    [Serializable]
    public class TextureReferenceResolver : IObjectReferenceResolver
    {
        public string Name;
        
        public int Width;
        public int Height;
        public TextureFormat Format;
        public bool MipMaps;
        
        // Note: This can cause issues with textures that contain too much info
        // Make sure they are a resource to prevent this
        public string EncodedPixels;

        public string ErrorMessage => $"Texture error"; // Should never happen?
        public string Description => $"Contains: {Width}x{Height} texture [{Format}]";

        public UnityEngine.Object Resolve()
        {
            var tex = new Texture2D(Width, Height, Format, MipMaps);
            tex.name = Name;
            
            var data = Convert.FromBase64String(EncodedPixels);
            tex.LoadRawTextureData(data);

            return tex;
        }

        [ReferenceResolver(25)]
        public static bool TryEncode(UnityEngine.Object target, out IObjectReferenceResolver resolver)
        {
            resolver = null;
            if (!(target is Texture2D tex))
                return false;

            resolver = new TextureReferenceResolver
            {
                Name = tex.name,
                Width = tex.width,
                Height = tex.height,
                Format = tex.format,
                MipMaps = tex.mipmapCount != 1,
                EncodedPixels = Convert.ToBase64String(tex.GetRawTextureData())
            };
            
            return true;
        }

        protected bool Equals(TextureReferenceResolver other)
        {
            return Name == other.Name &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Format == other.Format &&
                   MipMaps == other.MipMaps &&
                   EncodedPixels == other.EncodedPixels;
        }

        public bool Equals(IObjectReferenceResolver obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureReferenceResolver) obj);
        }
    }
}