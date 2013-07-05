using System;
using System.Xml.Serialization;

namespace FileManager
{
    class File
    {
        #region Variables

        private string _FileName, _FilePath, _FileExtension, _Title, _Album;

        private uint _Year;

        private string[] _Artists;

        private long _FileLength;

        private DateTime _FileDateCreation, _FileDateModification;

        private Folder _FileParentFolder;

        #endregion

        #region Constructeurs

        public File(string FileName, string FilePath, long FileLength, DateTime FileDateCreation, DateTime FileDateModification, string FileExtension, Folder FileParentFolder, string Title, string Album, uint Year, string[] Artists)
        {
            _FileName = FileName;
            _FilePath = FilePath;
            _FileLength = FileLength;
            _FileDateCreation = FileDateCreation;
            _FileDateModification = FileDateModification;
            _FileExtension = FileExtension;
            _FileParentFolder = FileParentFolder;
            _Title = Title;
            _Album = Album;
            _Year = Year;
            _Artists = Artists;
        }

        #endregion

        #region Méthodes

        #region Getters/Setters

        [XmlAttribute("FileName")]
        public string FileName
        { get { return this._FileName; } set { this._FileName = value; } }

        [XmlElement("FilePath")]
        public string FilePath
        { get { return this._FilePath; } set { this._FilePath = value; } }

        [XmlElement("FileLength")]
        public long FileLength
        { get { return this._FileLength; } set { this._FileLength = value; } }

        [XmlElement("FileDateCreation")]
        public DateTime FileDateCreation
        { get { return this._FileDateCreation; } set { this._FileDateCreation = value; } }

        [XmlElement("FileDateModification")]
        public DateTime FileDateModification
        { get { return this._FileDateModification; } set { this._FileDateModification = value; } }

        [XmlElement("FileExtension")]
        public string FileExtension
        { get { return this._FileExtension; } set { this._FileExtension = value; } }

        [XmlElement("FileParentFolder")]
        public Folder FileParentFolder
        { get { return this._FileParentFolder; } set { this._FileParentFolder = value; } }

        [XmlElement("MediaTitle")]
        public string Title
        { get { return this._Title; } set { this._Title = value; } }

        [XmlElement("MediaAlbum")]
        public string Album
        { get { return this._Album; } set { this._Album = value; } }

        [XmlElement("MediaYear")]
        public uint Year
        { get { return this._Year; } set { this._Year = value; } }

        [XmlElement("MediaArtists")]
        public string[] Artists
        { get { return this._Artists; } set { this._Artists = value; } }

        #endregion

        #endregion
    }
}