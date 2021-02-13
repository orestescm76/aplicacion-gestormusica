﻿using System;
using Newtonsoft.Json;

namespace aplicacion_musica
{
    public class Song
    {
        [JsonIgnore]
        public AlbumData album { get; protected set; }
        public string titulo { get; set; }
        [JsonConverter(typeof(TiempoConverter))]
        public TimeSpan duracion { get; set; }
        public bool Bonus { get; set; }
        [JsonIgnore]
        public String PATH { get; set; }
        public string[] Lyrics { get; set; }
        public int Num
        {
            get
            {
                return album.Songs.IndexOf(this)+1;
            }
            set
            {

            }
        }

        public Song()
        {
        }
        public Song(String titulo, int ms, bool Bonus)
        {
            this.titulo = titulo;
            duracion = new TimeSpan(0, 0, 0, 0, ms);
            this.Bonus = Bonus;
        }
        public Song(Song c)
        {
            titulo = c.titulo;
            album = c.album;
            duracion = c.duracion;
            Bonus = c.Bonus;
        }
        public Song(string t, TimeSpan d, ref AlbumData a)
        {
            titulo = t;
            duracion = d;
            album = a;
        }
        public Song(string t, TimeSpan d, ref AlbumData a, bool b)
        {
            titulo = t;
            duracion = d;
            album = a;
            Bonus = b;
        }
        public Song(string path) //Crea una canción fantasma con sólo un PATH
        {
            this.PATH = path;
        }
        public override string ToString()
        {
            if (!ReferenceEquals(album, null))
                return album.Artist + " - " + titulo + " (" + album.Title + ")";
            else
                return titulo;
        }
        public String[] ToStringArray()
        {
            String[] datos;
            if (duracion.TotalMinutes>=60)
                datos = new string[] { titulo, duracion.ToString(@"h\:mm\:ss") };
            else
                datos = new string[] { titulo, duracion.ToString(@"mm\:ss") };
            return datos;
        }
        public int GetMilisegundos()
        {
            return Convert.ToInt32(duracion.TotalMilliseconds);
        }
        public void SetAlbum(AlbumData a)
        {
            album = a;
        }
        //Tame Impala;The Less I Know The Better;Currents
        public String GuardarPATH()
        {
            return album.Artist+";"+titulo+";"+album.Title + Environment.NewLine+PATH + Environment.NewLine;
        }
    }
}
