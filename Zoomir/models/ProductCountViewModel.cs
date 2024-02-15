using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoomir.models
{
    public class ProductCountViewModel : ProductViewModel
    {
        public int Count { get; set; }
        public ProductCountViewModel() { }
    }
}
