/* ======================================================
 * Arena-Bootstrap compatibility scripts
 *
 * These scripts attempt to modify certain Arena structures 
 * to be more compatible with Bootstrap. 
 * ====================================================== */

$(document).ready(function () {

    /* loginlogout.ascx user control
     * ======================================================= */

    // Create variable of all anchors with class='logWrap.'
    var loginTags = $(".navbar-fixed-top .logWrap a");
    // Add navbar-link class for Bootstrap compatibility.
    loginTags.addClass("navbar-link");
    // Add arena-modified class to indicate modified elements.
    loginTags.addClass("arena-modified");
    // 1. Remove loginTags elements from their Arena parent element.
    // 2. Wrap loginTags in Bootstrap compatible container.
    loginTags.unwrap().wrapAll('<small class="navbar-text pull-left navbar-userinfo" />');
    // Add a pipe character before all but the first navbar-link elements.
    $('.navbar-text .navbar-link:gt(0)').before(' | ');


    /* Fix Arena checkboxes - move checkboxes into their labels and add 'checkbox' class
     * ======================================================= */

    $('input[type="checkbox"]').each(function () {
        var currentCheckbox = $(this);
        var checkboxId = $(this).attr('id');
        $('label[for=' + checkboxId + ']').addClass('checkbox text-left').prepend(currentCheckbox);
    });

    /* Fix Arena radio buttons - move radio buttons into their labels and add 'radio' class
     * ======================================================= */

    $('input[type="radio"]').each(function () {
        var currentRadio = $(this);
        var radioId = $(this).attr('id');
        $('label[for=' + radioId + ']').addClass('radio text-left').prepend(currentRadio);
    });

    /* Give buttons bootstrap styles
     * ======================================================= */

    $('input[type="submit"]').removeClass().addClass('btn btn-primary');

});

function pageLoad() {

    /* Convert ajax tabs to bootstrap tabs
     * ======================================================= */

    $('.ajax__tab_container').each(function () {
        var currentTabContainer = $(this);
        var newTabHeader = currentTabContainer.before('<ul class="nav nav-tabs">');
        var newTabContent = currentTabContainer.before('<div class="tab-content">');
        var myTabPanels = $('.ajax__tab_panel', this);
        myTabPanels.each(function () {
            var panelId = $(this).attr('id');
            var tabLabel = $('#' + panelId + '_tab').text();
            var tabClass = $('#' + panelId + '_tab').attr('class') == 'ajax__tab_active' ? 'active' : '';
            $('.tab-content').append($(this).contents().unwrap().wrapAll('<div class="tab-pane fade" id="' + $(this).attr('id') + '" />').parent());
            $('.nav-tabs').append('<li class="' + tabClass + '"><a href="#' + panelId + '" data-toggle="tab">' + tabLabel + '</a></li>');
            $('#' + panelId).addClass(tabClass).addClass('in');
        });
    });
    $('.ajax__tab_container').remove();
}

// Start with this structure...
//  <div class="ajax__tab_xp ajax__tab_container ajax__tab_default" id="ctl13_ctl06_tcEdit">
//    <div id="ctl13_ctl06_tcEdit_header" class="ajax__tab_header">
//      <span id="ctl13_ctl06_tcEdit_tpWebView_tab" class="ajax__tab_active" style="">
//        <span class="ajax__tab_outer">
//          <span class="ajax__tab_inner">
//            <span id="__tab_ctl13_ctl06_tcEdit_tpWebView" class="ajax__tab_tab">Web</span>
//          </span>
//        </span>
//      </span>
//      <span id="ctl13_ctl06_tcEdit_tpBulletin_tab" class="">
//        <span class="ajax__tab_outer">
//          <span class="ajax__tab_inner">
//            <span id="__tab_ctl13_ctl06_tcEdit_tpBulletin" class="ajax__tab_tab">Bulletin</span>
//          </span>
//        </span>
//      </span>
//  </div>
//  <div id="ctl13_ctl06_tcEdit_body" class="ajax__tab_body">
//    <div id="ctl13_ctl06_tcEdit_tpWebView" class="ajax__tab_panel" style="visibility: visible;">
//      <p>Stuff One content...</p>    		
//    </div>
//    <div id="ctl13_ctl06_tcEdit_tpBulletin" style="display:none;visibility:hidden;" class="ajax__tab_panel">
//      <p>Stuff Two content...</p>    		
//    </div>
//  </div>

// Looking for this result...
//  <ul id="ctl13_ctl06_tcEdit_header" class="nav nav-tabs">
//    <li>
//      <a href="#ctl13_ctl06_tcEdit_tpWebView" data-toggle="tab">Stuff One</a>
//    </li>
//    <li>
//      <a href="#ctl13_ctl06_tcEdit_tpBulletin" data-toggle="tab">Stuff Two</a>
//    </li>
//  </ul>
//  <div id="ctl13_ctl06_tcEdit_body" class="tab-content">
//    <div class="tab-pane fade" id="ctl13_ctl06_tcEdit_tpWebView">
//      <p>Stuff One content...</p>
//    </div>
//    <div class="tab-pane fade" id="ctl13_ctl06_tcEdit_tpBulletin">
//      <p>Stuff Two content...</p>
//    </div>
//  </div>