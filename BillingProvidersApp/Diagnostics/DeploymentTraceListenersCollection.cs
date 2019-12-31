using System.Collections;

namespace BillingProvidersApp.Diagnostics
{
    public class DeploymentTraceListenersCollection : CollectionBase
    {
        public DeploymentTraceListener this[int index]
        {
            get
            {
                return ((DeploymentTraceListener)(List[index]));
            }
            set
            {
                List[index] = value;
            }
        }


        public DeploymentTraceListenersCollection()
        {
        }


        public int Add(DeploymentTraceListener val)
        {
            return List.Add(val);
        }

        public void AddRange(DeploymentTraceListener[] val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                this.Add(val[i]);
            }
        }

        public new DeploymentTraceListenerEnumerator GetEnumerator()
        {
            return new DeploymentTraceListenerEnumerator(this);
        }


        public class DeploymentTraceListenerEnumerator : IEnumerator
        {
            IEnumerator baseEnumerator;
            IEnumerable temp;


            public DeploymentTraceListener Current
            {
                get
                {
                    return ((DeploymentTraceListener)(baseEnumerator.Current));
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return baseEnumerator.Current;
                }
            }


            public DeploymentTraceListenerEnumerator(DeploymentTraceListenersCollection mappings)
            {
                this.temp = mappings;
                this.baseEnumerator = temp.GetEnumerator();
            }

            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                baseEnumerator.Reset();
            }
        }
    }
}
