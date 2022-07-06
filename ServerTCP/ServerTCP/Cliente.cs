using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ServerTCP
{
    [Serializable]

    class Cliente
    {   // Variables.
        private NetworkStream salidaMensaje;
        private TcpClient clienteTCP;
        private BinaryWriter escritura;
        private BinaryReader lectura;
        private string aliasID;
        private string estadoCliente;
        private List<string> bloqueados;

        // Setter y getter de las variables privadas utilizadas.
        public NetworkStream SalidaMensaje { get => salidaMensaje; set => salidaMensaje = value; }
        public TcpClient ClienteTCP { get => clienteTCP; set => clienteTCP = value; }
        public BinaryWriter Escritura { get => escritura; set => escritura = value; }
        public BinaryReader Lectura { get => lectura; set => lectura = value; }
        public string AliasID { get => aliasID; set => aliasID = value; }
        public string EstadoCliente { get => estadoCliente; set => estadoCliente = value; }
        public List<string> Bloqueados { get => bloqueados; set => bloqueados = value; }

        // Método constructor de la clase Cliente, recibe por parámetro al TcpClient y el NetworkStream.
        public Cliente(TcpClient cliente, NetworkStream salidaMensaje, string alias, string estado)
        {
            this.SalidaMensaje = salidaMensaje;
            this.ClienteTCP = cliente;
            BinaryWriter escritor = new BinaryWriter(salidaMensaje);
            this.Escritura = escritor;
            BinaryReader lector = new BinaryReader(salidaMensaje);
            this.Lectura = lector;
            this.AliasID = alias;
            this.EstadoCliente = estado;
        }
    }
}
