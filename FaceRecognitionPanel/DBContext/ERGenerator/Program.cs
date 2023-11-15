using System.Text;
using DBContext;
using Microsoft.EntityFrameworkCore;

class ERGenerator
{
    // Main Method 
    static public void Main(String[] args)
    {
        using (var myContext = new FaceContext())
        {
            var path = "../er.dgml";
            File.WriteAllText(path, myContext.AsDgml(), Encoding.UTF8);
        }
    }
}
