using UnityEngine;

namespace Rhinox.Magnus
{
    public static class UnityTypeParser
    {
        /// <summary>
        /// Attempts to parse a Vector3 from a string of following format:
        /// x,y,z
        /// </summary>
        /// <param name="input">The string holding the info.</param>
        /// <param name="result">Out Vector3 parameters for the result (if successful).</param>
        /// <returns></returns>
        public static bool TryParseVector3(string input, out Vector3 result)
        {
            // Reset the result
            result = Vector3.zero;

            //Split the given string into substrings
            string[] values = input.Split(',');
            if (values.Length != 3)
                return false;

            // Attempt to parse the individual floats
            if (!float.TryParse(values[0], out float x) ||
                !float.TryParse(values[1], out float y) ||
                !float.TryParse(values[2], out float z))
                return false;

            // Set the result
            result = new Vector3(x, y, z);
            return true;
        }

        /// <summary>
        /// Attempts to parse a Quaternion from a string of following format:
        /// x,y,z,w
        /// </summary>
        /// <param name="input">The string holding the info.</param>
        /// <param name="result">Out Quaternion parameters for the result (if successful).</param>
        /// <returns></returns>
        public static bool TryParseQuaternion(string input, out Quaternion result)
        {
            // Reset the result
            result = Quaternion.identity;

            //Split the given string into substrings
            string[] values = input.Split(',');
            if (values.Length != 4)
                return false;

            // Attempt to parse the individual floats
            if (!float.TryParse(values[0], out float x) ||
                !float.TryParse(values[1], out float y) ||
                !float.TryParse(values[2], out float z) ||
                !float.TryParse(values[3], out float w))
                return false;

            // Set the result
            result = new Quaternion(x, y, z, w);
            return true;
        }

        /// <summary>
        /// Tries to parse a layer from a given string. The string can either be the name of the layer or the index.
        /// </summary>
        /// <param name="input">The input string representing either the name or the index of the layer.</param>
        /// <param name="layer">The output layer number if the input is valid, otherwise -1.</param>
        /// <returns>True if the input is valid and a layer is parsed, otherwise false.</returns>
        public static bool TryParseLayer(string input, out int layer)
        {
            layer = -1;

            if (int.TryParse(input, out int layerIndex))
            {
                if (IsValidLayerIndex(layerIndex))
                {
                    layer = layerIndex;
                    return true;
                }
            }
            else
            {
                int layerId = LayerMask.NameToLayer(input);
                if (IsValidLayerIndex(layerId))
                {
                    layer = layerId;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the layer index is valid.
        /// </summary>
        /// <param name="layerIndex">The layer index to be checked.</param>
        /// <returns>True if the layer index is valid, otherwise false.</returns>
        private static bool IsValidLayerIndex(int layerIndex)
        {
            return layerIndex >= 0 && layerIndex < 32;
        }
    }
}