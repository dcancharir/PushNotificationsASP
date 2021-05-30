using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PushNotification
{
    public class NotificationComponent
    {
        //Here we will add a function for register notification(will add sql dependency)
        public void RegisterNotification(DateTime currentTime)
        {
            string consStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
            string sqlCommand = @"Select [ContactId],[ContactName],[ContactNo] from [dbo].[Contacts] where [AddedOn]>@AddedOn";
            //you can notice here I have added table like this [dbo].[Contacts] with [dbo], its mendatory when you use sql Dependency
            using (SqlConnection con = new SqlConnection(consStr))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);
                cmd.Parameters.AddWithValue("@AddedOn", currentTime);
                if (con.State != System.Data.ConnectionState.Open) {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);
                sqlDep.OnChange += sqlDep_OnChange;
                //we must have to execute the command here
                using (SqlDataReader reader=cmd.ExecuteReader()) {
                    //nothing need to add here now
                }
            }
        }
        public void sqlDep_OnChange(Object sender, SqlNotificationEventArgs e) {
            //throw new NotImplementedException();
            if (e.Type == SqlNotificationType.Change) {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;
                //from here we will send notification message to client
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                notificationHub.Clients.All.notify("added");
                //re-register notification
                RegisterNotification(DateTime.Now);
            }
        }
        public List<Contact>GetContacts(DateTime afterDate)
        {
            using(MyPushNotificationEntities dc=new MyPushNotificationEntities())
            {
                return dc.Contacts.Where(a => a.AddedOn > afterDate).OrderByDescending(a => a.AddedOn).ToList();
            }
        }
    }
}