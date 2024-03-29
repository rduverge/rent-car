﻿using System;
using RentaCarroFinal.Models;
using RentaCarroFinal.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using RJCodeAdvance.RJControls;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentaCarroFinal.UI
{
    public partial class FrmVehiculo : Form
    {

        internal VehiculoRepo vehiculoRepo = new VehiculoRepo();
        internal MarcaRepo marcaRepo = new MarcaRepo();
        internal ModeloRepo modeloRepo = new ModeloRepo();
        internal TipoCombustibleRepo tipoCombustibleRepo = new TipoCombustibleRepo();
        internal TipoVehiculoRepo tipoVehiculoRepo = new TipoVehiculoRepo();
        readonly Vehiculo vehiculo = new Vehiculo();
        private int vehiculoId;
        readonly VehiculoRepo vehiculoRepository = new VehiculoRepo();
        List<string> errores = new List<string>();
        public FrmTiposCombustibles FrmTiposCombustibles;
        public FrmMarca FrmMarca;
        public FrmModelo FrmModelo;

        //Constructor
        public FrmVehiculo()
        {
            InitializeComponent();
            CollapseMenu();
            this.Padding = new Padding(borderSize);//Border size
            this.BackColor = Color.FromArgb(98, 102, 244);//Border color
        }

        //Fields
        private int borderSize = 2;
        private Size formSize;


        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        //Overridden methods
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;//Standar Title Bar - Snap Window
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020; //Minimize form (Before)
            const int SC_RESTORE = 0xF120; //Restore form (Before)
            const int WM_NCHITTEST = 0x0084;//Win32, Mouse Input Notification: Determine what part of the window corresponds to a point, allows to resize the form.
            const int resizeAreaSize = 10;
            #region Form Resize
            // Resize/WM_NCHITTEST values
            const int HTCLIENT = 1; //Represents the client area of the window
            const int HTLEFT = 10;  //Left border of a window, allows resize horizontally to the left
            const int HTRIGHT = 11; //Right border of a window, allows resize horizontally to the right
            const int HTTOP = 12;   //Upper-horizontal border of a window, allows resize vertically up
            const int HTTOPLEFT = 13;//Upper-left corner of a window border, allows resize diagonally to the left
            const int HTTOPRIGHT = 14;//Upper-right corner of a window border, allows resize diagonally to the right
            const int HTBOTTOM = 15; //Lower-horizontal border of a window, allows resize vertically down
            const int HTBOTTOMLEFT = 16;//Lower-left corner of a window border, allows resize diagonally to the left
            const int HTBOTTOMRIGHT = 17;//Lower-right corner of a window border, allows resize diagonally to the right
            ///<Doc> More Information: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest </Doc>
            if (m.Msg == WM_NCHITTEST)
            { //If the windows m is WM_NCHITTEST
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal)//Resize the form if it is in normal state
                {
                    if ((int)m.Result == HTCLIENT)//If the result of the m (mouse pointer) is in the client area of the window
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32()); //Gets screen point coordinates(X and Y coordinate of the pointer)                           
                        Point clientPoint = this.PointToClient(screenPoint); //Computes the location of the screen point into client coordinates                          
                        if (clientPoint.Y <= resizeAreaSize)//If the pointer is at the top of the form (within the resize area- X coordinate)
                        {
                            if (clientPoint.X <= resizeAreaSize) //If the pointer is at the coordinate X=0 or less than the resizing area(X=10) in 
                                m.Result = (IntPtr)HTTOPLEFT; //Resize diagonally to the left
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize))//If the pointer is at the coordinate X=11 or less than the width of the form(X=Form.Width-resizeArea)
                                m.Result = (IntPtr)HTTOP; //Resize vertically up
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (clientPoint.Y <= (this.Size.Height - resizeAreaSize)) //If the pointer is inside the form at the Y coordinate(discounting the resize area size)
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize horizontally to the left
                                m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X > (this.Width - resizeAreaSize))//Resize horizontally to the right
                                m.Result = (IntPtr)HTRIGHT;
                        }
                        else
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize diagonally to the left
                                m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize)) //Resize vertically down
                                m.Result = (IntPtr)HTBOTTOM;
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                    }
                }
                return;
            }
            #endregion
            //Remove border and keep snap window
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }
            //Keep form size when it is minimized and restored. Since the form is resized because it takes into account the size of the title bar and borders.
            if (m.Msg == WM_SYSCOMMAND)
            {
                /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
                /// Quote:
                /// In WM_SYSCOMMAND messages, the four low - order bits of the wParam parameter 
                /// are used internally by the system.To obtain the correct result when testing 
                /// the value of wParam, an application must combine the value 0xFFF0 with the 
                /// wParam value by using the bitwise AND operator.
                int wParam = (m.WParam.ToInt32() & 0xFFF0);
                if (wParam == SC_MINIMIZE)  //Before
                    formSize = this.ClientSize;
                if (wParam == SC_RESTORE)// Restored form(Before)
                    this.Size = formSize;
            }
            base.WndProc(ref m);
        }
        //Private methods
        private void AdjustForm()
        {
            switch (this.WindowState)
            {
                case FormWindowState.Maximized: //Maximized form (After)
                    this.Padding = new Padding(8, 8, 8, 0);
                    break;
                case FormWindowState.Normal: //Restored form (After)
                    if (this.Padding.Top != borderSize)
                        this.Padding = new Padding(borderSize);
                    break;
            }
        }
        private void CollapseMenu()
        {
            if (this.panelMenu.Width > 200) //Collapse menu
            {
                panelMenu.Width = 100;
                pictureBox1.Visible = false;
                btnMenu.Dock = DockStyle.Top;
                foreach (Button menuButton in panelMenu.Controls.OfType<Button>())
                {
                    menuButton.Text = "";
                    menuButton.ImageAlign = ContentAlignment.MiddleCenter;
                    menuButton.Padding = new Padding(0);
                }
            }
            else
            { //Expand menu
                panelMenu.Width = 230;
                pictureBox1.Visible = true;
                btnMenu.Dock = DockStyle.None;
                foreach (Button menuButton in panelMenu.Controls.OfType<Button>())
                {
                    menuButton.Text = "   " + menuButton.Tag.ToString();
                    menuButton.ImageAlign = ContentAlignment.MiddleLeft;
                    menuButton.Padding = new Padding(10, 0, 0, 0);
                }
            }
        }
        //Event methods
        private void FrmMenuModern_Resize(object sender, EventArgs e)
        {
            AdjustForm();
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                formSize = this.ClientSize;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.Size = formSize;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            CollapseMenu();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void rjDropdownMenu2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void Open_DropdownMenu(RJDropdownMenu dropdownMenu, object sender)
        {
            Control control = (Control)sender;
            dropdownMenu.VisibleChanged += new EventHandler((sender2, ev)
              => DropdownMenu_VisibleChanged(sender2, ev, control));
            dropdownMenu.Show(control, control.Width, 0);
        }

        private void DropdownMenu_VisibleChanged(object sender, EventArgs e, Control ctrl)
        {
            RJDropdownMenu dropdownMenu2 = (RJDropdownMenu)sender;
            if (!DesignMode)
            {
                if (dropdownMenu2.Visible)
                    ctrl.BackColor = Color.FromArgb(159, 161, 224);
                else ctrl.BackColor = Color.FromArgb(98, 102, 244);
            }
        }


        private void iconButton2_Click(object sender, EventArgs e)
        {
            Open_DropdownMenu(rjDropdownMenu2, sender);
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            Open_DropdownMenu(rjDropdownMenu1, sender);
        }

        private void iconButton10_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tiposDeCombustibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FrmTiposCombustibles == null || FrmTiposCombustibles.IsDisposed)
            {
                FrmTiposCombustibles = new FrmTiposCombustibles();
                FrmTiposCombustibles.Show();
            }
            else
            {
                FrmTiposCombustibles.Show();
                FrmTiposCombustibles.Focus();
            }
        }

        private void marcasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FrmMarca == null || FrmMarca.IsDisposed)
            {
                FrmMarca = new FrmMarca();
                FrmMarca.Show();
            }
            else
            {
                FrmMarca.Show();
                FrmMarca.Focus();
            }
        }

        private void modelosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FrmModelo == null || FrmModelo.IsDisposed)
            {
                FrmModelo = new FrmModelo();
                FrmModelo.LoadData();
                FrmModelo.Show();
            }
            else
            {
                FrmModelo.LoadData();
                FrmModelo.Show();
                FrmModelo.Focus();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private Vehiculo GetVehiculo()
        {
            vehiculo.Descripcion = descripcionText.Text.Trim();
            vehiculo.NoChasis = textChasis.Text.Trim();
            vehiculo.NoMotor = textBox2.Text.Trim();
            vehiculo.NoPlaca = textBox3.Text.Trim();
            vehiculo.Estado = estadoCheck.Checked;
            using RentaCarroFinalContext db = new RentaCarroFinalContext();
            var marca = db.Marcas.Where(y => y.Descripcion == comboBox2.Text).FirstOrDefault();
            if (marca != null)
            {
                vehiculo.MarcaId = marca.Id;
            }
            else
            {
                vehiculo.MarcaId = null;
                vehiculo.Marca = null;
            }
            var modelo = db.Modelos.Where(y => y.Descripcion == comboBox3.Text).FirstOrDefault();
            if (modelo != null)
            {
                vehiculo.ModeloId = modelo.Id;
            }
            else
            {
                vehiculo.ModeloId = null;
                vehiculo.Modelo = null;
            }
            var tipoVehic = db.TiposVehiculo.Where(y => y.Descripcion == comboBox1.Text).FirstOrDefault();
            if (tipoVehic != null)
            {
                vehiculo.TipoVehiculoId = tipoVehic.Id;
            }
            else
            {
                vehiculo.TipoVehiculoId = null;
                vehiculo.TipoVehiculo = null;



            }
            var tipoComb = db.TiposCombustible.Where(y => y.Descripcion == comboBox4.Text).FirstOrDefault();
            if (tipoComb != null)
            {
                vehiculo.TipoCombustibleId = tipoComb.Id;
            }
            else
            {
                vehiculo.TipoCombustibleId = null;
                vehiculo.TipoCombustible = null;



            }
            return vehiculo;
        }
        private void Clear()
        {
            descripcionText.Text = "";
            textChasis.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            estadoCheck.Checked = false;
        }

        public void LoadData()
        {
            dataGridView1.DataSource = vehiculoRepository.View();
            comboBox2.DataSource = marcaRepo.View();
            comboBox3.DataSource = modeloRepo.View();
            comboBox1.DataSource = tipoVehiculoRepo.View();
            comboBox4.DataSource = tipoCombustibleRepo.View();

            dataGridView1.ClearSelection();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            vehiculo.Id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
            vehiculo.Descripcion = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            vehiculo.NoChasis = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            vehiculo.NoPlaca = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            vehiculo.NoMotor = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            using RentaCarroFinalContext db = new RentaCarroFinalContext();
            var marca = db.Marcas.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[7].Value.ToString())).FirstOrDefault();


            if (marca != null)
            {
                vehiculo.MarcaId = marca.Id;
                vehiculo.Marca = marca;
            }
            else
            {
                vehiculo.MarcaId = null;
                vehiculo.Marca = null;
            }

            var modelo = db.Modelos.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[9].Value.ToString())).FirstOrDefault();
            if (modelo != null)
            {
                vehiculo.ModeloId = modelo.Id;
                vehiculo.Modelo = modelo;
            }
            else
            {
                vehiculo.ModeloId = null;
                vehiculo.Modelo = null;
            }

            var tipoComb = db.TiposCombustible.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[11].Value.ToString())).FirstOrDefault();
            if (tipoComb != null)
            {
                vehiculo.TipoCombustibleId = tipoComb.Id;
                vehiculo.TipoCombustible = tipoComb;
            }
            else
            {
                vehiculo.TipoCombustibleId = null;
                vehiculo.TipoCombustible = null;

            }
            var tipoVehic = db.TiposVehiculo.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[5].Value.ToString())).FirstOrDefault();
            if (tipoVehic != null)
            {
                vehiculo.TipoVehiculoId = tipoVehic.Id;
                vehiculo.TipoVehiculo = tipoVehic;
            }
            else
            {
                vehiculo.TipoVehiculoId = null;
                vehiculo.TipoVehiculo = null;



            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            vehiculo.Id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
            vehiculo.Descripcion = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            vehiculo.NoChasis = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            vehiculo.NoPlaca = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            vehiculo.NoMotor = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            using RentaCarroFinalContext db = new RentaCarroFinalContext();
            var marca = db.Marcas.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[7].Value.ToString())).FirstOrDefault();


            if (marca != null)
            {
                vehiculo.MarcaId = marca.Id;
                vehiculo.Marca = marca;
            }
            else
            {
                vehiculo.MarcaId = null;
                vehiculo.Marca = null;
            }

            var modelo = db.Modelos.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[9].Value.ToString())).FirstOrDefault();
            if (modelo != null)
            {
                vehiculo.ModeloId = modelo.Id;
                vehiculo.Modelo = modelo;
            }
            else
            {
                vehiculo.ModeloId = null;
                vehiculo.Modelo = null;
            }

            var tipoComb = db.TiposCombustible.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[11].Value.ToString())).FirstOrDefault();
            if (tipoComb != null)
            {
                vehiculo.TipoCombustibleId = tipoComb.Id;
                vehiculo.TipoCombustible = tipoComb;
            }
            else
            {
                vehiculo.TipoCombustibleId = null;
                vehiculo.TipoCombustible = null;

            }
            var tipoVehi = db.TiposVehiculo.Where(y => y.Id == Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[5].Value.ToString())).FirstOrDefault();
            if (tipoVehi != null)
            {
                vehiculo.TipoVehiculoId = tipoVehi.Id;
                vehiculo.TipoVehiculo = tipoVehi;
            }
            else
            {
                vehiculo.TipoVehiculoId = null;
                vehiculo.TipoVehiculo = null;



            }

            vehiculo.Estado = Convert.ToBoolean(dataGridView1.SelectedRows[0].Cells[13].Value.ToString());
            descripcionText.Text = vehiculo.Descripcion;
            textChasis.Text = vehiculo.NoChasis;
            textBox2.Text = vehiculo.NoMotor;
            textBox3.Text = vehiculo.NoPlaca;

            estadoCheck.Checked = vehiculo.Estado;

        }

        private void guardarBtn_Click(object sender, EventArgs e)
        {
            vehiculo.Id = null;

            if (Validar())
            {


                vehiculoRepository.Create(GetVehiculo());
                LoadData();
                Clear();
            }
        }

        private void borrarBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var m = GetVehiculo();
                if (m != null)
                {
                    vehiculoRepository.Delete(m);
                    LoadData();
                    Clear();
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                MessageBox.Show("Este vehiculo no puede ser borrado...");

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void actualizarBtn_Click(object sender, EventArgs e)
        {
            if (Validar())
            {
                vehiculoRepository.Update(GetVehiculo());
                LoadData();
                Clear();
            }
        }


        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            borrarBtn.Enabled = dataGridView1.SelectedRows.Count > 0;
        }

        private void FrmVehiculo_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        public bool Validar()
        {
            errores.Clear();
            if (string.IsNullOrWhiteSpace(descripcionText.Text.Trim()))
            {
                errores.Add("Descripcion no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(textBox2.Text.Trim()))
            {
                errores.Add("No. de motor no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(textChasis.Text.Trim()))
            {
                errores.Add("No. de chasis no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(textBox3.Text.Trim()))
            {
                errores.Add("No. de placa no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(comboBox1.Text.Trim()))
            {
                errores.Add("Tipo de vehiculo no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(comboBox2.Text.Trim()))
            {
                errores.Add("Marca no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(comboBox3.Text.Trim()))
            {
                errores.Add("Modelo no puede estar en blanco");
            }
            if (string.IsNullOrWhiteSpace(comboBox4.Text.Trim()))
            {
                errores.Add("Tipo de combustible no puede estar en blanco");
            }

            using RentaCarroFinalContext db = new RentaCarroFinalContext();
            if (db.Vehiculos.Where(x => x.Descripcion == descripcionText.Text.Trim() && x.Id != vehiculo.Id).Any())
            {
                errores.Add("Ya existe un vehiculo con esa descripcion.");
            }
            if (db.Vehiculos.Where(x => x.NoChasis == textChasis.Text.Trim() && x.Id != vehiculo.Id).Any())
            {
                errores.Add("Ya existe un vehiculo con ese numero de chasis.");
            }
            if (db.Vehiculos.Where(x => x.NoPlaca == textBox3.Text.Trim() && x.Id != vehiculo.Id).Any())
            {
                errores.Add("Ya existe un vehiculo con ese numero de placa.");
            }
            if (errores.Count > 0)
            {
                var message = "";
                foreach (var e in errores)
                {
                    message += e + "\n";
                }
                MessageBox.Show(message);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }
    }


}