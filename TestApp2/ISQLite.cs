using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
