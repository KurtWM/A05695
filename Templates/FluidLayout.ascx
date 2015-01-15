<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FluidLayout.ascx.cs" Inherits="ArenaWeb.Custom.A05695.Templates.FluidLayout" %>

<style type="text/css">
  @media (max-width: 980px) {
    /* Enable use of floated navbar text */
    .navbar-text.pull-right {
      float: none;
      padding-left: 5px;
      padding-right: 5px;
    }
  }
</style>

<asp:PlaceHolder ID="HeaderCell" Runat="server"></asp:PlaceHolder>
<div class="navbar navbar-inverse navbar-fixed-top">
  <div class="navbar-inner">
    <div class="container-fluid">
      <button type="button" class="btn btn-navbar" data-toggle="collapse" data-target=".nav-collapse">
        <span class="icon-bar"></span>
        <span class="icon-bar"></span>
        <span class="icon-bar"></span>
      </button>
      
      <div class="nav-collapse collapse">
        <div id="social-bar" class="navbar-fixed-top">
          <div class="row-fluid">
            <div class="span8">
              <!-- Developer Note: Some styles set for UserInfoCell in /Custom/JohnsonFerry/Scripts/main.js -->
              <asp:PlaceHolder ID="UserInfoCell" Runat="server"></asp:PlaceHolder>
            </div>
            <div class="span4 navbar-social">
              <span class="pull-right">FB | TW | Email</span>
            </div>
          </div>
        </div>
        <div id="id-bar" class="navbar-fixed-top">
          <div class="row-fluid">
            <div class="span2">
              <a href="/"><img src="/Content/HtmlImages/Public/Images/General/reverse_JFlogomark-small.png" alt="Johnson Ferry Logo" id="site-logo" /></a>
            </div>
          </div>
        </div>
        <div id="nav-bar" class="row-fluid">
          <asp:PlaceHolder ID="NavCell" Runat="server"></asp:PlaceHolder>
        </div>
      </div><!--/.nav-collapse -->
    </div>
  </div>
</div>

<div class="container-fluid">
  <div class="row-fluid">
    <div class="span3">
      <asp:PlaceHolder ID="SubNavCell" Runat="server"></asp:PlaceHolder>
    </div><!--/span-->
    <div class="span9">
      <asp:PlaceHolder ID="MainCell" Runat="server"></asp:PlaceHolder>
    </div><!--/span-->
  </div><!--/row-->

  <hr>

  <footer>
    <asp:PlaceHolder ID="FooterCell" Runat="server"></asp:PlaceHolder>
    <p>&copy; Company 2013</p>
  </footer>

</div><!--/.fluid-container-->