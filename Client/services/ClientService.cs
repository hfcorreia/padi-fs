using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.services
{
    abstract class ClientService
    {
        public ClientState State { get; set; }

        public void waitVoidQuorum(Task[] tasks, int quorum)
        {
            int responsesCounter = 0;
            List<Exception> errorResponses = new List<Exception>();

            while (responsesCounter < quorum)
            {
                responsesCounter = 0;
                for (int i = 0; i < tasks.Length; ++i)
                {
                    try {
                        responsesCounter += tasks[i].IsCompleted ? 1 : 0;
                        if (tasks[i].Exception!=null) {
                            errorResponses.Add(tasks[i].Exception.Flatten().InnerException);
                        }
                    } catch(AggregateException aggregateException){
                        errorResponses.Add(aggregateException.Flatten().InnerException);
                    }
                }
            }

            if (errorResponses.Count > 0) {
                Console.WriteLine("#Client - waitVoidQuorum - Error: " + errorResponses[0].Message + ".\n" + errorResponses[0].StackTrace);
                throw errorResponses[0];
            }
        }

        public T waitQuorum<T>(Task<T>[] tasks, int quorum)
        {
            List<Object> responses = new List<Object>();
            while (responses.Count < quorum)
            {
                responses = new List<Object>();
                for (int i = 0; i < tasks.Length; ++i)
                {
                    if (tasks[i].IsCompleted)
                    {
                        try
                        {
                            responses.Add(tasks[i].Result);
                        }
                        catch (AggregateException aggregateException)
                        {
                            responses.Add(aggregateException.Flatten().InnerException);
                        }
                    }
                }
            }
            //choose the bettew option
            int betterOptionId = 0;

            if (responses[betterOptionId] is Exception)
            {
                throw (Exception)responses[betterOptionId];
            }
            else
            {
                return (T)responses[betterOptionId];
            }
        }



        public ClientService(ClientState clientState){
            State = clientState;
        }

        public abstract void execute();
    }
}
