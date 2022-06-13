using System.Collections.Generic;
using Newtonsoft.Json;

namespace FishFM.Models
{
    
public class SongResult
{
    /**
    * 歌曲ID
    **/
    public string Id { get; set; }
    /**
    * 曲名
    **/
    public string Name{ get; set; }

    public List<ArtistInfo> ArtistInfo { get; set; }

    public AlbumInfo AlbumInfo { get; set; }

    /**
    * 歌曲别名
    **/
    public string SubName{ get; set; }
    
    /**
    * 时长
    **/
    public int Length{ get; set; }

    public string SongLength{ get; set; }

    /**
    * 比特率
    **/
    public string BitRate{ get; set; }
    /**
    * Flac无损地址
    **/
    public string FlacUrl{ get; set; }
    /**
    * Ape无损地址
    **/
    public string ApeUrl{ get; set; }
    /**
    * Wav地址
    **/
    public string WavUrl{ get; set; }
    /**
    * 320K
    **/
    public string SqUrl{ get; set; }
    /**
    * 192K
    **/
    public string HqUrl{ get; set; }
    /**
    * 128K
    **/
    public string LqUrl{ get; set; }
    /**
    * 复制链接
    **/
    public string CopyUrl{ get; set; }
    /**
    * 歌曲小封面120*120
    **/
    public string SmallPic{ get; set; }
    /**
    * 歌曲封面
    **/
    public string PicUrl{ get; set; }
    /**
    * LRC歌词
    **/
    public string LrcUrl{ get; set; }
    /**
    * TRC歌词
    **/
    public string TrcUrl{ get; set; }
    /**
    * KRC歌词
    **/
    public string KrcUrl{ get; set; }
    /**
    * MV Id
    **/
    public string MvId{ get; set; }
    /**
    * 高清MV地址
    **/
    ///
    public string MvHdUrl{ get; set; }
    /**
    * 普清MV地址
    **/
    public string MvLdUrl{ get; set; }
    /**
    * 语种
    **/
    public string Language{ get; set; }
    /**
    * 发行公司
    **/
    public string Company{ get; set; }
    /**
    * 歌曲发行日期
    **/
    public string Year{ get; set; }
    /**
    * 碟片
    **/
    public int Disc{ get; set; }
    /**
    * 曲目编号
    **/
    public int TrackNum{ get; set; }
    /**
    * 类型
    **/
    public string Type{ get; set; }

    public DbSong ToDbSong(string fmType, string date)
    {
        if (string.IsNullOrEmpty(Type))
        {
            Type = PicUrl.Contains("/wy_") ? "wy" : "qq";
        }
        return new DbSong
        {
            Id = Type + "#" + Id,
            FmType = fmType,
            AddDate = date,
            LocalPath = "",
            Text = JsonConvert.SerializeObject(this, Formatting.None)
        };
    }
}
}
