using Infor.Droid;
using SQLite;
using System.IO;
using TestApp2;
using TestApp2.Common.APIclasses;
using TestApp2.Common.SQLite;
using TestApp2.Models.SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLLite_Droid))]
namespace Infor.Droid
{
    public class SQLLite_Droid : ISQLite
    {
        public SQLiteConnection GetConnection()
        {
            var dbName = "QuadrentInforAppDB.sqlite";
            var dbPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(dbPath,dbName);
            var con = new SQLiteConnection(path);
            con.CreateTable<LoginDetails>();
            con.CreateTable<UserDetails>();
            con.CreateTable<AppointmentList>();
            con.CreateTable<ContractDetails>();
            con.CreateTable<Attachments>();
            con.CreateTable<ContractLineDetails>();
            con.CreateTable<ContractLineSiteData>();
            con.CreateTable<CustomerListData>();
            con.CreateTable<WorkOrderData>();
            con.CreateTable<PostAppointmentTemp>();
            con.CreateTable<Configurations>();
            con.CreateTable<CollectionLineData>();
            con.CreateTable<CollectionWorkOrderData>();
            con.CreateTable<CollectionLineDetailsTable>();
            con.CreateTable<YardReturnLineData>();
            con.CreateTable<YardReturnPostTemplate>();
            con.CreateTable<YardPickingLinesData>();
            con.CreateTable<YardPickupPostTemplate>();
            con.CreateTable<YardPickingLineDetails>();
            con.CreateTable<YardReturnLineDetails>();
            con.CreateTable<PocDocumentDetails>();
            con.CreateTable<PodDocumentDetails>();
            //con.CreateTable<UnitNumbers>();
            return con;
        }
    }
}