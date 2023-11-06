// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace GLTFast.Loading {
    public class LocalFileProvider : IDownloadProvider {
        public async Task<IDownload> Request(Uri url)
        {
            Debug.Log($"Started loading");
            var req = new FileLoad(url);
            while (req.MoveNext())
            {
                await Task.Yield();
            }
            req.Close();
            Debug.Log($"Ended loading");
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
        {
            var req = new AwaitableTextureDownload(url, nonReadable);
            await req.WaitAsync();
            // while (req.MoveNext())
            // {
            //     await Task.Yield();
            // }
            return req;
        }
    }

    public class FileLoad : IDownload {

        protected const Int32 bufferSize = 32 * 4096;
        protected FileStream fileStream;

        protected string path;
        protected int length;
        protected int sumLoaded;
        protected byte[] bytes;
        protected string readError = null;

        public FileLoad() { }

        public FileLoad(Uri url)
        {
            if (url.Scheme != "file")
            {
                throw new ArgumentException("[FileLoad] FileLoad can only load uris starting with file:");
            }
            path = url.LocalPath;
            if (!File.Exists(path))
            {
                throw new ArgumentException("[FileLoad] File " + url.LocalPath + " does not exist!");
            }
            Init();
        }

        public void Close()
        {
            Debug.Log("[FileLoad] Closing " + fileStream);
            fileStream.Close();
        }

        protected void Init()
        {
            Debug.Log("[FileLoad] Opening " + path);
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            length = (int)fileStream.Length;
            sumLoaded = 0;
            bytes = new byte[length];
        }

        public bool MoveNext()
        {
            if (fileStream == null)
            {
                Debug.LogWarning("[FileLoad] filestream is null, this shouldn't happend!");
                return false;
            }

            if (Success)
                return false;

            try
            {
                var readSize = Math.Min(length - sumLoaded, bufferSize);
                var count = fileStream.Read(bytes, sumLoaded, readSize);
                sumLoaded += count;
                return count > 0;
            }
            catch (Exception e)
            {
                readError = e.Message;
                Debug.LogError("[FileLoad] error: " + readError);
                throw e;
            }
        }

        public void Reset() { }
        public bool Success => sumLoaded >= length;
        public string Error => readError;
        public byte[] Data => bytes;
        public string Text { get { return System.Text.Encoding.UTF8.GetString(bytes); } }
        public bool? IsBinary {
            get {
                if (Success)
                {
                    return path.EndsWith(".glb");
                }
                else
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            fileStream?.Dispose();
        }
    }

    public class AwaitableTextureLoad :  ITextureDownload
    {
        public AwaitableTextureLoad(Uri url)
        {
            Init(url);
        }

        public Task<byte[]> readingTask { get; private set; }
        private Texture2D myTex;
        protected void Init(Uri url)
        {
            myTex = new Texture2D(2, 2);
            readingTask = File.ReadAllBytesAsync(url.LocalPath);
        }

        public void GenerateTexture()
        {
            myTex = new Texture2D(1, 1);
            myTex.LoadImage(readingTask.Result);
        }
        
        public Texture2D Texture => myTex;

        public void Dispose()
        {
            myTex = null;
            readingTask = null;
        }

        public bool Success => readingTask.IsCompleted;
        public string Error => readingTask.Exception?.ToString();
        public byte[] Data => readingTask.Result;
        public string Text => throw new NotImplementedException();
        public bool? IsBinary => throw new NotImplementedException();
    }
}