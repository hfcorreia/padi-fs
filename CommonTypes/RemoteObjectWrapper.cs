using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class RemoteObjectWrapper
    {
        
        public int Port { get; set; }
        public int Id { get; set; }
        public string Host { get; set; }
        public string URL { get { return "tcp://" + Host + ":" + Port + "/" + Id; } }

        private Object wrappedObject = null;

        public RemoteObjectWrapper( int port, int id, string host ) 
        {
            Port = port;
            Id = id;
            Host = host;
        }

        public T getObject<T>() 
        {
            if ( wrappedObject == null )
            {
                wrappedObject = Activator.GetObject( typeof(T), URL );
            }

            //return (T) ( wrappedObject.GetType() == typeof(T) ? wrappedObject : null );
            return (T)wrappedObject;
        }
    }
}
