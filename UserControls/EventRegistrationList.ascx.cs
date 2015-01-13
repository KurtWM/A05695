using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Configuration;
using System.Collections.Specialized;
using Arena.Portal;
using Arena.Portal.UI;
using Arena.Security;
using Arena.Exceptions;
using Arena.Core;
using Arena.Event;
using Arena.Marketing;
using Arena.DataLayer.Core;
using Arena.DataLayer.Marketing;

namespace ArenaWeb.Custom.A05695.UserControls
{
    public partial class EventRegistrationList : PortalControl
    {
        protected bool eventsOnly;
        private EventProfile _eventProfile;

        [PageSetting("Detail Page", "The page that should be used to display promotion details.", true)]
        public string DetailPageSetting
        {
            get
            {
                return this.Setting("DetailPage", "", true);
            }
        }

        [PageSetting("Event Detail Page", "The page that should be used to display event details.", true)]
        public string EventDetailPageSetting
        {
            get
            {
                return Setting("EventDetailPage", "", false);
            }
        }

        [ListFromSqlSetting("Topic Area ID", "The Topic Area IDs that should be used to filter promotions.", false, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 69 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Multiple)]
        public string TopicAreaIDSetting
        {
            get
            {
                return Setting("TopicAreaID", "", false);
            }
        }

        [ListFromSqlSetting("Dateless Event Types", "List of Event Types for which a start date is irrelevant. The start date for these Event Types will not be displayed.", false, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 46 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Multiple)]
        public string DatelessEventTypeIDSetting
        {
            get
            {
                return Setting("DatelessEventTypeID", "", false);
            }
        }

        [CustomListSetting("Topic Area Filter", "Filter flag for area.", false, "primary", new string[] { "primary", "secondary", "both", "home" }, new string[] { "primary", "secondary", "both", "home" })]
        public string AreaFilterSetting
        {
            get
            {
                return this.Setting("AreaFilter", "primary", false);
            }
        }

        [ListFromSqlSetting("Event Types to Display", "List of Event Types to be displayed.", false, "", "SELECT lookup_id, lookup_value FROM core_lookup WHERE lookup_type_id = 46 AND active = 1 AND organization_id = 1 ORDER BY lookup_value", ListSelectionMode.Multiple)]
        public string DisplayEventTypeIDSetting
        {
            get
            {
                return Setting("DisplayEventTypeID", "", false);
            }
        }

        [NumericSetting("Max Items", "The maximum number of promotions to display (default = 6).", false)]
        public string MaxItemsSetting
        {
            get
            {
                return this.Setting("MaxItems", "6", false);
            }
        }

        [BooleanSetting("EventsOnly", "Flag indicating if only promotions tied to an event should be displayed.", false, false)]
        public string EventsOnlySetting
        {
            get
            {
                return this.Setting("EventsOnly", "false", false);
            }
        }

        [BooleanSetting("Hide Summary Popups", "Setting this to true will hide the column of info buttons and will prevent event summary popup dialogs. Default is false.", false, false)]
        public string HideSummaryPopupSetting
        {
            get
            {
                return this.Setting("HideSummaryPopup", "false", false);
            }
        }

        [TextSetting("List Header", "The header for this list.", false)]
        public string HeaderSetting
        {
            get
            {
                return Setting("Header", "", false);
            }
        }

        [TextSetting("Hidden Date Text", "The text to show when an event's date is hidden. Default is no text.", false)]
        public string HiddenDateSetting
        {
            get
            {
                return Setting("HiddenDate", "", false);
            }
        }

        [CssSetting("Css File", "An optional CSS File to use for this module.", false)]
        public string CssFileSetting
        {
            get
            {
                return this.Setting("CssFile", "", false);
            }
        }

        [CampusSetting("Campus Filter", "The campus to filter promotions for.", false)]
        public string CampusSetting
        {
            get
            {
                return this.Setting("Campus", "", false);
            }
        }

        public bool TopicAreaIDIsEmpty = true;
        public bool luidIsEmpty = true;
        public string strTopicAreas;
        public string errMsg = "";
        public string cssStr;
        public int campusID = -1;
        public int maxItems;
        public int itemCounter;

        protected void Page_Load(object sender, EventArgs e)
        {
            itemCounter = 0;
            if (!Page.IsPostBack)
            {
                if (this.Request["promotionID"] != null)
                {
                    this.Response.Redirect(new PromotionRequest(int.Parse(this.Request["promotionID"])).DetailUrl(this.Request, this.DetailPageSetting, this.EventDetailPageSetting), true);
                }

                DetermineTopicArea();

                SetVarsForSettings();

                PopulateRegistrationRepeater();

                RegisterScripts();

                if (HideSummaryPopupSetting == "true")
                {
                    cssStr = cssStr + ".DetailColumn_" + this.UniqueID.Replace("$", String.Empty) + " {display:none;} ";
                }

                cssLiteral.Text = "<style type='text/css'>" + cssStr + "</style>";
            }
        }

        protected void DetermineTopicArea()
        {
            luidIsEmpty = (Request.QueryString["luid"] == null);
            TopicAreaIDIsEmpty = (TopicAreaIDSetting.Length == 0);

            if (!TopicAreaIDIsEmpty) //if there is a LookupID set for this module instance it takes priority.
            {
                strTopicAreas = TopicAreaIDSetting;
            }
            else if (!luidIsEmpty) //else if there is an "luid" QueryString value, use it instead.
            {
                strTopicAreas = Request.QueryString["luid"];
            }
            else
            {
                strTopicAreas = "";
                DefaultHeader.Visible = true;
            }

            if (HeaderSetting.Trim().Length > 0)
            {
                DefaultHeader.Text = HeaderSetting;
            }

            if (strTopicAreas.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
            {
                //cssLiteral.Text = "<style type='text/css'>" + cssStr + "</style>";
                cssStr = cssStr + ".TopicColumn_" + this.UniqueID.Replace("$", String.Empty) + " {display:none;} ";
            }
            //else
            //{
            //  cssLiteral.Text = "";
            //}
        }

        /// <summary>
        /// Set variables.
        /// </summary>
        protected void SetVarsForSettings()
        {
            try
            {
                maxItems = int.Parse(this.MaxItemsSetting);
            }
            catch
            {
            }

            this.eventsOnly = this.EventsOnlySetting.ToLower() == "true";

            string script = "";
            if (this.CssFileSetting != string.Empty)
                script = "<link href=\"" + this.Request.ApplicationPath + "/css/" + this.CssFileSetting + "\" type=\"text/css\" rel=\"stylesheet\" />";
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "promotionCss", script);

            if (this.CampusSetting != string.Empty)
            {
                try
                {
                    campusID = int.Parse(this.CampusSetting);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Binds data to the Repeater.
        /// </summary>
        protected void PopulateRegistrationRepeater()
        {
            this.RegistrationRepeater.DataSource = (object)new PromotionRequestData().GetCurrentPromotionWebRequests_DT(this.TopicAreaIDSetting, this.AreaFilterSetting.ToLower(), campusID, 9999, this.eventsOnly, -1, ArenaContext.Current.Organization.OrganizationID);
            this.RegistrationRepeater.DataBind();
        }

        /// <summary>
        /// Executes logic for each row in the Repeater as it is bound.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void RegistrationRepeater_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            // Execute the following logic for Items and Alternating Items.
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Hide all rows initially--they will be made visible if they meet certain criteria.
                e.Item.Visible = false;
                // Get the promotionId for the current row.
                int intPromoId;
                // If the promotionId is a valid number, then continue.
                bool result = Int32.TryParse(((DataRowView)e.Item.DataItem)["promotion_request_id"].ToString(), out intPromoId);
                if (result)
                {
                    SqlDataReader PromoSqlDataReader = new PromotionRequestData().GetPromotionRequestByID(intPromoId, ArenaContext.Current.Organization.OrganizationID);
                    // Get the ordinal number of the "event_id" column.
                    int intEventIdColumn = PromoSqlDataReader.GetOrdinal("event_id");
                    // Confirm that the SqlDataReader has returned rows of data before continuing.
                    if (PromoSqlDataReader.HasRows)
                    {
                        // Begin reading rows of data and perform actions upon each one.
                        while (PromoSqlDataReader.Read())
                        {
                            if (!PromoSqlDataReader.IsDBNull(intEventIdColumn))
                            {
                                // Create an EventProfile object based on the event id in the current row.
                                // An EventProfile object contains methods which return properties for an event.
                                EventProfile eventProfile1 = new EventProfile(PromoSqlDataReader.GetInt32(intEventIdColumn));
                                // Determine if the event's type is to be displayed.
                                if (ItemIsEventType(eventProfile1.Type.LookupID))
                                {
                                    if (itemCounter < maxItems)
                                    {
                                        // The item meets all criteria--make the row visible.
                                        e.Item.Visible = true;
                                        itemCounter = itemCounter + 1;
                                        // Use the EventProfile object to display data about the event in the current row.
                                        ((Label)e.Item.FindControl("StartDateLabel")).Text = ShowStartDateLabel(eventProfile1.Type.LookupID) ? HiddenDateSetting : DateTimeExtensions.ToShortDateString(eventProfile1.Start, true);
                                        ((Label)e.Item.FindControl("TopicAreaLabel")).Text = eventProfile1.TopicArea.Value;

                                        // Uncomment the following line for debugging in development only.
                                        //((Label)e.Item.FindControl("TopicAreaLabel")).Text += " | " + eventProfile1.Type.Value + " | " + ItemIsEventType(eventProfile1.Type.LookupID).ToString() + " | " + ShowStartDateLabel(eventProfile1.Type.LookupID).ToString() + " | " + DatelessEventTypeIDSetting.ToString();
                                    }
                                }
                            }
                        }
                    }
                    PromoSqlDataReader.Close();
                }
            }
        }

        /// <summary>
        /// Returns true if the Event Type ID is found in the DisplayEventTypeIDSetting array.</summary>
        /// <param name="eventTypeId">The ID of the event being tested.</param>
        protected bool ItemIsEventType(int eventTypeId)
        {
            int number;
            bool result = Int32.TryParse(eventTypeId.ToString(), out number);
            if (result)
            {
                string[] displayEventTypes = DisplayEventTypeIDSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (displayEventTypes.Length > 0)
                {
                    return displayEventTypes.Contains(eventTypeId.ToString());
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the Event Type ID is found in the DatelessEventTypeIDSetting array.</summary>
        /// <param name="eventTypeId">The ID of the event being tested.</param>
        protected bool ShowStartDateLabel(int eventTypeId)
        {
            string[] datelessEventTypes = DatelessEventTypeIDSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return datelessEventTypes.Contains(eventTypeId.ToString());
        }

        /// <summary>
        /// Add JavaScript code to the page.</summary>
        /// 
        private void RegisterScripts()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n\n<script type=\"text/javascript\">\n");
            stringBuilder.Append("\t$(document).ready(function () {\n");
            stringBuilder.Append("\t\t$(\".tablesorter\").each(function (index) {\n");
            stringBuilder.Append("\t\t\t$.tablesorter.defaults.sortList = [[2, 0]];\n");
            stringBuilder.Append("\t\t\t$.tablesorter.defaults.widgets = ['zebra'];\n");
            stringBuilder.Append("\t\t\t$(this).tablesorter({ headers: { 0: { sorter: false}} });\n");
            stringBuilder.Append("\t\t});\n");
            stringBuilder.Append("\t});\n");
            stringBuilder.Append("\tfunction openDialog(obj) {\n");

            stringBuilder.Append("\t\t$.getScript(\"http://ajax.googleapis.com/ajax/libs/jqueryui/\" + jQuery.ui.version + \"/jquery-ui.min.js\", function () {\n");

            stringBuilder.Append("\t\t\t$( obj ).dialog({\n");
            stringBuilder.Append("\t\t\t\tmodal: true,\n");
            stringBuilder.Append("\t\t\t\ttitle: \"Information\",\n");
            stringBuilder.Append("\t\t\t\tbuttons: {\n");
            stringBuilder.Append("\t\t\t\t\t\"Done\": function() {\n");
            stringBuilder.Append("\t\t\t\t\t\t$( this ).dialog( \"close\" );\n");
            stringBuilder.Append("\t\t\t\t\t}\n");
            stringBuilder.Append("\t\t\t\t}\n");
            stringBuilder.Append("\t\t\t});\n");

            stringBuilder.Append("\t\t});\n");

            stringBuilder.Append("\t}\n");
            stringBuilder.Append("</script>\n\n");
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ListScripts", ((object)stringBuilder).ToString());
        }

    }
}