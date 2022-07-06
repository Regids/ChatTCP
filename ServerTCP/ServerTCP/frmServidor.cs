using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ServerTCP
{
    public partial class frmServidor : Form
    {
        private TcpListener tcpListener;
        private Thread subprocesoEscuchaClientes;
        private bool servidorIniciado;

        private delegate void DelegadoListBox(string texto);
        private delegate void DelegadoVacio();

        private List<Cliente> listaClientes = new List<Cliente>();
        private List<String> listaEnviar = new List<String>();

        public frmServidor()
        {
            InitializeComponent();
            btnDetener.Enabled = false;
 
        }

        private void EscucharClientes()
        {
            tcpListener.Start();
            while (servidorIniciado)
            {
                //Se bloquea hasta que un cliente se haya conectado al servidor 
                TcpClient client = tcpListener.AcceptTcpClient();
                /*Se crea un nuevo hilo para manejar la comunicación con los clientes que se conectan al servidor*/
                Thread clientThread = new Thread(new ParameterizedThreadStart(ComunicacionCliente));
                clientThread.Start(client);
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
                Invoke(new DelegadoVacio(vaciarLista), new Object[] { });
            }
            else
            {
                listaUsuarios.Items.Clear();
            }
        }


        private void ComunicacionCliente(object cliente)
        {
            TcpClient tcpCliente = (TcpClient)cliente;
            NetworkStream clienteStream = tcpCliente.GetStream();
            Cliente usuario = new Cliente(tcpCliente, clienteStream, "", "Disponible");

            while (servidorIniciado)
            {

                string mensaje;
                
                try
                {
                    mensaje = usuario.Lectura.ReadString(); //cada vez que leo algo que el cliente envio se hace esta linea de usuario 

                    switch (mensaje)
                    {
                        case "CambiarEstado":
                            usuario.EstadoCliente = usuario.Lectura.ReadString();
                            break;
                        case "Agregar":
                            string id = usuario.Lectura.ReadString();
                            usuario.AliasID = id;                          
                            DialogResult respuesta = MessageBox.Show("¿Desea aceptar al cliente " + id + "?", "Aceptar conexión", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                            if (respuesta == DialogResult.Yes)
                            {
                                listaClientes.Add(usuario);
                                usuario.Escritura.Write("Agregar");
                                usuario.Escritura.Write(true);

                               

                                listaEnviar = new List<String>();

                                for (int a = 0; a < listaClientes.Count; a++)
                                {
                                    listaEnviar.Add(listaClientes.ElementAt(a).AliasID);
                                }

                                vaciarLista();
                                for (int i = 0; i < listaEnviar.Count; i++)
                                {
                                    string nombre = listaEnviar[i];
                                    MostrarLista(nombre);
                                }

                                Byte[] bytesEnvio = ObjetoByte(listaEnviar);

                                for (int a = 0; a < listaClientes.Count; a++)
                                {
                                    Cliente destinatario = listaClientes.ElementAt(a);

                                    destinatario.Escritura.Write("Lista");
                                    destinatario.Escritura.Write(bytesEnvio.Length);
                                    destinatario.Escritura.Write(bytesEnvio);
                                }
                            }
                            else
                            {
                                usuario.Escritura.Write("Agregar");
                                usuario.Escritura.Write(false);
                            }

                            break;

                        case "Mensaje":
                            string aliasReceptor = usuario.Lectura.ReadString();
                            string mensajeUsuario = usuario.Lectura.ReadString();

                            for (int i = 0; i < listaClientes.Count; i++)
                            {
                                Cliente destinatario = listaClientes.ElementAt(i);

                                if (destinatario.AliasID.Equals(aliasReceptor))
                                {                                  
                                    if (usuario.EstadoCliente.Equals("Disponible"))
                                    {
                                        if (destinatario.EstadoCliente.Equals("Disponible"))
                                        {
                                            destinatario.Escritura.Write("Mensaje");
                                            destinatario.Escritura.Write(mensajeUsuario);

                                            usuario.Escritura.Write("Mensaje");
                                            usuario.Escritura.Write(mensajeUsuario);
                                        }
                                        else if (destinatario.EstadoCliente.Equals("Ocupado"))
                                        {
                                            usuario.Escritura.Write("Mensaje");
                                            usuario.Escritura.Write("El estado del destinatario es: Ocupado, no puede recibir mensajes");
                                        }
                                        else if (destinatario.EstadoCliente.Equals("No Molestar"))
                                        {
                                            usuario.Escritura.Write("Mensaje");
                                            usuario.Escritura.Write("El estado del destinatario es: No Molestar, no puede recibir mensajes");
                                        }
                                    }
                                    else if (usuario.EstadoCliente.Equals("Ocupado"))
                                    {
                                        usuario.Escritura.Write("Mensaje");
                                        usuario.Escritura.Write("El estado del usuario es: Ocupado, no puede enviar mensajes");
                                    }
                                    else if (destinatario.EstadoCliente.Equals("No Molestar"))
                                    {
                                        usuario.Escritura.Write("Mensaje");
                                        usuario.Escritura.Write("El estado del usuario es: No Molestar, no puede enviar mensajes");
                                    }                                   
                                }
                            }

                            break;

                        default:
                            MessageBox.Show(mensaje);
                            break;                        
                    }
                }
                catch 
                {
                   // listaClientes.Remove(usuario);
                    //Ocurrió un error en el socket 
                    break;
                }
            }

            usuario.ClienteTCP.Close();
            listaClientes.Remove(usuario);

            listaEnviar = new List<String>();

            for (int a = 0; a < listaClientes.Count; a++)
            {
                listaEnviar.Add(listaClientes.ElementAt(a).AliasID);
            }

            vaciarLista();
            for (int i = 0; i < listaEnviar.Count; i++)
            {
                string nombre = listaEnviar[i];
                MostrarLista(nombre);
            }

            Byte[] bytesEnvio1 = ObjetoByte(listaEnviar);

            for (int a = 0; a < listaClientes.Count; a++)
            {
                Cliente destinatario = listaClientes.ElementAt(a);

                destinatario.Escritura.Write("Lista");
                destinatario.Escritura.Write(bytesEnvio1.Length);
                destinatario.Escritura.Write(bytesEnvio1);
            }

        }       

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            IPAddress local = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(local, 30000);
            subprocesoEscuchaClientes = new Thread(new ThreadStart(EscucharClientes));
            subprocesoEscuchaClientes.Start();
            subprocesoEscuchaClientes.IsBackground = true;

            servidorIniciado = true;
            btnIniciar.Enabled = false;
            btnDetener.Enabled = true;

            label1.Text = "Estado Actual: Servidor Encendido";
            label1.ForeColor = Color.DarkBlue;


        }

        private void btnDetener_Click(object sender, EventArgs e)
        {
            servidorIniciado = false;
            tcpListener.Stop();
            subprocesoEscuchaClientes.Abort();


            label1.Text = "Estado Actual: Servidor Apagado";
            label1.ForeColor = Color.DarkRed;
            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;
        }

        //private void Conectar(string pIdentificadorCliente)
        //{
        //    ModificarLista(pIdentificadorCliente);
        //    txtHistorial.Invoke(modificarTxtHistorial, new object[] { pIdentificadorCliente + " se ha conectado..." });
        //    listaConectados.Invoke(modificarClientes, new object[] { id, true });
        //}

        //private void Desconectar(string pIdentificadorCliente)
        //{
        //    ModificarLista(pIdentificadorCliente);
        //    txtHistorial.Invoke(modificarTxtHistorial, new object[] { pIdentificadorCliente + " se ha desconectado!" });
        //    listaConectados.Invoke(modificarClientes, new object[] { id, false });
        //}

        public static byte[] ObjetoByte(Object obj)
        {
            if (obj == null)
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lblEstado_Click(object sender, EventArgs e)
        {

        }

        private void frmServidor_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void frmServidor_Load(object sender, EventArgs e)
        {

        }

        private void frmServidor_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
