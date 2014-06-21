using System;
using System.Collections;

namespace FileManager
{
    [Serializable]
    public class XmlFileInfos : CollectionBase
    {
        #region Constructor

        public XmlFileInfos()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets/Sets value for the item by that index
        /// </summary>
        public XmlFileInfo this[int index]
        {
            get
            {
                return (XmlFileInfo)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        #endregion

        #region Public Methods

        public int IndexOf(XmlFileInfo XmlFileInfoItem)
        {
            if (XmlFileInfoItem != null)
            {
                return base.List.IndexOf(XmlFileInfoItem);
            }
            return -1;
        }

        public int Add(XmlFileInfo XmlFileInfoItem)
        {
            if (XmlFileInfoItem != null)
            {
                return this.List.Add(XmlFileInfoItem);
            }
            return -1;
        }

        public void Remove(XmlFileInfo XmlFileInfoItem)
        {
            this.InnerList.Remove(XmlFileInfoItem);
        }

        public void AddRange(XmlFileInfos collection)
        {
            if (collection != null)
            {
                this.InnerList.AddRange(collection);
            }
        }

        public void Insert(int index, XmlFileInfo XmlFileInfoItem)
        {
            if (index <= List.Count && XmlFileInfoItem != null)
            {
                this.List.Insert(index, XmlFileInfoItem);
            }
        }

        public bool Contains(XmlFileInfo XmlFileInfoItem)
        {
            return this.List.Contains(XmlFileInfoItem);
        }

        #endregion
    }
}