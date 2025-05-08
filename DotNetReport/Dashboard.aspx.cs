using ReportBuilder.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ReportBuilder.WebForms.DotNetReport
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private DotNetDashboardModel _model;
        public DotNetDashboardModel Model
        {
            get
            {
                return _model ?? new DotNetDashboardModel();
            }
            set
            {
                _model = value;
            }
        }

        private DotNetReportSettings GetSettings()
        {
            var settings = new DotNetReportSettings
            {
                ApiUrl = ConfigurationManager.AppSettings["dotNetReport.apiUrl"],
                AccountApiToken = ConfigurationManager.AppSettings["dotNetReport.accountApiToken"], // Your Account Api Token from your http://dotnetreport.com Account
                DataConnectApiToken = ConfigurationManager.AppSettings["dotNetReport.dataconnectApiToken"] // Your Data Connect Api Token from your http://dotnetreport.com Account
            };

            // Populate the values below using your Application Roles/Claims if applicable
            List<decimal> lstCompanyDataFilter = new List<decimal>() { 10924, 10925, 221, 10818, 10764, 668, 10928, 10926, 10958, 10986, 11039, 10971, 592, 11010, 11041, 10991, 10964, 591, 11050, 541, 545, 10851, 215, 404, 10976, 10993, 11051, 11043, 11008, 11042, 407, 10941, 10931, 11015, 11045, 10982, 143, 11030, 11014, 10975, 10980, 10952, 1, 10947, 11046, 11047, 11053, 566, 93, 10984, 11031, 465, 525, 707, 536, 10939, 10951, 11048, 10994, 527, 11026, 11040, 10827, 10988, 537, 10983, 10981, 10995, 11049, 11001, 10960, 10710, 402, 11004, 11007, 11005, 694, 11037, 11054, 10957, 10967, 10790, 589, 10769, 10954, 10996, 558, 11036, 10911, 10955, 10956, 10940, 11028, 10741, 213, 10997, 476, 10962, 10985, 471, 10959, 550, 10998, 11012, 42, 571, 11009, 10935, 11018, 11032, 11038, 11020, 10990, 11027, 548, 10989, 11033, 11035, 11025, 11029, 10953, 10965, 10906, 10972, 10973, 11019, 11024, 270, 11023, 10942, 10936, 10992, 11052, 10937, 11044, 11002, 11034, 10977, 11016, 11017, 629, 10974, 508, 276, 11021, 11000, 10999, 10930, 10938, 683, 10943, 695, 10969, 10801, 10961, 10869, 700, 10922, 637, 634, 10963, 1, 2, 3, 4, 5, 1, 42, 93, 141, 142, 143, 213, 294, 638, 215, 215, 143, 213 };
            settings.ClientId = "XRM-QA (https://xrmqa.acrocorp.com/xrm_qa/)";//"DotNetReportLocalClient";//"XRM-QA (https://xrmqa.acrocorp.com/xrm_qa/)";//"DotNetReportLocalClient";//"DotNetReportLocalClient";  "https-://staging.acroxrm.com/PBLiveCopy" You can pass your multi-tenant client id here to track their reports and folders
            settings.UserId = "YPATEL"; // You can pass your current authenticated user id here to track their reports and folders            
            settings.UserName = "";
            settings.CurrentUserRole = new List<string>() { "MSP" }; // Populate your current authenticated user's roles

            settings.Users = new List<dynamic>() { "ypatel", "A1A2", "acroxrm", "Sonal.Agrawal", "yash.patel" }; // Populate all your application's user, ex  { "Jane", "John" } or { new { id="1", text="Jane" }, new { id="2", text="John" }}
            settings.UserRoles = new List<string>() { "None", "MSP", "Client", "Supplier", "Employee", "SoWEmployee" }; // Populate all your application's user roles, ex  { "Admin", "Normal" }       
            settings.CanUseAdminMode = true; // Set to true only if current user can use Admin mode to setup reports and dashboard
            settings.DataFilters = new
            {
                _applyOnce = "true"
                ,
                CompanyId = String.Join(",", lstCompanyDataFilter)
                ,
                ReportParameterUserid = "acroxrm"
                ,
                UDFDetails__UserGroupId = 16
            }; // add global data filters to apply as needed https://dotnetreport.com/docs/advance-topics/global-filters/
            //,CLPUDFDetails__UserGroupId= "2"
            return settings;
        }

        public dynamic GetDashboards(bool adminMode = false)
        {
            var settings = GetSettings();

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole)),
                    new KeyValuePair<string, string>("adminMode", adminMode.ToString()),
                });

                var response = client.PostAsync(new Uri(settings.ApiUrl + $"/ReportApi/GetDashboards"), content).Result;
                var stringContent = response.Content.ReadAsStringAsync().Result;

                Context.Response.StatusCode = (int)response.StatusCode;
                return (new JavaScriptSerializer()).Deserialize<dynamic>(stringContent);
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(Request.QueryString["id"] != null ? Request.QueryString["id"] : "0");
            bool adminMode = Convert.ToBoolean(Request.QueryString["adminMode"] != null ? Request.QueryString["adminMode"] : "false");
            var model = new List<DotNetDasboardReportModel>();
            var settings = GetSettings();

            var dashboards = (dynamic[])(GetDashboards(adminMode));
            if (id == 0 && dashboards.Length > 0)
            {
                id = ((dynamic)dashboards.First())["Id"];
            }

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole)),
                    new KeyValuePair<string, string>("id", id.ToString()),
                    new KeyValuePair<string, string>("adminMode", adminMode.ToString()),
                });

                var response = client.PostAsync(new Uri(settings.ApiUrl + $"/ReportApi/LoadSavedDashboard"), content).Result;
                var stringContent = response.Content.ReadAsStringAsync().Result;

                model = (new JavaScriptSerializer()).Deserialize<List<DotNetDasboardReportModel>>(stringContent);
            }

            Model = new DotNetDashboardModel
            {
                Dashboards = dashboards.Select(x => (dynamic)x).ToList(),
                Reports = model
            };
        }
    }
}