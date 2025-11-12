using Calcpad.WebApi.Models.Base;
using MongoDB.Bson;

namespace Calcpad.WebApi.Models
{
    public enum CpdFileStatus
    {
        /// <summary>
        /// 已删除
        /// </summary>
        Deleted,

        /// <summary>
        /// 正常
        /// </summary>
        Normal
    }


    public class CalcpadFileModel : MongoDoc
    {
        public CpdFileStatus Status { get; set; } = CpdFileStatus.Normal;

        /// <summary>
        /// uploader user id
        /// </summary>
        public ObjectId UserId { get; set; }

        /// <summary>
        /// file fullName for calcpad include path witch prefix is started with %CALCPAD_STORAGEROOT%
        /// calcpad only support full path or relative path witch relative to the current file
        /// fullNmae = %CALCPAD_STORAGEROOT%/ObjectName
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// filePath with extension in server storage relative to storage root
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// fileName for human, no dir
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// unique id for managing the file
        /// </summary>
        
        public string UniqueId { get; set; }

        /// <summary>
        /// if true, the file can be accessed without authentication
        /// </summary>
        public bool IsCpd { get; set; }

        /// <summary>
        /// include file list
        /// </summary>
        public List<string> IncludeUniqueIds { get; set; } = [];

        /// <summary>
        /// copy from Id
        /// </summary>
        public ObjectId SourceId { get; set; }

        /// <summary>
        /// last update date
        /// </summary>
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;
    }
}
