using System;
using UnityEngine;

namespace MVXUnity
{
    public static class MvxUtils
    {
        public static void EnsureCollectionMinimalCapacity<T>(ref T[] collection, UInt32 minimalCapacity)
        {
            if (collection == null || collection.Length < minimalCapacity)
                collection = new T[minimalCapacity];
        }

        private static float[] m_boundingBoxData = new float[6];
        
        public static Bounds GetFrameBoundingBox(Mvx2API.Frame frame)
        {
            Mvx2API.FrameMeshExtractor.GetMeshData(frame).CopyBoundingBox(m_boundingBoxData);

            Vector3 boundingBoxVector1 = new Vector3(m_boundingBoxData[0], m_boundingBoxData[1], m_boundingBoxData[2]);
            Vector3 boundingBoxVector2 = new Vector3(m_boundingBoxData[3], m_boundingBoxData[4], m_boundingBoxData[5]);
            Vector3 boundingBoxCenter = (boundingBoxVector1 + boundingBoxVector2) / 2.0f;
            Vector3 boundingBoxSize = boundingBoxVector2 - boundingBoxVector1;
            return new Bounds(boundingBoxCenter, boundingBoxSize);
        }
    }
}
