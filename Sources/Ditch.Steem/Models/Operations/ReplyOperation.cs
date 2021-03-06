﻿using Ditch.Core.Helpers;
using Newtonsoft.Json;

namespace Ditch.Steem.Models.Operations
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReplyOperation : CommentOperation
    {
        public ReplyOperation(string parentAuthor, string parentPermlink, string author, string body, string jsonMetadata)
            : base(parentAuthor, parentPermlink, author, OperationHelper.CreateReplyPermlink(author, parentAuthor, parentPermlink), string.Empty, body, jsonMetadata) { }
    }
}
