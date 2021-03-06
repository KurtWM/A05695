﻿using Arena;
using Arena.Portal;
using Arena.Core;
using Arena.DataLayer.Core;
using Arena.Enums;
using ASP;
using System;
using System.IO;
using System.Text;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Profile;
using System.Collections.Generic;


namespace ArenaWeb.Custom.A05695.UserControls
{
    public partial class MemberProfileStatusUpdater : PortalControl
    {
        //protected string errMsg = "";
        private Profile _profile;
        private ProfileMember _profileMember;
        private List<int> ServingActivityTypeIds;
        private String ddlTypeClientID;
        protected DropDownList ddlType;
        protected Button btnAdd;

        [ListFromSqlSetting("Serving Activity Types", "Activity Types that, if added to a profile, will trigger a status update. Multiple Activity Types may be selected.", true, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 36 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Multiple)]
        public string ServingActivityTypeIDSetting
        {
            get
            {
                return Setting("ServingActivityTypeID", "", true);
            }
        }

        [ListFromSqlSetting("Member Status", "If any of the selected Serving Activity Types are added to a person, that person's status will be set to this value.", true, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 35 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Single)]
        public string TagMemberStatusIDSetting
        {
            get
            {
                return Setting("TagMemberStatusID", "", true);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
            ddlTypeClientID = ddlType.ClientID;
            RegisterScripts(ddlTypeClientID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int num = -1;
            int personID = -1;

            ActivityTypeValue.Value = ddlType.SelectedValue;
            ActivityTypeIDs.Value = ServingActivityTypeIDSetting;

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
            }
            CheckMemberStatus();
        }

        /// <summary>
        /// Check to see if the current person's status already matches the selected status and proceed accordingly.</summary>
        private void CheckMemberStatus()
        {
            if (_profileMember.Status.LookupID == Int32.Parse(TagMemberStatusIDSetting)) 
            {
                this.Visible = false;
                //ScriptManager.RegisterStartupScript(Page, this.GetType(), "closeModule", "alert('STATUS is already set to desired value.');", true);
            }
            else
            {
                //ScriptManager.RegisterStartupScript(Page, this.GetType(), "continueModule", "alert('STATUS is NOT set to desired value.');", true);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ServingActivityTypeIds = ActivityTypeIDs.Value.Split(',').Select(int.Parse).ToList();
            if (Convert.ToBoolean(ServingActivityTypeIds.Contains(Int32.Parse(ActivityTypeValue.Value))))
            {
                //ProfileMember _profileMember = new ProfileMember(this._profileMember.ProfileID, this._profileMember.PersonID);
                SaveProfileMember(_profileMember);
                Response.Redirect(HttpContext.Current.Request.Url.ToString(), false);
            }
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

        private void InitializeComponent()
        {
            // Get the "Add" button from the "ProfileMemberActivityList" module and add a click event handler
            // so that we can run our tests when it is clicked.
            this.btnAdd = ArenaWeb.UserControls.Core.ProfileMemberActivityList.FindControlRecursive(this.Page, "btnAdd") as Button;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            // Get the Activity Type dropdown list from the "ProfileMemberActivityList" module so we can get the selected value.
            this.ddlType = ArenaWeb.UserControls.Core.ProfileMemberActivityList.FindControlRecursive(this.Page, "ddlType") as DropDownList;
        }

        /// <summary>
        /// Add JavaScript code to the page.</summary>
        /// 
        private void RegisterScripts(String ddlTypeID)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n\n<script type=\"text/javascript\">\n");
            stringBuilder.Append("\t$(document).ready(function () {\n");
            stringBuilder.Append("\t\t$('#" + ddlTypeID + "').change(function() {\n");
            stringBuilder.Append("\t\t\tvar x = $(this).val();\n");
            stringBuilder.Append("\t\t\t$('#" + ActivityTypeValue.ClientID + "').val(x);\n");
            //stringBuilder.Append("\t\t\talert( $('#" + ActivityTypeValue.ClientID + "').val() );\n");
            stringBuilder.Append("\t\t\t});\n");
            stringBuilder.Append("\t\t});\n");
            stringBuilder.Append("</script>\n\n");
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "activityTypeChangeScript", ((object)stringBuilder).ToString());
        }
    }
}