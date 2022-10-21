using System.Collections.Generic;

namespace XMindAPI.Core
{
    public interface IImage
    {
        void AddImage(byte[] imageBinary,string filename);
    }
}