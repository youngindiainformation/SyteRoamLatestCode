using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestApp2
{

    public class TestApp2SyteLineServicePageFlyoutMenuItem
    {
        public TestApp2SyteLineServicePageFlyoutMenuItem()
        {
            TargetType = typeof(TestApp2SyteLineServicePageFlyoutMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public ImageSource Flyoutouticon { get; set; }
        public Type TargetType { get; set; }
    }
}