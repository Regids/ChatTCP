using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClienteTCP
{
    [Serializable]

    class Usuario
    {
        private string aliasID;
        private string estadoCliente;

        public string AliasID { get => aliasID; set => aliasID = value; }
        public string EstadoCliente { get => estadoCliente; set => estadoCliente = value; }

        public Usuario(string alias, string estado)
        {
            this.AliasID = alias;
            this.EstadoCliente = estado;
        }
    }
}
