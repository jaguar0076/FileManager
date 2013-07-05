using System;
using System.Xml.Serialization;

namespace FileManager
{
    public class Folder
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

        #endregion

        #region Méthodes

        #region Getters/Setters

        [XmlAttribute("FolderName")]
        public string FolderName
        { get { return this._FolderName; } set { this._FolderName = value; } }

        [XmlElement("FolderPath")]
        public string FolderPath
        { get { return this._FolderPath; } set { this._FolderPath = value; } }

        [XmlElement("FolderLength")]
        public long FolderLength
        { get { return this._FolderLength; } set { this._FolderLength = value; } }

        [XmlElement("FolderDateCreation")]
        public DateTime FolderDateCreation
        { get { return this._FolderDateCreation; } set { this._FolderDateCreation = value; } }

        [XmlElement("FolderDateModification")]
        public DateTime FolderDateModification
        { get { return this._FolderDateModification; } set { this._FolderDateModification = value; } }

        [XmlElement("FolderParent")]
        public Folder FolderParent
        { get { return this._FolderParent; } set { this._FolderParent = value; } }

        #endregion

        #endregion
    }
}