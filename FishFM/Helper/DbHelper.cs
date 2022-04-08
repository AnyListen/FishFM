using System.Collections.Generic;
using FishFM.Models;
using LiteDB;
using Newtonsoft.Json;

namespace FishFM.Helper;

public class DbHelper
{
    private const string FmSongTable = "t_fm_songs";
    private const string LikeTable = "t_liked_songs";

    public static bool UpsertSongs(List<SongResult> list, string date, string type)
    {
        var dbSongs = new List<DbSong>(list.Count);
        foreach (var songResult in list)
        {
            dbSongs.Add(songResult.ToDbSong(type, date));
        }
        using var db = new LiteDatabase("./MyFM.db");
        var col = db.GetCollection<DbSong>(FmSongTable);
        return col.Upsert(dbSongs) >= 0;
    }
    
    public static List<SongResult> GetSongs(string date, string type)
    {
        using var db = new LiteDatabase("./MyFM.db");
        var col = db.GetCollection<DbSong>(FmSongTable);
        var list = col.Query().Where(s => s.FmType == type && s.AddDate == date).ToList();
        List<SongResult> results = new List<SongResult>(list.Count);
        foreach (var dbSong in list)
        {
            var song = JsonConvert.DeserializeObject<SongResult>(dbSong.Text);
            if (song != null)
            {
                results.Add(song);
            }
        }
        return results;
    }

    public static bool LikeSong(SongResult songResult)
    {
        using var db = new LiteDatabase("./MyFM.db");
        var col = db.GetCollection<string>(LikeTable);
        return col.Upsert(songResult.ToDbSong("","").Id);
    }
    
    public static bool DislikeSong(SongResult songResult)
    {
        using var db = new LiteDatabase("./MyFM.db");
        var col = db.GetCollection<string>(LikeTable);
        return col.Delete(songResult.ToDbSong("","").Id);
    }
    
    public static List<string> GetAllLikedSong(SongResult songResult)
    {
        using var db = new LiteDatabase("./MyFM.db");
        var col = db.GetCollection<string>(LikeTable);
        return col.Query().ToList();
    }
}