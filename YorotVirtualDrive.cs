using HTAlt;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Yorot
{
    public class YorotVirtualDrive : YorotVirtualDirectory
    {
        public new YorotVirtualDrive ParentDir = null;
        public new YorotVirtualDrive ParentDrive = null;
    }

    public class YorotVirtualDirectory
    {
        private static List<YorotVirtualFile> yorotVirtualFiles = new List<YorotVirtualFile>();
        private List<YorotVirtualFile> files = yorotVirtualFiles;

        public YorotVirtualDirectory ParentDir { get; }
        public object SymbolicLinkTo { get; set; }
        public YorotVirtualDrive ParentDrive { get; }

        public List<YorotVirtualFile> Files
        {
            get
            {
                return files;
            }
            set
            {
                files = value;
            }
        }

        public YorotFilePermission OwnerUserRights { get; set; } = new YorotFilePermission() { CanDelete = true, CanExecute = false, CanRead = true, CanWrite = true, };
        public YorotFilePermission OtherUserRights { get; set; } = new YorotFilePermission() { CanDelete = false, CanExecute = false, CanRead = true, CanWrite = false, };
        public YorotFilePermission OtherRights { get; set; } = new YorotFilePermission() { CanDelete = false, CanExecute = false, CanRead = true, CanWrite = false, };
    }

    public class YorotVirtualFile : YorotVirtualDirectory
    {
        public new YorotVirtualDrive ParentDrive => ParentDir.ParentDrive;
        public string Name => NameWithoutExtension + (FileExtension.StartsWith(".") ? "" : ".") + FileExtension.ToLowerEnglish();
        public string NameWithoutExtension { get; set; }
        public string FileExtension { get; set; }
        public System.IO.Stream ContentStream { get; set; }

        public void Write(byte[] content, bool append = false)
        {
            if (append)
            {
            }
            else
            {
                ContentStream = new System.IO.MemoryStream(content);
            }
        }

        public void Write(string content, System.Text.Encoding encoding, bool append = false) => Write(encoding.GetBytes(content), append);

        public string ToString(Encoding encoding)
        {
            if (encoding is null)
            {
                encoding = Encoding.Unicode;
            }
            var bytes = new List<byte>();
            long contentPos = ContentStream.Position;
            ContentStream.Position = 0;
            long i = 0;
            while (i < ContentStream.Length)
            {
                bytes.Add((byte)ContentStream.ReadByte());
                i++;
            }
            ContentStream.Position = contentPos;
            return encoding.GetString(bytes.ToArray());
        }

        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            long contentPos = ContentStream.Position;
            ContentStream.Position = 0;
            long i = 0;
            while (i < ContentStream.Length)
            {
                bytes.Add((byte)ContentStream.ReadByte());
                i++;
            }
            ContentStream.Position = contentPos;
            return bytes.ToArray();
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            return new System.Drawing.Bitmap(ContentStream);
        }

        public YorotProfile FileOwner { get; }
        public YorotFileChecksum[] FileChecksums { get; set; }

        public bool Verify()
        {
            for (int i = 0; i < FileChecksums.Length; i++)
            {
                if (!FileChecksums[i].Verify())
                {
                    return false;
                }
            }
            return true;
        }

        public string GenerateHash(System.Security.Cryptography.HashAlgorithm Algorithm = null)
        {
            if (Algorithm is null)
            {
                Algorithm = System.Security.Cryptography.MD5.Create();
            }
            YorotFileChecksum foundHash = null;
            for (int i = 0; i < FileChecksums.Length; i++)
            {
                if (FileChecksums[i].Algorithm.Equals(Algorithm))
                {
                    foundHash = FileChecksums[i];
                }
            }
            if (foundHash is null)
            {
                List<YorotFileChecksum> checksums = new List<YorotFileChecksum>();
                checksums.AddRange(FileChecksums);
                YorotFileChecksum checksum = new YorotFileChecksum(Algorithm, HTAlt.Tools.BytesToString(Algorithm.ComputeHash(ContentStream)), this);
                checksums.Add(checksum);
                FileChecksums = checksums.ToArray();
                return checksum.Hash;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(foundHash.Hash))
                {
                    foundHash.Hash = HTAlt.Tools.BytesToString(Algorithm.ComputeHash(ContentStream));
                }
                return foundHash.Hash;
            }
        }

        public void GenerateAlgorithm(System.Security.Cryptography.HashAlgorithm Algorithm = null, bool calculateHash = false)
        {
            if (Algorithm is null)
            {
                Algorithm = System.Security.Cryptography.MD5.Create();
            }
            YorotFileChecksum foundHash = null;
            for (int i = 0; i < FileChecksums.Length; i++)
            {
                if (FileChecksums[i].Algorithm.Equals(Algorithm))
                {
                    foundHash = FileChecksums[i];
                }
            }
            if (foundHash is null)
            {
                List<YorotFileChecksum> checksums = new List<YorotFileChecksum>();
                checksums.AddRange(FileChecksums);
                YorotFileChecksum checksum = new YorotFileChecksum(Algorithm, calculateHash ? HTAlt.Tools.BytesToString(Algorithm.ComputeHash(ContentStream)) : "", this);
                checksums.Add(checksum);
                FileChecksums = checksums.ToArray();
            }
            else
            {
                if (calculateHash || string.IsNullOrWhiteSpace(foundHash.Hash))
                {
                    foundHash.Hash = HTAlt.Tools.BytesToString(Algorithm.ComputeHash(ContentStream));
                }
            }
        }
    }

    public class YorotFileChecksum
    {
        public YorotFileChecksum(HashAlgorithm algorithm, string hash, YorotVirtualFile file)
        {
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        public System.Security.Cryptography.HashAlgorithm Algorithm { get; set; }
        public string Hash { get; set; }
        public YorotVirtualFile File { get; set; }

        public bool Verify()
        {
            return HTAlt.Tools.VerifyFile(Algorithm, File.ContentStream, Hash);
        }
    }

    public class YorotFilePermission
    {
        public string GroupName { get; set; }
        public int GroupID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExecute { get; set; }
    }
}