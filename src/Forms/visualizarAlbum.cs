﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using aplicacion_musica.src.Forms;

namespace aplicacion_musica
{
    public partial class visualizarAlbum : Form
    {
        private AlbumData albumToVisualize;
        private byte numDisco;
        private DiscoCompacto CDaVisualizar;
        private ListViewItemComparer lvwColumnSorter;
        public visualizarAlbum(ref AlbumData a)
        {
            InitializeComponent();
            numDisco = 0;
            albumToVisualize = a;
            CDaVisualizar = null;

            try
            {
                if (!string.IsNullOrEmpty(a.CoverPath))
                {
                    Image caratula = Image.FromFile(a.CoverPath);
                    vistaCaratula.Image = caratula;
                    vistaCaratula.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            catch (FileNotFoundException)
            {
                Log.Instance.ImprimirMensaje("No se encuentra la carátula", TipoMensaje.Advertencia);
                vistaCaratula.Image = Properties.Resources.albumdesconocido;
            }
            lvwColumnSorter = new ListViewItemComparer();
            vistaCanciones.ListViewItemSorter = lvwColumnSorter;
            vistaCanciones.View = View.Details;
            vistaCanciones.MultiSelect = true;
            duracionSeleccionada.AutoSize = true;
            barraAbajo.Font = new Font("Segoe UI", 10);
            Controls.Add(barraAbajo);
            labelEstadoDisco.Hide();
            if(!(albumToVisualize is null) && string.IsNullOrEmpty(albumToVisualize.SoundFilesPath))
            {
                buttonAnotaciones.Enabled = false;
            }
            if (string.IsNullOrEmpty(albumToVisualize.SoundFilesPath))
                buttonPATH.Enabled = false;
            ponerTextos();
            cargarVista();
        }
        public visualizarAlbum(ref DiscoCompacto cd)
        {
            InitializeComponent();
            CDaVisualizar = cd;
            buttonPATH.Hide();
            albumToVisualize = cd.Album;
            numDisco = 1;
            infoAlbum.Text = Program.LocalTexts.GetString("artista") + ": " + cd.Album.Artist + Environment.NewLine +
                Program.LocalTexts.GetString("titulo") + ": " + cd.Album.Title + Environment.NewLine +
                Program.LocalTexts.GetString("año") + ": " + cd.Album.Year + Environment.NewLine +
                Program.LocalTexts.GetString("duracion") + ": " + cd.Album.Length.ToString() + Environment.NewLine +
                Program.LocalTexts.GetString("genero") + ": " + cd.Album.Genre.Name + Environment.NewLine +
                Program.LocalTexts.GetString("formato") + ": " + Program.LocalTexts.GetString(cd.FormatoCD.ToString()) + Environment.NewLine +
                Program.LocalTexts.GetString("añoPublicacion") + ": " + cd.Year + Environment.NewLine +
                Program.LocalTexts.GetString("paisPublicacion") + ":" + cd.Country + Environment.NewLine +
                Program.LocalTexts.GetString("estado_exterior") + ": " + Program.LocalTexts.GetString(cd.EstadoExterior.ToString()) + Environment.NewLine;
            labelEstadoDisco.Text = Program.LocalTexts.GetString("estado_medio") + " " + numDisco + ": " + Program.LocalTexts.GetString(cd.Discos[0].EstadoDisco.ToString()) + Environment.NewLine;
            if (!string.IsNullOrEmpty(cd.Album.CoverPath))
            {
                Image caratula = Image.FromFile(cd.Album.CoverPath);
                vistaCaratula.Image = caratula;
                vistaCaratula.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            lvwColumnSorter = new ListViewItemComparer();
            vistaCanciones.ListViewItemSorter = lvwColumnSorter;
            vistaCanciones.View = View.Details;
            vistaCanciones.MultiSelect = true;
            duracionSeleccionada.AutoSize = true;
            barraAbajo.Font = new Font("Segoe UI", 9);
            Controls.Add(barraAbajo);
            ponerTextos();
            cargarVista();
        }
        private void ponerTextos()
        {
            Text = Program.LocalTexts.GetString("visualizando") + " " + albumToVisualize.Artist + " - " + albumToVisualize.Title;
            vistaCanciones.Columns[0].Text = "#";
            vistaCanciones.Columns[1].Text = Program.LocalTexts.GetString("titulo");
            vistaCanciones.Columns[2].Text = Program.LocalTexts.GetString("duracion");
            okDoomerButton.Text = Program.LocalTexts.GetString("hecho");
            editarButton.Text = Program.LocalTexts.GetString("editar");
            duracionSeleccionada.Text = Program.LocalTexts.GetString("dur_total") + ": 00:00:00";
            if (!(CDaVisualizar is null))
                buttonAnotaciones.Text = Program.LocalTexts.GetString("editar_anotaciones");
            else
                buttonAnotaciones.Text = Program.LocalTexts.GetString("reproducir");
            setBonusToolStripMenuItem.Text = Program.LocalTexts.GetString("setBonus");
            reproducirToolStripMenuItem.Text = Program.LocalTexts.GetString("reproducir");
            reproducirspotifyToolStripMenuItem.Text = Program.LocalTexts.GetString("reproducirSpotify");
            buttonPATH.Text = Program.LocalTexts.GetString("calcularPATHS");
            if(Config.Idioma == "el") //Greek needs a little adjustment on the UI
            {
                Font but = buttonPATH.Font;
                Font neo = new Font(but.FontFamily, 7);
                buttonPATH.Font = neo;
            }
            verLyricsToolStripMenuItem.Text = Program.LocalTexts.GetString("verLyrics");
            fusionarToolStripMenuItem.Text = Program.LocalTexts.GetString("fusionarCancionPartes");
            defusionarToolStripMenuItem.Text = Program.LocalTexts.GetString("defusionarCancionPartes");
            copiarImagenStrip.Text = Program.LocalTexts.GetString("copiarImagen");
        }
        private void refrescarVista()
        {
            ponerTextos();
            vistaCanciones.Items.Clear();
            int i = 0;
            foreach (Song c in albumToVisualize.Songs)
            {
                String[] datos = new string[3];
                datos[0] = (i + 1).ToString();
                c.ToStringArray().CopyTo(datos, 1);
                ListViewItem item = new ListViewItem(datos);

                if (c is LongSong)
                {
                    item.BackColor = Color.LightSalmon;
                }
                if (c.IsBonus)
                {
                    item.BackColor = Color.SkyBlue;
                }
                vistaCanciones.Items.Add(item);
                i++;
            }

        }
        private void cargarVista()
        {
            vistaCanciones.Items.Clear();
            if (string.IsNullOrEmpty(albumToVisualize.IdSpotify) || Program._spotify is null || !Program._spotify.cuentaLista)
                reproducirspotifyToolStripMenuItem.Enabled = false;
            if (string.IsNullOrEmpty(albumToVisualize.SoundFilesPath))
                reproducirToolStripMenuItem.Enabled = false;
            ListViewItem[] items = new ListViewItem[albumToVisualize.Songs.Count];
            int i = 0, j = 0, d = 0;
            TimeSpan durBonus = new TimeSpan();
            if (!(CDaVisualizar is null) && CDaVisualizar.Discos.Length > 1)
            {
                ListViewGroup d1 = new ListViewGroup("Disco 1");
                ListViewGroup d2 = new ListViewGroup("Disco 2");
                vistaCanciones.Groups.Add(d1);
                vistaCanciones.Groups.Add(d2);
                vistaCanciones.ShowGroups = true;
                foreach (Song c in albumToVisualize.Songs)
                {
                    String[] datos = new string[3];
                    datos[0] = (j + 1).ToString();
                    c.ToStringArray().CopyTo(datos, 1);
                    items[i] = new ListViewItem(datos);
                    j++;
                    items[i].Group = vistaCanciones.Groups[d];
                    if (j >= CDaVisualizar.Discos[d].NumCanciones)
                    {
                        d++;
                        j = 0;
                    }
                    if (c is LongSong)
                    {
                        items[i].BackColor = Color.LightSalmon;
                    }
                    if (c.IsBonus)
                    {
                        items[i].BackColor = Color.SkyBlue;
                        durBonus += c.Length;
                    }
                    i++;
                }
                if (durBonus.TotalMilliseconds != 0)
                    infoAlbum.Text = Program.LocalTexts.GetString("artista") + ": " + albumToVisualize.Artist + Environment.NewLine +
                        Program.LocalTexts.GetString("titulo") + ": " + albumToVisualize.Title + Environment.NewLine +
                        Program.LocalTexts.GetString("año") + ": " + albumToVisualize.Year + Environment.NewLine +
                        Program.LocalTexts.GetString("duracion") + ": " + albumToVisualize.Length.ToString() + " (" + durBonus.ToString() + ")" + Environment.NewLine +
                        Program.LocalTexts.GetString("genero") + ": " + albumToVisualize.Genre.Name + Environment.NewLine +
                        Program.LocalTexts.GetString("estado_exterior") + ": " + Program.LocalTexts.GetString(CDaVisualizar.EstadoExterior.ToString()) + Environment.NewLine +
                        Program.LocalTexts.GetString("estado_medio") + ": " + Program.LocalTexts.GetString(CDaVisualizar.Discos[0].EstadoDisco.ToString()) + Environment.NewLine +
                        Program.LocalTexts.GetString("formato") + ": " + Program.LocalTexts.GetString(CDaVisualizar.FormatoCD.ToString()) + Environment.NewLine;
                vistaCanciones.Items.AddRange(items);
            }
            else if (!(CDaVisualizar is null))
            {
                foreach (Song c in albumToVisualize.Songs)
                {

                    String[] datos = new string[3];
                    datos[0] = (i + 1).ToString();
                    c.ToStringArray().CopyTo(datos, 1);
                    items[i] = new ListViewItem(datos);

                    if (c is LongSong)
                    {
                        items[i].BackColor = Color.LightSalmon;
                    }
                    if (c.IsBonus)
                    {
                        items[i].BackColor = Color.SkyBlue;
                        durBonus += c.Length;
                    }
                    i++;
                }
                if (durBonus.TotalMilliseconds != 0)
                    infoAlbum.Text = Program.LocalTexts.GetString("artista") + ": " + albumToVisualize.Artist + Environment.NewLine +
                        Program.LocalTexts.GetString("titulo") + ": " + albumToVisualize.Title + Environment.NewLine +
                        Program.LocalTexts.GetString("año") + ": " + albumToVisualize.Year + Environment.NewLine +
                        Program.LocalTexts.GetString("duracion") + ": " + albumToVisualize.Length.ToString() + " (" + durBonus.ToString() + ")" + Environment.NewLine +
                        Program.LocalTexts.GetString("genero") + ": " + albumToVisualize.Genre.Name + Environment.NewLine + 
                        Program.LocalTexts.GetString("estado_exterior") + ": " + Program.LocalTexts.GetString(CDaVisualizar.EstadoExterior.ToString()) + Environment.NewLine +
                        Program.LocalTexts.GetString("estado_medio") + ": " + Program.LocalTexts.GetString(CDaVisualizar.Discos[0].EstadoDisco.ToString()) + Environment.NewLine +
                        Program.LocalTexts.GetString("formato") + ": " + Program.LocalTexts.GetString(CDaVisualizar.FormatoCD.ToString()) + Environment.NewLine;
                vistaCanciones.Items.AddRange(items);
                
            }
            else
            {
                foreach (Song c in albumToVisualize.Songs)
                {

                    String[] datos = new string[3];
                    datos[0] = (i + 1).ToString();
                    c.ToStringArray().CopyTo(datos, 1);
                    items[i] = new ListViewItem(datos);

                    if (c is LongSong)
                    {
                        items[i].BackColor = Color.LightSalmon;
                    }
                    if (c.IsBonus)
                    {
                        items[i].BackColor = Color.SkyBlue;
                        durBonus += c.Length;
                    }
                    i++;
                }
                if (durBonus.TotalMilliseconds != 0)
                    infoAlbum.Text = Program.LocalTexts.GetString("artista") + ": " + albumToVisualize.Artist + Environment.NewLine +
                        Program.LocalTexts.GetString("titulo") + ": " + albumToVisualize.Title + Environment.NewLine +
                        Program.LocalTexts.GetString("año") + ": " + albumToVisualize.Year + Environment.NewLine +
                        Program.LocalTexts.GetString("duracion") + ": " + albumToVisualize.Length.ToString() + " (" + durBonus.ToString() + ")" + Environment.NewLine +
                        Program.LocalTexts.GetString("genero") + ": " + albumToVisualize.Genre.Name;
                else
                    infoAlbum.Text = Program.LocalTexts.GetString("artista") + ": " + albumToVisualize.Artist + Environment.NewLine +
                        Program.LocalTexts.GetString("titulo") + ": " + albumToVisualize.Title + Environment.NewLine +
                        Program.LocalTexts.GetString("año") + ": " + albumToVisualize.Year + Environment.NewLine +
                        Program.LocalTexts.GetString("duracion") + ": " + albumToVisualize.Length.ToString() + Environment.NewLine +
                        Program.LocalTexts.GetString("genero") + ": " + albumToVisualize.Genre.Name + Environment.NewLine +
                        Program.LocalTexts.GetString("localizacion") + ": " + albumToVisualize.SoundFilesPath + Environment.NewLine;
                vistaCanciones.Items.AddRange(items);
            }
        }
        private void ordenarColumnas(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.ColumnaAOrdenar) // Determine if clicked column is already the column that is being sorted.
            {
                if (lvwColumnSorter.Orden == SortOrder.Ascending)
                    lvwColumnSorter.Orden = SortOrder.Descending;
                else lvwColumnSorter.Orden = SortOrder.Ascending;

            }
            else if (e.Column != 2 && e.Column != 3)//si la columna es  la del año o la de la duracion, que lo ponga de mayor a menor.
            {
                lvwColumnSorter.ColumnaAOrdenar = e.Column;
                lvwColumnSorter.Orden = SortOrder.Ascending;

            }
            else
            {
                lvwColumnSorter.ColumnaAOrdenar = e.Column; // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.Orden = SortOrder.Descending;
            }
            vistaCanciones.Sort();
            vistaCanciones.Refresh();
        }

        private void okDoomerButton_Click(object sender, EventArgs e)
        {
            Dispose();
        }
        private void editarButton_Click(object sender, EventArgs e)
        {
            if(CDaVisualizar is null)
            {
                editarAlbum editor = new editarAlbum(ref albumToVisualize);
                editor.Show();
            }
            else
            {
                CrearCD editor = new CrearCD(ref CDaVisualizar, numDisco, true);
                editor.ShowDialog();
            }
            Close();
        }
        private void vistaCanciones_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            TimeSpan seleccion = new TimeSpan();
            foreach (ListViewItem cancion in vistaCanciones.SelectedItems)
            {
                if(!(CDaVisualizar is null) &&CDaVisualizar.Discos.Length > 1)
                {
                    Song can = albumToVisualize.GetSong(cancion.SubItems[1].Text);
                    seleccion += can.Length;
                }
                else
                {
                    int c = Convert.ToInt32(cancion.SubItems[0].Text); c--;
                    Song can = albumToVisualize.GetSong(c);
                    seleccion += can.Length;
                }
            }
            duracionSeleccionada.Text = Program.LocalTexts.GetString("dur_total") + ": " + seleccion.ToString();
        }

        private void vistaCanciones_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int n = Convert.ToInt32(vistaCanciones.SelectedItems[0].SubItems[0].Text);
            Song c = albumToVisualize.GetSong(n-1);
            if(c is LongSong cl)
            {
                string infoDetallada = "";
                for (int i = 0; i < cl.Parts.Count; i++)
                {
                    infoDetallada += Utils.ConvertToRomanNumeral(i + 1) + ". ";
                    infoDetallada += cl.Parts[i].Title + " - " + cl.Parts[i].Length;
                    infoDetallada += Environment.NewLine;
                }
                MessageBox.Show(infoDetallada);
            }
        }

        private void buttonAnotaciones_Click(object sender, EventArgs e)
        {
            if(!(CDaVisualizar is null))
            {
                Anotaciones anoForm = new Anotaciones(ref CDaVisualizar);
                anoForm.ShowDialog();
            }
            else
            {
                ListaReproduccion ls = new ListaReproduccion(albumToVisualize.ToString());
                foreach (Song cancion in albumToVisualize.Songs)
                {
                    ls.AgregarCancion(cancion);
                }
                Reproductor.Instancia.ReproducirLista(ls);
            }
        }

        private void labelEstadoDisco_Click(object sender, EventArgs e)
        {
            if (CDaVisualizar.Discos.Length == 1)
                return;
            else
            {
                switch (numDisco)
                {
                    case 1:
                        numDisco = 2;
                        labelEstadoDisco.Text = Program.LocalTexts.GetString("estado_medio") + " " + numDisco + ": " + Program.LocalTexts.GetString(CDaVisualizar.Discos[numDisco-1].EstadoDisco.ToString()) + Environment.NewLine;
                        break;
                    case 2:
                        numDisco = 1;
                        labelEstadoDisco.Text = Program.LocalTexts.GetString("estado_medio") + " " + numDisco + ": " + Program.LocalTexts.GetString(CDaVisualizar.Discos[numDisco - 1].EstadoDisco.ToString()) + Environment.NewLine;
                        break;
                    default:
                        break;
                }
            }
        }

        private void visualizarAlbum_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in vistaCanciones.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void setBonusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in vistaCanciones.SelectedItems)
            {
                Song c = albumToVisualize.Songs[Convert.ToInt32(item.SubItems[0].Text)-1];
                c.IsBonus = !c.IsBonus;
            }
            cargarVista();
        }

        private void vistaCanciones_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                clickDerechoConfig.Show(vistaCanciones, e.Location);
            }
        }

        private void infoAlbum_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(albumToVisualize.SoundFilesPath))
            {
                Process explorador = new Process();
                explorador.StartInfo.UseShellExecute = true;
                explorador.StartInfo.FileName = "explorer.exe";
                explorador.StartInfo.Arguments = albumToVisualize.SoundFilesPath;
                explorador.Start();
                Log.Instance.ImprimirMensaje("Abierto explorer con PID: " + explorador.Id, TipoMensaje.Info);
            }
        }

        private void reproducirspotifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(albumToVisualize.IdSpotify))
            {
                SpotifyAPI.Web.Models.ErrorResponse err = Program._spotify.ReproducirCancion(albumToVisualize.IdSpotify, vistaCanciones.SelectedItems[0].Index);
                if (err.Error != null && err.Error.Message != null)
                    MessageBox.Show(err.Error.Message);
            }
                
        }
        private void reproducirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Song cancionAReproducir = albumToVisualize.GetSong(vistaCanciones.SelectedItems[0].Index);
            if(cancionAReproducir is LongSong)
            {
                LongSong cl = (LongSong)cancionAReproducir;
                Reproductor.Instancia.ReproducirCancion(cl);
            }
                
            else
                Reproductor.Instancia.ReproducirCancion(cancionAReproducir);
        }

        private void vistaCanciones_ItemDrag(object sender, ItemDragEventArgs e)
        {
            Song cancion = albumToVisualize.GetSong(vistaCanciones.SelectedItems[0].Index);
            vistaCanciones.DoDragDrop(cancion, DragDropEffects.Copy);
        }

        private void buttonPATH_Click(object sender, EventArgs e)
        {
            Log.Instance.ImprimirMensaje("Buscando canciones para " + albumToVisualize.ToString(), TipoMensaje.Info);
            bool correcto = true;
            DirectoryInfo directorioCanciones = new DirectoryInfo(albumToVisualize.SoundFilesPath);
            foreach (FileInfo file in directorioCanciones.GetFiles())
            {
                string extension = Path.GetExtension(file.FullName);
                if (extension != ".ogg" && extension != ".mp3" && extension != ".flac")
                    continue;
                foreach (Song c in albumToVisualize.Songs)
                {
                    try
                    {
                        LectorMetadatos LM = new LectorMetadatos(file.FullName);
                        if (LM.Evaluable() && (c.Title.ToLower() == LM.Titulo.ToLower()) && (c.AlbumFrom.Artist.ToLower() == LM.Artista.ToLower()))
                        {
                            c.Path = file.FullName;
                            Log.Instance.ImprimirMensaje(c.Path + " leído correctamente", TipoMensaje.Correcto);
                            break;
                        }
                        else if (LM.Evaluable() && string.Equals(c.Title, LM.Titulo) && string.Equals(LM.Artista, c.AlbumFrom.Artist))
                        {
                            c.Path = file.FullName;
                            Log.Instance.ImprimirMensaje(c.Path + " leído correctamente", TipoMensaje.Correcto);
                            break;
                        }
                        else
                        {
                            if (file.FullName.ToLower().Contains(c.Title.ToLower()))
                            {
                                c.Path = file.FullName;
                                Log.Instance.ImprimirMensaje(c.Path + " leído correctamente", TipoMensaje.Correcto);
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        correcto = false;
                    }

                }
            }
            if (correcto)
                MessageBox.Show(Program.LocalTexts.GetString("pathsCorrectos"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                foreach (Song cancion in albumToVisualize.Songs)
                {
                    if (cancion.Path == null) //No se ha encontrado
                    {
                        Log.Instance.ImprimirMensaje("No se encontró la canción para " + cancion.Title + ".", TipoMensaje.Advertencia);
                    }
                }
                MessageBox.Show(Program.LocalTexts.GetString("pathsError"), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Program.SavePATHS();
        }

        private void verLyricsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Song cancion = albumToVisualize.GetSong(vistaCanciones.SelectedItems[0].Index);
            VisorLyrics VL = new VisorLyrics(cancion);
            VL.Show();
        }

        private void vistaCaratula_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                clickDerechoAlbum.Show(vistaCaratula, e.Location);
            }
        }

        private void copiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(vistaCaratula.Image);
            Log.Instance.ImprimirMensaje("Enviada imagen al portapapeles", TipoMensaje.Correcto);
        }
        private void clickDerechoConfig_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            defusionarToolStripMenuItem.Visible = true;
            fusionarToolStripMenuItem.Visible = true;
            int i = vistaCanciones.SelectedItems[0].Index;
            Song seleccion = albumToVisualize.GetSong(i);
            if (vistaCanciones.SelectedItems.Count > 1)
                defusionarToolStripMenuItem.Visible = false;
            if (!(seleccion is LongSong))
                defusionarToolStripMenuItem.Visible = false;
            else
                fusionarToolStripMenuItem.Visible = false;
        }
        private void fusionarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (vistaCanciones.SelectedItems.Count == 1)
            {
                MessageBox.Show(Program.LocalTexts.GetString("error_fusionsingular"));
                return;
            }
            int num = vistaCanciones.SelectedItems[0].Index;
            List<string> cancionesABorrar = new List<string>();
            LongSong cl = new LongSong();
            cl.SetAlbum(albumToVisualize);
            cl.Title = albumToVisualize.GetSong(num).Title;

            foreach (ListViewItem cancionItem in vistaCanciones.SelectedItems)
            {
                cl.AddPart(albumToVisualize.GetSong(cancionItem.Index));
                cancionesABorrar.Add(cancionItem.SubItems[1].Text);
            }

            foreach (string songTitle in cancionesABorrar)
                albumToVisualize.RemoveSong(songTitle);

            albumToVisualize.AddSong(cl, num); //IT works...

            refrescarVista();
        }

        private void defusionarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem item = vistaCanciones.SelectedItems[0];

            int num = item.Index;
            if (!(albumToVisualize.Songs[num] is LongSong))
            {
                MessageBox.Show(Program.LocalTexts.GetString("error_defusion"));
                return;
            }

            LongSong longSong = (LongSong)albumToVisualize.GetSong(num);
            foreach (Song parte in longSong.Parts)
            {
                albumToVisualize.AddSong(parte, num);
                num++;
            }

            longSong.Title = "---"; //This is for safe defusing

            albumToVisualize.RemoveSong(longSong.Title);
            refrescarVista();
        }
    }
}
