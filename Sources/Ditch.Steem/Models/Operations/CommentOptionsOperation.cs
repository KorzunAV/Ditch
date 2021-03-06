﻿using System;
using Ditch.Core.Attributes;
using Ditch.Steem.Models.Other;
using Newtonsoft.Json;

namespace Ditch.Steem.Models.Operations
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CommentOptionsOperation : BaseOperation
    {
        public override OperationType Type => OperationType.CommentOptions;
        public override string TypeName => "comment_options";

        [MessageOrder(20)]
        [JsonProperty("author")]
        public string Author { get; set; }

        [MessageOrder(30)]
        [JsonProperty("permlink")]
        public string Permlink { get; set; }

        [JsonProperty("max_accepted_payout")]
        public string MaxAcceptedPayoutStr { get; set; }
        [MessageOrder(40)]
        public Asset MaxAcceptedPayout
        {
            get => MaxAcceptedPayoutStr;
            set => MaxAcceptedPayoutStr = value;
        }

        [MessageOrder(50)]
        [JsonProperty("percent_steem_dollars")]
        public UInt16 PercentSteemDollars { get; set; }

        [MessageOrder(60)]
        [JsonProperty("allow_votes")]
        public bool AllowVotes { get; set; }

        [MessageOrder(70)]
        [JsonProperty("allow_curation_rewards")]
        public bool AllowCurationRewards { get; set; }

        [MessageOrder(80)]
        [JsonProperty("extensions")]
        public object[] Extensions { get; set; }

        public CommentOptionsOperation(string author, string permlink, Asset maxAcceptedPayout, UInt16 percentSteemDollars, bool allowVotes, bool allowCurationRewards, params object[] extensions)
        {
            Author = author;
            Permlink = permlink;
            MaxAcceptedPayout = maxAcceptedPayout;
            PercentSteemDollars = percentSteemDollars;
            AllowVotes = allowVotes;
            AllowCurationRewards = allowCurationRewards;
            Extensions = extensions;
        }
    }
}