using Newtonsoft.Json;

namespace Ditch.Steem.Models.Other
{
    /// <summary>
    /// market_volume
    /// libraries\plugins\market_history\include\steemit\market_history\market_history_api.hpp
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class MarketVolume
    {
        /// <summary>
        /// API name: steem_volume
        /// = asset( 0, STEEM_SYMBOL );
        /// </summary>
        /// <returns>API type: asset</returns>
        [JsonProperty("steem_volume")]
        public Asset SteemVolume {get; set;}

        /// <summary>
        /// API name: sbd_volume
        /// = asset( 0, SBD_SYMBOL );
        /// </summary>
        /// <returns>API type: asset</returns>
        [JsonProperty("sbd_volume")]
        public Asset SbdVolume {get; set;}
    }
}
