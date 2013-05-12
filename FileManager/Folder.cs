using System;

namespace FileManager
{
    class Folder
    {
        #region Variables

        private string _FolderName, _FolderPath;

        private long _FolderLength;

        private DateTime _FolderDateCreation, _FolderDateModification;

        private Folder _FolderParent;

        #endregion

        #region Constructeurs

        public Folder(string FolderName, string FolderPath, long FolderLength, DateTime FolderDateCreation, DateTime FolderDateModification, Folder FolderParent)
        {
            _FolderName = FolderName;
            _FolderPath = FolderPath;
            _FolderLength = FolderLength;
            _FolderDateCreation = FolderDateCreation;
            _FolderDateModification = FolderDateModification;
            _FolderParent = FolderParent;
        }

        public Folder()
        { }

        #endregion

        #region Méthodes


        #region Getters/Setters

        public string FolderName
        { get { return this._FolderName; } set { this._FolderName = value; } }

        public string FolderPath
        { get { return this._FolderPath; } set { this._FolderPath = value; } }

        public long FolderLength
        { get { return this._FolderLength; } set { this._FolderLength = value; } }

        public DateTime FolderDateCreation
        { get { return this._FolderDateCreation; } set { this._FolderDateCreation = value; } }

        public DateTime FolderDateModification
        { get { return this._FolderDateModification; } set { this._FolderDateModification = value; } }

        public Folder FolderParent
        { get { return this._FolderParent; } set { this._FolderParent = value; } }

        #endregion


        #endregion
    }
}