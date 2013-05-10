using System;

namespace FileManager
{
    class File
    {
        #region Variables

        private string _FileName, _FilePath, _FileExtension, _Title, _Album, _Year;

        private string[] _Artists;

        private long _FileLength;

        private DateTime _FileDateCreation, _FileDateModification;

        private Folder _FileParentFolder;

        #endregion

        #region Constructeurs

        public File(string FileName, string FilePath, long FileLength, DateTime FileDateCreation, DateTime FileDateModification, string FileExtension, Folder FileParentFolder, string Title, string Album, string Year, string[] Artists)
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

        public File()
        { }

        #endregion

        #region Méthodes

        #region Getters/Setters

        public string FileName
        { get { return this._FileName; } set { this._FileName = value; } }

        public string FilePath
        { get { return this._FilePath; } set { this._FilePath = value; } }

        public long FileLength
        { get { return this._FileLength; } set { this._FileLength = value; } }

        public DateTime FileDateCreation
        { get { return this._FileDateCreation; } set { this._FileDateCreation = value; } }

        public DateTime FileDateModification
        { get { return this._FileDateModification; } set { this._FileDateModification = value; } }

        public string FileExtension
        { get { return this._FileExtension; } set { this._FileExtension = value; } }

        public Folder FileParentFolder
        { get { return this._FileParentFolder; } set { this._FileParentFolder = value; } }

        public string Title
        { get { return this._Title; } set { this._Title = value; } }

        public string Album
        { get { return this._Album; } set { this._Album = value; } }

        public string Year
        { get { return this._Year; } set { this._Year = value; } }

        public string[] Artists
        { get { return this._Artists; } set { this._Artists = value; } }

        #endregion

        #endregion
    }
}