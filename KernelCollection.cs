using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace ProcView
{

    [DataContract(Name = "KernelCollection", Namespace = "http://www.facebook.com/joshua.niehus")]
    [KnownType(typeof(Kernel))]

    public class KernelCollection : IEnumerable
    {

        public KernelCollection()
        {
        }

        [DataMember(Name = "KernelList")]
        public List<Kernel> kernelList = new List<Kernel>();

        public void AddKernel(Kernel iKernel)
        {
            kernelList.Add(iKernel);
        }

        public void DeleteKernel(int index)
        {
            kernelList.RemoveAt(index);
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < kernelList.Count; i++)
            {
                yield return kernelList[i];
            }
        }
    }
}
