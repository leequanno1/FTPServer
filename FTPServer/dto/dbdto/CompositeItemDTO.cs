using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.dbdto
{
    internal class CompositeItemDTO
    {
        private string _itemId;
        private string _itemPath;
        private string _parentPath;
        private string _itemName;
        private string _userId;
        private string _itemType;

        public CompositeItemDTO(CompositeItem model)
        {
            ItemId = model.ItemId;
            ItemPath = model.ItemPath;
            ParentPath = model.ParentPath;
            ItemName = model.ItemName;
            UserId = model.UserId;
            ItemType = model.ItemType;
        }

        public string ItemId { get => _itemId; set => _itemId = value; }
        public string ItemPath { get => _itemPath; set => _itemPath = value; }
        public string ParentPath { get => _parentPath; set => _parentPath = value; }
        public string ItemName { get => _itemName; set => _itemName = value; }
        public string UserId { get => _userId; set => _userId = value; }
        public string ItemType { get => _itemType; set => _itemType = value; }
    }
}
