using System.Linq;
using UnityEngine;

namespace Rhinox.Magnus
{
    public static class PlayerExtensions
    {
        public static Ray GetViewRay(this Player p)
        {
            return new Ray(p.ViewOrigin, p.ViewDirection);
        }
    }
}