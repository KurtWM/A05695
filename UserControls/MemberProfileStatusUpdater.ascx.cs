using Arena;
using Arena.Portal;
//using Arena.Portal.UI;
//using Arena.Security;
//using Arena.Exceptions;
using Arena.Core;
//using Arena.Event;
//using Arena.Marketing;
using Arena.DataLayer.Core;
//using Arena.DataLayer.Marketing;
using Arena.Enums;
using ASP;
using System;
using System.IO;
using System.Text;
using System.Data;
//using System.Data.SqlClient;
//using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
//using System.Web.Configuration;
using System.Web.Profile;
using System.Collections.Generic;
//using System.Collections.Specialized;


namespace ArenaWeb.Custom.A05695.UserControls
{
    public partial class MemberProfileStatusUpdater : PortalControl //, IPostBackEventHandler
    {
        //public bool ServingActivityTypeIDIsEmpty = true;
        protected string errMsg = "";
        protected int TagMemberStatusId;
        //int PersonId;
        //int ProfileId;
        protected int LatestProfileActivityId;

        private Profile _profile;
        private ProfileMember _profileMember;

        //protected DefaultProfile Profile
        //{
        //    get
        //    {
        //        return (DefaultProfile) this.Context.Profile;
        //    }
        //}

        //protected global_asax ApplicationInstance
        //{
        //    get
        //    {
        //        return (global_asax) this.Context.ApplicationInstance;
        //    }
        //}


        [ListFromSqlSetting("Serving Activity Types", "Types of activities that occur while communicating with a member who has volunteered.", false, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 36 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Multiple)]
        public string ServingActivityTypeIDSetting
        {
            get
            {
                return Setting("ServingActivityTypeID", "", false);
            }
        }

        [ListFromSqlSetting("Tag Member Status", "The status that a member is currently in within a particular profile.", false, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 35 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Single)]
        public string TagMemberStatusIDSetting
        {
            get
            {
                return Setting("TagMemberStatusID", "", false);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int num = -1;
            int personID = -1;

            // Get the "Add" button from the "ProfileMemberActivityList" module and add a click event handler
            // so that we can run our tests when it is clicked.
            Button btnAdd = ArenaWeb.UserControls.Core.ProfileMemberActivityList.FindControlRecursive(this.Page, "btnAdd") as Button;
            btnAdd.Click += new EventHandler(this.btnAdd_Click);

            // If values for "Profile" and "Person" are in the page's querystring, use their values.
            foreach (string name in this.Request.QueryString.AllKeys)
            {
                switch (name.ToUpper())
                {
                    case "PROFILE":
                        try
                        {
                            num = int.Parse(this.Request.QueryString.Get(name));
                            break;
                        }
                        catch
                        {
                            num = -1;
                            break;
                        }
                    case "PERSON":
                        try
                        {
                            personID = int.Parse(this.Request.QueryString.Get(name));
                            break;
                        }
                        catch
                        {
                            personID = -1;
                            break;
                        }
                }
            }
            this._profile = new Profile(num);
            this._profileMember = new ProfileMember(num, personID);

            // Hide this module if we don't have a legitimate value for "Profile" or "Person."
            if (this._profileMember.ProfileID == -1 || this._profileMember.PersonID == -1)
            {
                this.Visible = false;
            }
            else
            {
                if (this._profile.ProfileType == ProfileType.Personal && this.CurrentPerson.PersonID != this._profile.Owner.PersonID)
                {
                    // If this profile is personal and the currently logged in person is not the owner then access denied.
                    this.Response.Redirect("~/AccessDenied.aspx");
                }
                if (this.Page.IsPostBack)
                {
                    return;
                }
                //this.tblAdd.Visible = false;
                //new LookupType(SystemLookupType.ServingActivityType).Values.LoadDropDownList((ListControl) this.ddlType, -1);
                //this.ShowList();
            }


            //TagMemberStatusId = Convert.ToInt32(TagMemberStatusIDSetting, 16);
            TagMemberStatusId = Int32.Parse(TagMemberStatusIDSetting);
            //PersonId = this._profileMember.PersonID; // this.CurrentArenaContext.SelectedPerson.PersonID;
            //ProfileId = this._profileMember.ProfileID; // this.CurrentArenaContext.SelectedProfile.ProfileID;

            ShowTestData();
            //RegisterScripts();
        }

        /// <summary>
        /// Returns true if the Event Type ID is found in the TagMemberStatusIDSetting array.</summary>
        /// <param name="eventTypeId">The ID of the event being tested.</param>
        //protected bool ShowStartDateLabel(int eventTypeId)
        //{
        //    string[] datelessEventTypes = TagMemberStatusIDSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    return datelessEventTypes.Contains(eventTypeId.ToString());
        //}

        private void ShowTestData()
        {
            //if (this.Page.IsPostBack)
            //{
            //Page.ClientScript.RegisterStartupScript(this.GetType(), "Alerts", "alert('test');");
            //}
            List<int> ServingActivityTypeIds = ServingActivityTypeIDSetting.Split(',').Select(int.Parse).ToList();

            SimpleMsg.Text = "PersonID: " + this._profileMember.PersonID;
            SimpleMsg.Text += "<br />ProfileID: " + this._profileMember.ProfileID;
            SimpleMsg.Text += "<br />ServingActivityTypeIDSetting: " + ServingActivityTypeIDSetting;
            SimpleMsg.Text += "<br />ServingActivityTypeIds: " + ServingActivityTypeIds.ToString();
            SimpleMsg.Text += "<br />TagMemberStatusId: " + TagMemberStatusId;
            ProfileMember _profileMember = new ProfileMember(this._profileMember.ProfileID, this._profileMember.PersonID);
            SimpleMsg.Text += "<br />_profileMember.Status.LookupID: " + _profileMember.Status.LookupID;
            //SimpleMsg.Text += "<br />Member Status update is required = " + Convert.ToBoolean(TagMemberStatusId = _profileMember.Status.LookupID).ToString();
            SimpleMsg.Text += "<br />Profile Member Status ID is in the Serving Activity IDs = " + Convert.ToBoolean(ServingActivityTypeIds.IndexOf(_profileMember.Status.LookupID) != -1);

            DataTable dataTable1;
            dataTable1 = new ProfileMemberActivityData().GetProfileMemberActivityDetailsByProfileID_DT(this._profileMember.ProfileID, this.CurrentArenaContext.SelectedProfile.ProfileType, this.CurrentPerson.PersonID, this._profileMember.PersonID, ArenaContext.Current.Organization.OrganizationID);
            dgTest.DataSource = (object)dataTable1;
            dgTest.DataBind();
        }

        void btnAdd_Click(object sender, EventArgs e)
        {

            ProfileMember _profileMember = new ProfileMember(this._profileMember.ProfileID, this._profileMember.PersonID);
            SaveProfileMember(_profileMember);
            //ProfileMember _profileMember = new ProfileMember(this._profileMember.ProfileID, this._profileMember.PersonID);
            //SaveProfileMember(_profileMember);
            Response.Redirect(HttpContext.Current.Request.Url.ToString(), true);
        }

        protected void SaveProfileMember(ProfileMember pm)
        {
            pm.Source = pm.Source;
            pm.Status = new Lookup(SystemLookup.TagMemberStatus_Connected);
            pm.StatusReason = pm.StatusReason;
            pm.ProfileID = pm.ProfileID;
            pm.MemberNotes = "Automatically Updated";
            pm.Save(this.CurrentUser.Identity.Name);
        }

        /// <summary>
        /// Add JavaScript code to the page.</summary>
        /// 
        private void RegisterScripts()
        {
            //StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append("\n\n<script type=\"text/javascript\">\n");
            //stringBuilder.Append("\t$(document).ready(function () {\n");
            //stringBuilder.Append("\t\t$(\".tablesorter\").each(function (index) {\n");
            //stringBuilder.Append("\t\t\t$.tablesorter.defaults.sortList = [[2, 0]];\n");
            //stringBuilder.Append("\t\t\t$.tablesorter.defaults.widgets = ['zebra'];\n");
            //stringBuilder.Append("\t\t\t$(this).tablesorter({ headers: { 0: { sorter: false}} });\n");
            //stringBuilder.Append("\t\t});\n");
            //stringBuilder.Append("\t});\n");
            //stringBuilder.Append("\tfunction openDialog(obj) {\n");

            //stringBuilder.Append("\t\t$.getScript(\"http://ajax.googleapis.com/ajax/libs/jqueryui/\" + jQuery.ui.version + \"/jquery-ui.min.js\", function () {\n");

            //stringBuilder.Append("\t\t\t$( obj ).dialog({\n");
            //stringBuilder.Append("\t\t\t\tmodal: true,\n");
            //stringBuilder.Append("\t\t\t\ttitle: \"Information\",\n");
            //stringBuilder.Append("\t\t\t\tbuttons: {\n");
            //stringBuilder.Append("\t\t\t\t\t\"Done\": function() {\n");
            //stringBuilder.Append("\t\t\t\t\t\t$( this ).dialog( \"close\" );\n");
            //stringBuilder.Append("\t\t\t\t\t}\n");
            //stringBuilder.Append("\t\t\t\t}\n");
            //stringBuilder.Append("\t\t\t});\n");

            //stringBuilder.Append("\t\t});\n");

            //stringBuilder.Append("\t}\n");
            //stringBuilder.Append("</script>\n\n");
            //this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ListScripts", ((object)stringBuilder).ToString());
        }

    }
}