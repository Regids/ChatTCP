using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClienteTCP
{
    public partial class frmCliente : Form
    {
        private NetworkStream salidaMensaje;
        private TcpClient clienteTCP;
        private BinaryWriter escritura;
        private BinaryReader lectura;
        private string aliasID;
        string destinatario;
        private bool clienteConectado = false;
        private Thread subprocesoEscuchaServidor;

        List<Usuario> lista = new List<Usuario>();

        public NetworkStream SalidaMensaje { get => salidaMensaje; set => salidaMensaje = value; }
        public TcpClient ClienteTCP { get => clienteTCP; set => clienteTCP = value; }
        public BinaryWriter Escritura { get => escritura; set => escritura = value; }
        public BinaryReader Lectura { get => lectura; set => lectura = value; }
        public string AliasID { get => aliasID; set => aliasID = value; }
        public bool ClienteConectado { get => clienteConectado; set => clienteConectado = value; }
        public Thread SubprocesoEscuchaServidor { get => subprocesoEscuchaServidor; set => subprocesoEscuchaServidor = value; }
        public string Destinatario { get => destinatario; set => destinatario = value; }

        public frmCliente()
        {
            InitializeComponent();
        }

        //Delegado, necesario para modificar controles de la interfaz gráfica desde un subproceso
        private delegate void DelegadoTextbox(string texto);
        private delegate void DelegadoListBox(string texto);
        private delegate void DelegadoHabilitado(bool habilitar);
        private delegate void DelegadoDeshabilitado(bool deshabilitar);
        private delegate void DelegadoForm();
        private delegate void DelegadoVacio();


        private void cerrarFormulario()
        {
            if (this.InvokeRequired)
            {
                Invoke(new DelegadoForm(cerrarFormulario), new object[] {});
            }
            else
            {
                try
                {
                    ClienteTCP.Close();
                    SubprocesoEscuchaServidor.Abort();

                }catch(Exception )
                {

                }
              

                this.Dispose();
            }
        }

        private void modificarConectar()
        {
            if (txtIdentificador.InvokeRequired)
            {
                Invoke(new DelegadoForm(modificarConectar), new object[] { });
            }
            else
            {
                txtIdentificador.Enabled = !txtIdentificador.Enabled;
                btnConectar.Enabled = !btnConectar.Enabled;
                btnDesconectar.Enabled = !btnDesconectar.Enabled;

                label1.Text = "Estado de la Conexción: Conectado";
                label1.ForeColor = Color.DarkBlue;
                panel1.Visible = true;
            }
        }


        //Método utilizado por el delegado para modificar la interfaz gráfica desde un subproceso
        private void EspacioMensaje(string texto)
        {
            if (txtMensajes.InvokeRequired)
            {
                Invoke(new DelegadoTextbox(EspacioMensaje), new object[] { texto });
            }
            else
            {
                txtMensajes.Text += texto;
                txtMensajes.AppendText(Environment.NewLine);
            }
        }

        private void HabilitarBoton(bool habilitar)
        {
            if(btnConectar.InvokeRequired || btnDesconectar.InvokeRequired)
            {
                Invoke(new DelegadoHabilitado(HabilitarBoton), new object[] { habilitar });
            }
            else
            {
                btnConectar.Enabled = true;
            }
        }

        private void DeshabilitarBoton(bool habilitar)
        {
            if (btnConectar.InvokeRequired || btnDesconectar.InvokeRequired)
            {
                Invoke(new DelegadoDeshabilitado(DeshabilitarBoton), new object[] { habilitar });
            }
            else
            {
                btnDesconectar.Enabled = false;
            }   
        }

        private void MostrarLista(Object mensaje)
        {
            if (listaUsuarios.InvokeRequired)
            {
                Invoke(new DelegadoListBox(MostrarLista), new Object[] { mensaje });
            }
            else
            {
                listaUsuarios.Items.Add(mensaje); 
            }
        }

        private void vaciarLista()
        {
            if (listaUsuarios.InvokeRequired)
            {
                Invoke(new DelegadoVacio(vaciarLista), new Object[] {});
            }
            else
            {
                listaUsuarios.Items.Clear();
            }
        }

        private void ModificarListaCliente(object alias, bool agregar)
        {
            if (agregar)
            {
                listaUsuarios.Items.Add(alias);
            }
            else
            {
                listaUsuarios.Items.Remove(alias);
            }
        }

        //Método utilizado por el delegado para modificar la interfaz gráfica desde un subproceso
        private void EscribirTexto(string texto)
        {
            txtMensajes.AppendText(DateTime.Now.ToString() + " - " + texto);
            txtMensajes.AppendText(Environment.NewLine);
        }

        //Método utilizado por el delegado para modificar la interfaz gráfica desde un subproceso
        private void ModificarLista(string texto, bool agregar)
        {
            //listaConectados.Items.Add(texto);

            if (agregar)
            {
                listaUsuarios.Items.Add(texto);
            }
            else
            {
                listaUsuarios.Items.Remove(texto);
            }
        }

        private void IniciarCliente() {
            if (!(txtIdentificador.Text.Equals(string.Empty)))
            {
                try
                {
                    IPAddress ipServidor = IPAddress.Parse("127.0.0.1");
                    ClienteTCP = new TcpClient();
                    IPEndPoint serverEndPoint = new IPEndPoint(ipServidor, 30000);

                    ClienteTCP.Connect(serverEndPoint);
                    salidaMensaje = ClienteTCP.GetStream();
                    
                    BinaryWriter escritor = new BinaryWriter(SalidaMensaje);
                    this.Escritura = escritor;
                    BinaryReader lector = new BinaryReader(SalidaMensaje);
                    this.Lectura = lector;

                    aliasID = txtIdentificador.Text;

                    escritura.Write("Agregar");
                    escritura.Write(txtIdentificador.Text);

                    while (true)
                    {
                        string mensaje;

                        try
                        {
                            mensaje = lector.ReadString(); // con esto se lee lo que el servidor envia

                            switch (mensaje)
                            {
                                case "Agregar": // se hace un encabezado por cada accion que se quiera realizar 
                                    Boolean resultado = lector.ReadBoolean();

                                    if (resultado == true)
                                    {

                                        modificarConectar();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Fue rechazado por el servidor");
                                        cerrarFormulario();
                                    }

                                    break;

                                case "Mensaje":
                                    string mensajeEnviado = lector.ReadString();
                                    EspacioMensaje(mensajeEnviado);
                                    
                                    break;

                                case "Lista":
                                    int bytesServidor = lector.ReadInt32();
                                    List<String> listaAmigos = (List<String>)ByteObjeto(lector.ReadBytes(bytesServidor));
                                    vaciarLista();

                                    for (int i = 0; i < listaAmigos.Count; i++)
                                    {
                                        string nombre = listaAmigos[i];
                                        MostrarLista(nombre);
                                    }
                         
                                    break;
                                default:
                                    //MessageBox.Show(mensaje);
                                    break;
                            }
                        }
                        catch
                        {
                            //clienteConectado = false;
                            //HabilitarBoton(true);
                            //DeshabilitarBoton(false);
                            break;
                        }
                    }
                }
                catch (SocketException)
                {
                    MessageBox.Show("Verifique que el servidor esté escuchando clientes...", "No es posible conectarse", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else
            {
                MessageBox.Show("Debe ingresar el identificador del cliente", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            SubprocesoEscuchaServidor = new Thread(new ThreadStart(IniciarCliente));
            SubprocesoEscuchaServidor.Start();
        }

        private void btnDesconectar_Click(object sender, EventArgs e)
        {
            cerrarFormulario();
        }


        private void frmCliente_FormClosed(object sender, FormClosedEventArgs e)
        {
            cerrarFormulario();
        }


        public static byte[] ObjetoByte(Object obj)
        {
            if(obj == null)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static Object ByteObjeto(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        // Para poder hablar con algun cliente es necesario darle click dos veces al usuario con el que quiere conversar


        private void comboEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboEstado.SelectedIndex != -1 && escritura != null)
            {
                Escritura.Write("CambiarEstado");
                Escritura.Write(comboEstado.SelectedItem.ToString());
            }
           
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (listaUsuarios.SelectedIndex != -1)
            {
                escritura.Write("Mensaje");
                escritura.Write(Destinatario);
                escritura.Write(aliasID + ":" + txtEnviar.Text);
            }
            else
            {
                MessageBox.Show("Seleccione su destinatario de la lista");
            }
        }

        private void listaUsuarios_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                Destinatario = listaUsuarios.SelectedItem.ToString();

            }
            catch (Exception)
            {
            }
        }
    }
}
